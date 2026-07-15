using System;
using System.Collections.Generic;
using System.Linq;
using Ascendex.Game.Content;
using Ascendex.Game.Save;

namespace Ascendex.Game;

public sealed class GameSession
{
    private readonly Dictionary<string, SpeciesBarConfig> _speciesBarConfigs = new(StringComparer.Ordinal);
    private readonly Dictionary<string, TrainerBarConfig> _trainerBarConfigs = new(StringComparer.Ordinal);
    private readonly Dictionary<string, (int Level, double Required)> _speciesProgressRequiredCache = new(StringComparer.Ordinal);
    private readonly Dictionary<string, (int Level, double Required)> _trainerProgressRequiredCache = new(StringComparer.Ordinal);
    private readonly Dictionary<string, double> _vitaminTrainingMultiplierCache = new(StringComparer.Ordinal);
    private SpeciesBarConfig[] _activeSpeciesBarConfigs = [];
    private TrainerBarConfig? _activeTrainerBarConfig;
    private double _simulationTickRemainder;
    private bool _speedCachesDirty = true;
    private double _cachedPokemonTrainingSpeed;
    private double _cachedPokemonCatchSpeed;
    private double _cachedBattleSpeed;
    private double _cachedBestBallMultiplier;
    private bool _cachedQualifiesForFirstCatchBonus;
    private int _lastNotifiedBankSeconds = int.MinValue;
    private readonly HashSet<string> _pendingSpeciesLevelChanges = new(StringComparer.Ordinal);
    private readonly HashSet<SpeciesProgress> _simulationSpeciesProgressChanged = [];
    private readonly HashSet<SpeciesProgress> _simulationSpeciesLevelChanged = [];
    private readonly HashSet<TrainerProgress> _simulationTrainerProgressChanged = [];
    private readonly HashSet<TrainerProgress> _simulationTrainerLevelChanged = [];
    private int _notificationBatchDepth;
    private bool _pendingTrainerLevelChanged;
    private bool _pendingTypeCountersChanged;
    private bool _pendingProgressionChanged;
    private bool _pendingShopStateChanged;

    public RunState State { get; } = new();

    public event Action<string>? SpeciesLevelChanged;

    public event Action? TrainerLevelChanged;

    public event Action? TypeCountersChanged;

    public event Action? ProgressionChanged;

    public event Action? ActiveBarsChanged;

    public event Action? BankTimeChanged;

    public event Action? ShopStateChanged;

    public static GameSession CreateNew()
    {
        var session = new GameSession();
        session.InitializeState();
        return session;
    }

    public static GameSession CreateFromSave(SaveGameData data)
    {
        var session = new GameSession();
        session.InitializeState();
        SaveGameMapper.ApplyToRunState(session.State, data);

        if (string.IsNullOrEmpty(session.State.SelectedRouteId))
        {
            session.State.SelectedRouteId = RouteIds.PalletTown;
        }

        session.NotifyActiveBarsChanged();
        session.RestoreCeladonAlternateLevelsIfUnlocked();
        return session;
    }

    // Offline catch-up is represented by bank time and consumed by Advance.
    //
    // /// <summary>Simulate ticks while the app was closed for any bar that was actively training or catching.</summary>
    // public void CatchUpOfflineProgress(DateTimeOffset savedAtUtc)
    // {
    //     if (!HasActiveBars())
    //     {
    //         return;
    //     }
    //
    //     var elapsed = DateTimeOffset.UtcNow - savedAtUtc;
    //     if (elapsed <= TimeSpan.Zero)
    //     {
    //         return;
    //     }
    //
    //     var msPerTick = GameBalance.Training.TickIntervalMilliseconds;
    //     var totalTicks = (long)(elapsed.TotalMilliseconds / msPerTick);
    //     var maxTicks = (long)(TimeSpan.FromHours(24).TotalMilliseconds / msPerTick);
    //     totalTicks = Math.Min(totalTicks, maxTicks);
    //
    //     for (var i = 0L; i < totalTicks; i++)
    //     {
    //         Tick();
    //     }
    //
    //     RestoreCeladonAlternateLevelsIfUnlocked();
    // }

    /// <summary>Credits offline elapsed time into bank (capped per period and total).</summary>
    public void ApplyOfflineBankTime(DateTimeOffset savedAtUtc)
    {
        var elapsed = DateTimeOffset.UtcNow - savedAtUtc;
        if (elapsed <= TimeSpan.Zero)
        {
            return;
        }

        var deposit = Math.Min(elapsed.TotalSeconds, GameBalance.SpeedBoost.MaxOfflineDepositSeconds);
        var newBank = Math.Min(State.BankTimeSeconds + deposit, GameBalance.SpeedBoost.MaxBankSeconds);
        if (Math.Abs(newBank - State.BankTimeSeconds) < double.Epsilon)
        {
            return;
        }

        State.BankTimeSeconds = newBank;
        NotifyBankTimeChanged(force: true);
    }

    /// <summary>Advances logical 16 ms ticks from monotonic foreground elapsed time.</summary>
    public void Advance(double realElapsedSeconds)
    {
        if (realElapsedSeconds <= 0 || !HasActiveBars())
        {
            return;
        }

        var boostedRealSeconds = State.BankTimeSeconds > 0
            ? Math.Min(realElapsedSeconds, State.BankTimeSeconds / GameBalance.SpeedBoost.Multiplier)
            : 0;
        var normalRealSeconds = realElapsedSeconds - boostedRealSeconds;
        var bankCost = boostedRealSeconds * GameBalance.SpeedBoost.Multiplier;
        if (boostedRealSeconds > 0)
        {
            State.BankTimeSeconds = Math.Max(
                0,
                State.BankTimeSeconds - bankCost);
            NotifyBankTimeChanged();
        }

        var logicalSeconds = normalRealSeconds + boostedRealSeconds * GameBalance.SpeedBoost.Multiplier;
        var totalTicks = logicalSeconds * 1000.0 / GameBalance.Training.TickIntervalMilliseconds
            + _simulationTickRemainder;
        var wholeTicks = (long)Math.Floor(totalTicks);
        _simulationTickRemainder = totalTicks - wholeTicks;
        if (wholeTicks > 0)
        {
            var processedTicks = AdvanceTicks(wholeTicks);
            if (processedTicks < wholeTicks && bankCost > 0)
            {
                var unusedTicks = wholeTicks - processedTicks;
                var normalLogicalTicks = normalRealSeconds * 1000.0
                    / GameBalance.Training.TickIntervalMilliseconds;
                var boostedLogicalTicks = boostedRealSeconds * GameBalance.SpeedBoost.Multiplier * 1000.0
                    / GameBalance.Training.TickIntervalMilliseconds;
                var unusedBoostedTicks = Math.Clamp(
                    unusedTicks - normalLogicalTicks,
                    0,
                    boostedLogicalTicks);
                var refund = unusedBoostedTicks * GameBalance.Training.TickIntervalMilliseconds / 1000.0;
                State.BankTimeSeconds = Math.Min(
                    GameBalance.SpeedBoost.MaxBankSeconds,
                    State.BankTimeSeconds + refund);
                NotifyBankTimeChanged(force: true);
            }
        }

        if (!HasActiveBars())
        {
            _simulationTickRemainder = 0;
        }
    }

    public void RestoreCeladonAlternateLevelsIfUnlocked()
    {
        if (!State.CeladonAlternateEeveelutionsUnlocked)
        {
            return;
        }

        GrantSpeciesLevelsWithTypePoints("Flareon", 25);
        GrantSpeciesLevelsWithTypePoints("Jolteon", 25);
    }

    public SpeciesBarConfig GetSpeciesBarConfig(string speciesRootName) => _speciesBarConfigs[speciesRootName];

    public TrainerBarConfig GetTrainerBarConfig(string trainerId) => _trainerBarConfigs[trainerId];

    public double GetSpeciesProgressRequired(SpeciesBarConfig config, int level)
    {
        if (_speciesProgressRequiredCache.TryGetValue(config.SpeciesRootName, out var cached)
            && cached.Level == level)
        {
            return cached.Required;
        }

        var required = TrainingSimulator.GetSpeciesProgressRequired(config, level);
        _speciesProgressRequiredCache[config.SpeciesRootName] = (level, required);
        return required;
    }

    public double GetTrainerProgressRequired(TrainerBarConfig config, int level)
    {
        if (_trainerProgressRequiredCache.TryGetValue(config.TrainerId, out var cached)
            && cached.Level == level)
        {
            return cached.Required;
        }

        var required = TrainingSimulator.GetTrainerProgressRequired(config, level);
        _trainerProgressRequiredCache[config.TrainerId] = (level, required);
        return required;
    }

    public SpeciesProgress GetSpecies(string speciesRootName) => State.SpeciesByRoot[speciesRootName];

    public TrainerProgress GetTrainer(string trainerId) => State.TrainersById[trainerId];

    public TrainerProgress? GetActiveTrainerProgress() =>
        _activeTrainerBarConfig is null ? null : GetTrainer(_activeTrainerBarConfig.TrainerId);

    public int GetTypeCounterCount(string typeKey) =>
        State.TypeCounterCounts.TryGetValue(typeKey, out var count) ? count : 0;

    public void SelectRoute(string routeId) => State.SelectedRouteId = routeId;

    public void Tick() => AdvanceTicks(1);

    private long AdvanceTicks(long tickCount)
    {
        var processedTicks = 0L;
        _simulationSpeciesProgressChanged.Clear();
        _simulationSpeciesLevelChanged.Clear();
        _simulationTrainerProgressChanged.Clear();
        _simulationTrainerLevelChanged.Clear();
        BeginNotificationBatch();
        try
        {
            while (tickCount-- > 0 && HasActiveBars())
            {
                processedTicks++;
                var activeSpecies = _activeSpeciesBarConfigs;
                foreach (var config in activeSpecies)
                {
                    var progress = GetSpecies(config.SpeciesRootName);
                    var change = TrainingSimulator.TickSpeciesDeferred(this, progress, config);
                    if (change.ProgressChanged)
                    {
                        _simulationSpeciesProgressChanged.Add(progress);
                    }

                    if (change.LevelChanged)
                    {
                        _simulationSpeciesLevelChanged.Add(progress);
                    }
                }

                var trainerConfig = _activeTrainerBarConfig;
                if (trainerConfig is not null)
                {
                    var progress = GetTrainer(trainerConfig.TrainerId);
                    var change = TrainingSimulator.TickTrainerDeferred(this, progress, trainerConfig);
                    if (change.ProgressChanged)
                    {
                        _simulationTrainerProgressChanged.Add(progress);
                    }

                    if (change.LevelChanged)
                    {
                        _simulationTrainerLevelChanged.Add(progress);
                    }
                }
            }
        }
        finally
        {
            PublishSimulationChanges();
            EndNotificationBatch();
        }

        return processedTicks;
    }

    private void PublishSimulationChanges()
    {
        foreach (var progress in _simulationSpeciesProgressChanged)
        {
            progress.PublishSimulationChanges(
                _simulationSpeciesLevelChanged.Contains(progress),
                progressChanged: true);
        }

        foreach (var progress in _simulationSpeciesLevelChanged)
        {
            if (!_simulationSpeciesProgressChanged.Contains(progress))
            {
                progress.PublishSimulationChanges(levelChanged: true, progressChanged: false);
            }
        }

        foreach (var progress in _simulationTrainerProgressChanged)
        {
            progress.PublishSimulationChanges(
                _simulationTrainerLevelChanged.Contains(progress),
                progressChanged: true);
        }

        foreach (var progress in _simulationTrainerLevelChanged)
        {
            if (!_simulationTrainerProgressChanged.Contains(progress))
            {
                progress.PublishSimulationChanges(levelChanged: true, progressChanged: false);
            }
        }

        _simulationSpeciesProgressChanged.Clear();
        _simulationSpeciesLevelChanged.Clear();
        _simulationTrainerProgressChanged.Clear();
        _simulationTrainerLevelChanged.Clear();
    }

    public bool HasActiveBars() => _activeSpeciesBarConfigs.Length > 0 || _activeTrainerBarConfig is not null;

    public void ToggleSpeciesActivity(string speciesRootName, bool catchMode)
    {
        var selected = GetSpecies(speciesRootName);
        if (catchMode)
        {
            if (!selected.IsCatching && selected.Level == 0)
            {
                ClearSpeciesCatchingExcept(speciesRootName);
                selected.IsCatching = true;
            }
            else
            {
                selected.IsCatching = false;
            }

            NotifyActiveBarsChanged();
            return;
        }

        if (selected.IsTraining)
        {
            DeactivateSpeciesTraining(speciesRootName);
            NotifyActiveBarsChanged();
            return;
        }

        ActivateSpeciesTraining(speciesRootName);
        NotifyActiveBarsChanged();
    }

    public void ToggleTrainerActivity(string trainerId)
    {
        var selected = GetTrainer(trainerId);
        if (selected.IsTraining)
        {
            selected.IsTraining = false;
            NotifyActiveBarsChanged();
            return;
        }

        foreach (var trainer in State.TrainersById.Values)
        {
            trainer.IsTraining = trainer.TrainerId == trainerId;
        }

        NotifyActiveBarsChanged();
    }

    internal void NotifySpeciesLevelChanged(string speciesRootName)
    {
        _speedCachesDirty = true;
        var progress = GetSpecies(speciesRootName);
        if (progress.Level >= 1 && progress.IsCatching)
        {
            progress.IsCatching = false;
            NotifyActiveBarsChanged();
        }

        TryUnlockCeladonAlternateEeveelutions(speciesRootName);
        if (_notificationBatchDepth > 0)
        {
            _pendingSpeciesLevelChanges.Add(speciesRootName);
            _pendingProgressionChanged = true;
            return;
        }

        SpeciesLevelChanged?.Invoke(speciesRootName);
        ProgressionChanged?.Invoke();
    }

    internal void NotifyTrainerLevelChanged(string trainerId)
    {
        _speedCachesDirty = true;
        GrantPokedollarsForTrainerClear(trainerId);
        if (_notificationBatchDepth > 0)
        {
            _pendingTrainerLevelChanged = true;
            _pendingProgressionChanged = true;
            _pendingShopStateChanged = true;
            return;
        }

        TrainerLevelChanged?.Invoke();
        ProgressionChanged?.Invoke();
        ShopStateChanged?.Invoke();
    }

    public void GrantPokedollarsForTrainerClear(string trainerId)
    {
        var trainerIndex = 0;
        foreach (var trainer in KantoTrainerCatalog.All)
        {
            if (trainer.Id == trainerId)
            {
                var clearCount = GetTrainer(trainerId).Level;
                State.Pokedollars += ShopRules.DollarsForTrainerClear(trainerIndex, clearCount);
                return;
            }

            trainerIndex++;
        }
    }

    public void RecordTypeLevelContributions(TypeLevelContribution[] contributions)
    {
        var any = false;
        foreach (var contribution in contributions)
        {
            if (contribution.Points == 0)
            {
                continue;
            }

            any = true;
            if (!State.TypeCounterCounts.TryGetValue(contribution.TypeKey, out var count))
            {
                count = 0;
            }

            State.TypeCounterCounts[contribution.TypeKey] = count + contribution.Points;
        }

        if (any)
        {
            _speedCachesDirty = true;
            if (_notificationBatchDepth > 0)
            {
                _pendingTypeCountersChanged = true;
            }
            else
            {
                TypeCountersChanged?.Invoke();
            }
        }
    }

    public void GrantSpeciesLevelsWithTypePoints(string speciesRootName, int targetLevel)
    {
        var progress = GetSpecies(speciesRootName);
        var config = GetSpeciesBarConfig(speciesRootName);
        BeginNotificationBatch();
        try
        {
            TrainingSimulator.GrantSpeciesLevelsWithTypePoints(this, progress, config, targetLevel);
        }
        finally
        {
            EndNotificationBatch();
        }
    }

    public bool QualifiesForFirstCatchSpeedBonus()
    {
        EnsureSpeedCaches();
        return _cachedQualifiesForFirstCatchBonus;
    }

    public double GetBattleSpeedFromTypeLevels()
    {
        EnsureSpeedCaches();
        return _cachedBattleSpeed;
    }

    public double GetPokemonTrainingSpeedFromBattleClears()
    {
        EnsureSpeedCaches();
        return _cachedPokemonTrainingSpeed;
    }

    public double GetPokemonCatchSpeedFromBattleClears()
    {
        EnsureSpeedCaches();
        return _cachedPokemonCatchSpeed;
    }

    public double GetBestOwnedBallCatchMultiplier()
    {
        EnsureSpeedCaches();
        return _cachedBestBallMultiplier;
    }

    public double GetVitaminTrainingMultiplier(string speciesRootName)
    {
        if (_vitaminTrainingMultiplierCache.TryGetValue(speciesRootName, out var multiplier))
        {
            return multiplier;
        }

        multiplier = ShopRules.VitaminTrainingMultiplier(State, speciesRootName);
        _vitaminTrainingMultiplierCache[speciesRootName] = multiplier;
        return multiplier;
    }

    public bool IsShopVisible(string shopId) => ShopRules.IsShopVisible(State, shopId);

    public bool IsShopTabUnlocked() => ShopRules.IsShopTabUnlocked(State);

    public bool TryPurchaseShopItem(string itemId)
    {
        var item = KantoShopCatalog.FindItem(itemId);
        if (item is null || !ShopRules.IsItemUnlocked(State, item.Value.Id))
        {
            return false;
        }

        if (!ShopRules.TryPurchase(State, item.Value))
        {
            return false;
        }

        _speedCachesDirty = true;
        ShopStateChanged?.Invoke();
        return true;
    }

    public int TryBuyAllVitamins()
    {
        var bought = ShopRules.TryBuyAllVitamins(State);
        if (bought > 0)
        {
            ShopStateChanged?.Invoke();
        }

        return bought;
    }

    public bool TryApplyVitamin(string speciesRootName)
    {
        if (!ShopRules.TryApplyVitamin(State, speciesRootName))
        {
            return false;
        }

        _vitaminTrainingMultiplierCache.Remove(speciesRootName);
        ShopStateChanged?.Invoke();
        return true;
    }

    public int TryApplyMaxVitamins(string speciesRootName)
    {
        var applied = ShopRules.TryApplyMaxVitamins(State, speciesRootName);
        if (applied > 0)
        {
            _vitaminTrainingMultiplierCache.Remove(speciesRootName);
            ShopStateChanged?.Invoke();
        }

        return applied;
    }

    public bool CanChampionReset() =>
        GetTrainer(TrainerIds.Blue).Level >= GameBalance.Battles.MinTrainerLevelToRevealNextBattle;

    public bool CanPokedexReset() => CountCaughtPokedexEntries() >= KantoSpeciesCatalog.NationalDexCellCount;

    public bool PerformChampionReset()
    {
        if (!CanChampionReset())
        {
            return false;
        }

        State.ChampionResetUnlocked = true;
        State.ChampionResetCount++;
        State.ExpShareCount = ComputeExpShareCount(State.ChampionResetCount);
        ResetRunProgressForPrestige();
        return true;
    }

    public bool PerformPokedexReset()
    {
        if (!CanPokedexReset())
        {
            return false;
        }

        State.PokedexResetCount++;
        State.ShinyCharmCount++;
        ResetRunProgressForPrestige();
        return true;
    }

    public bool IsRouteVisible(string routeId) => ProgressionRules.IsRouteVisible(State, routeId);

    public bool IsTrainerVisible(string trainerId) => ProgressionRules.IsTrainerVisible(State, trainerId);

    public bool AnySpeciesCatching() => State.SpeciesByRoot.Values.Any(progress => progress.IsCatching);

    private int CountCaughtPokedexEntries() =>
        PokedexRules.GetFilledCells(State).Select(fill => fill.CellIndex).Distinct().Count();

    private static int ComputeExpShareCount(int championResetCount)
    {
        if (championResetCount <= 0)
        {
            return 0;
        }

        var shares = 0;
        var required = 1;
        var spent = 0;
        while (spent + required <= championResetCount)
        {
            spent += required;
            shares++;
            required++;
        }

        return shares;
    }

    private void ResetRunProgressForPrestige()
    {
        foreach (var species in State.SpeciesByRoot.Values)
        {
            species.Level = 0;
            species.Progress = 0;
            species.IsTraining = false;
            species.IsCatching = false;
            species.IsShiny = false;
            species.IsVisible = !StartsHiddenBySpeciesRoot(species.SpeciesRootName);
        }

        foreach (var trainer in State.TrainersById.Values)
        {
            trainer.Level = 0;
            trainer.Progress = 0;
            trainer.IsTraining = false;
            trainer.IsVisible = true;
        }

        foreach (var typeKey in TypeCatalog.CounterTypeKeys)
        {
            State.TypeCounterCounts[typeKey] = 0;
        }

        State.SelectedRouteId = RouteIds.PalletTown;
        State.CeladonAlternateEeveelutionsUnlocked = false;
        State.SpeciesTrainingOrder.Clear();
        State.Pokedollars = 0;
        State.OwnedShopItemIds.Clear();
        // UnassignedVitaminCount, VitaminApplySectionUnlocked, and VitaminDosesBySpeciesRoot persist across prestige.

        InvalidateRuntimeCaches();
        SpeciesLevelChanged?.Invoke(string.Empty);
        TrainerLevelChanged?.Invoke();
        TypeCountersChanged?.Invoke();
        ProgressionChanged?.Invoke();
        NotifyActiveBarsChanged();
        NotifyBankTimeChanged(force: true);
        ShopStateChanged?.Invoke();
    }

    private static bool StartsHiddenBySpeciesRoot(string speciesRootName)
    {
        foreach (var route in KantoRouteCatalog.All)
        {
            foreach (var spawn in route.Spawns)
            {
                if (spawn.SpeciesRootName == speciesRootName)
                {
                    return spawn.StartsHidden;
                }
            }
        }

        return false;
    }

    private double RouteGymTrainingSpeedMultiplier =>
        SpeedMultiplierFromBattleClears(
            GameBalance.Battles.RouteTrainingBonusPerClearByTrainerIndex,
            GameBalance.Battles.RouteTrainingSpeedMultiplierBaseline,
            GameBalance.Battles.RouteTrainingSpeedMultiplierCap);

    private double SpeedMultiplierFromBattleClears(double[] weightsPerTrainer, double baseline, double cap)
    {
        var bonus = 0.0;
        var trainerIndex = 0;
        foreach (var trainer in KantoTrainerCatalog.All)
        {
            var clears = Math.Max(0, GetTrainer(trainer.Id).Level);
            bonus += clears * BonusWeightForTrainer(weightsPerTrainer, trainerIndex);
            trainerIndex++;
        }

        return Math.Min(baseline + bonus, cap);
    }

    private void EnsureSpeedCaches()
    {
        if (!_speedCachesDirty)
        {
            return;
        }

        _cachedPokemonTrainingSpeed = SpeedMultiplierFromBattleClears(
            GameBalance.Battles.RouteTrainingBonusPerClearByTrainerIndex,
            GameBalance.Battles.RouteTrainingSpeedMultiplierBaseline,
            GameBalance.Battles.RouteTrainingSpeedMultiplierCap);
        var gymBonusAboveBaseline = Math.Max(
            0,
            _cachedPokemonTrainingSpeed - GameBalance.Battles.RouteTrainingSpeedMultiplierBaseline);
        _cachedPokemonCatchSpeed = GameBalance.Battles.RouteTrainingSpeedMultiplierBaseline
            + gymBonusAboveBaseline * GameBalance.Battles.RouteCatchFractionOfTrainingGymBonus;

        var typeLevelSum = State.TypeCounterCounts.Values.Sum();
        var battleMultiplier = GameBalance.Battles.BattleSpeedMultiplierBaseline
            + typeLevelSum * GameBalance.Battles.BattleSpeedBonusPerTotalTypeLevel;
        battleMultiplier *= ShopRules.XItemBattleMultiplier(State);
        _cachedBattleSpeed = Math.Min(battleMultiplier, GameBalance.Battles.BattleSpeedMultiplierCap);
        _cachedBestBallMultiplier = ShopRules.BestOwnedBallCatchMultiplier(State);
        _cachedQualifiesForFirstCatchBonus = !State.SpeciesByRoot.Values.Any(progress =>
            progress.Level >= GameBalance.Routes.MinPokemonLevelToPassRoute);
        _speedCachesDirty = false;
    }

    private void InvalidateRuntimeCaches()
    {
        _speciesProgressRequiredCache.Clear();
        _trainerProgressRequiredCache.Clear();
        _vitaminTrainingMultiplierCache.Clear();
        _speedCachesDirty = true;
        _simulationTickRemainder = 0;
    }

    private void BeginNotificationBatch() => _notificationBatchDepth++;

    private void EndNotificationBatch()
    {
        _notificationBatchDepth--;
        if (_notificationBatchDepth > 0)
        {
            return;
        }

        if (_notificationBatchDepth < 0)
        {
            _notificationBatchDepth = 0;
            throw new InvalidOperationException("Notification batch depth became unbalanced.");
        }

        var speciesChanges = _pendingSpeciesLevelChanges.ToArray();
        var trainerChanged = _pendingTrainerLevelChanged;
        var typeCountersChanged = _pendingTypeCountersChanged;
        var progressionChanged = _pendingProgressionChanged;
        var shopChanged = _pendingShopStateChanged;
        _pendingSpeciesLevelChanges.Clear();
        _pendingTrainerLevelChanged = false;
        _pendingTypeCountersChanged = false;
        _pendingProgressionChanged = false;
        _pendingShopStateChanged = false;

        if (typeCountersChanged)
        {
            TypeCountersChanged?.Invoke();
        }

        foreach (var speciesRoot in speciesChanges)
        {
            SpeciesLevelChanged?.Invoke(speciesRoot);
        }

        if (trainerChanged)
        {
            TrainerLevelChanged?.Invoke();
        }

        if (progressionChanged)
        {
            ProgressionChanged?.Invoke();
        }

        if (shopChanged)
        {
            ShopStateChanged?.Invoke();
        }
    }

    private static double BonusWeightForTrainer(double[] weights, int trainerIndexZeroBased)
    {
        if (weights.Length == 0 || trainerIndexZeroBased < 0)
        {
            return 0;
        }

        if (trainerIndexZeroBased >= weights.Length)
        {
            return weights[^1];
        }

        return weights[trainerIndexZeroBased];
    }

    private void InitializeState()
    {
        foreach (var route in KantoRouteCatalog.All)
        {
            foreach (var spawn in route.Spawns)
            {
                if (!State.SpeciesByRoot.ContainsKey(spawn.SpeciesRootName))
                {
                    State.SpeciesByRoot[spawn.SpeciesRootName] = new SpeciesProgress
                    {
                        SpeciesRootName = spawn.SpeciesRootName,
                        IsVisible = !spawn.StartsHidden,
                    };
                }

                if (_speciesBarConfigs.ContainsKey(spawn.SpeciesRootName))
                {
                    continue;
                }

                var catchMultiplier = spawn.IsBoss ? GameBalance.Routes.BossCatchDifficultyMultiplier : 1.0;
                _speciesBarConfigs[spawn.SpeciesRootName] = new SpeciesBarConfig
                {
                    SpeciesRootName = spawn.SpeciesRootName,
                    BaseProgressRequired = GameBalance.Training.DefaultBaseProgressRequired,
                    ProgressRequiredPerLevelExponent = GameBalance.Training.RoutePokemonProgressRequiredPerLevelExponent,
                    CatchDifficultyMultiplier = catchMultiplier,
                    AllowsCatching = spawn.AllowsCatching,
                    EvolutionChain = KantoSpeciesCatalog.TryGetEvolutionChain(spawn.SpeciesRootName),
                };
            }
        }

        for (var i = 0; i < KantoTrainerCatalog.All.Length; i++)
        {
            var trainer = KantoTrainerCatalog.All[i];
            State.TrainersById[trainer.Id] = new TrainerProgress
            {
                TrainerId = trainer.Id,
            };

            var baseRequired = GameBalance.Battles.FirstTrainerBaseProgress
                * Math.Pow(GameBalance.Battles.PerTrainerDifficultyStep, i);
            _trainerBarConfigs[trainer.Id] = new TrainerBarConfig
            {
                TrainerId = trainer.Id,
                BaseProgressRequired = baseRequired,
                ProgressRequiredPerLevelExponent = GameBalance.Battles.BattleProgressRequiredPerLevelExponent,
            };
        }

        foreach (var typeKey in TypeCatalog.CounterTypeKeys)
        {
            State.TypeCounterCounts[typeKey] = 0;
        }

        State.SelectedRouteId = RouteIds.PalletTown;
    }

    private int MaxConcurrentSpeciesTraining => 1 + State.ExpShareCount;

    private void ActivateSpeciesTraining(string speciesRootName)
    {
        var queue = State.SpeciesTrainingOrder;
        if (queue.Contains(speciesRootName))
        {
            return;
        }

        while (queue.Count >= MaxConcurrentSpeciesTraining)
        {
            var oldest = queue[0];
            queue.RemoveAt(0);
            if (State.SpeciesByRoot.TryGetValue(oldest, out var displaced))
            {
                displaced.IsTraining = false;
            }
        }

        queue.Add(speciesRootName);
        GetSpecies(speciesRootName).IsTraining = true;
    }

    private void DeactivateSpeciesTraining(string speciesRootName)
    {
        State.SpeciesTrainingOrder.Remove(speciesRootName);
        GetSpecies(speciesRootName).IsTraining = false;
    }

    private void ClearSpeciesCatchingExcept(string speciesRootName)
    {
        foreach (var progress in State.SpeciesByRoot.Values)
        {
            if (progress.SpeciesRootName != speciesRootName)
            {
                progress.IsCatching = false;
            }
        }
    }

    private void NotifyActiveBarsChanged()
    {
        var activeSpecies = new List<SpeciesBarConfig>();
        foreach (var config in _speciesBarConfigs.Values)
        {
            var progress = GetSpecies(config.SpeciesRootName);
            if (progress.IsTraining || progress.IsCatching)
            {
                activeSpecies.Add(config);
            }
        }

        _activeSpeciesBarConfigs = activeSpecies.ToArray();
        _activeTrainerBarConfig = null;
        foreach (var config in _trainerBarConfigs.Values)
        {
            if (GetTrainer(config.TrainerId).IsTraining)
            {
                _activeTrainerBarConfig = config;
                break;
            }
        }

        ActiveBarsChanged?.Invoke();
    }

    private void NotifyBankTimeChanged(bool force = false)
    {
        var displayedSeconds = (int)Math.Ceiling(Math.Max(0, State.BankTimeSeconds));
        if (!force && displayedSeconds == _lastNotifiedBankSeconds)
        {
            return;
        }

        _lastNotifiedBankSeconds = displayedSeconds;
        BankTimeChanged?.Invoke();
    }

    /// <summary>Celadon: Flareon and Jolteon stay hidden until Eevee's bar reaches Vaporeon (level 25).</summary>
    private void TryUnlockCeladonAlternateEeveelutions(string speciesRootName)
    {
        if (State.CeladonAlternateEeveelutionsUnlocked || speciesRootName != "Eevee")
        {
            return;
        }

        var chain = KantoSpeciesCatalog.TryGetEvolutionChain("Eevee");
        if (chain is not { Length: >= 2 })
        {
            return;
        }

        if (GetSpecies("Eevee").Level < chain[1].MinLevel)
        {
            return;
        }

        State.CeladonAlternateEeveelutionsUnlocked = true;

        if (State.SpeciesByRoot.TryGetValue("Flareon", out var flareon))
        {
            flareon.IsVisible = true;
        }

        if (State.SpeciesByRoot.TryGetValue("Jolteon", out var jolteon))
        {
            jolteon.IsVisible = true;
        }

        GrantSpeciesLevelsWithTypePoints("Flareon", 25);
        GrantSpeciesLevelsWithTypePoints("Jolteon", 25);
    }
}
