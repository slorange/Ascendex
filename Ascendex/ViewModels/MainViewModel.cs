using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Ascendex.Game.Content;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;

namespace Ascendex.ViewModels;

public class MainViewModel : ViewModelBase
{
    private static readonly IBrush MainTabSelectedBrush = Brush.Parse(MagicNumbersUI.Tabs.MainTabSelectedBackground);
    private static readonly IBrush MainTabUnselectedBrush = Brush.Parse(MagicNumbersUI.Tabs.MainTabUnselectedBackground);

    private readonly List<PokemonTrainingBarViewModel> _allPokemonBars;
    private readonly List<PokemonTrainingBarViewModel> _allBattleBars;
    private readonly Dictionary<string, TypeCounterViewModel> _typeCountersByKey;
    private Dictionary<string, AreaSelectionViewModel> _areasByRouteId = null!;
    private Dictionary<string, PokemonTrainingBarViewModel> _trainersById = null!;
    private PokemonTrainingBarViewModel? _celadonFlareonBar;
    private PokemonTrainingBarViewModel? _celadonJolteonBar;
    private bool _celadonAlternateEeveelutionsUnlocked;
    private string _currentAreaName = string.Empty;
    private int _selectedAreaIndex;
    private int _selectedMainTab;
    private PokemonTrainingBarViewModel? _battlesTabTrackedBar;
    private double _battlesTabProgressFraction;
    private bool _hasBattlesTabProgressIndicator;
    private IBrush _battlesTabProgressAccentBrush = Brushes.Transparent;

    public MainViewModel()
    {
        SelectRoutesTabCommand = new RelayCommand(() => SelectedMainTab = 0);
        SelectBattlesTabCommand = new RelayCommand(() => SelectedMainTab = 1);
        SelectCollectionsTabCommand = new RelayCommand(() => SelectedMainTab = 2);

        TypeCounters = new ObservableCollection<TypeCounterViewModel>(
            TypeCatalog.CounterTypeKeys.Select(key => new TypeCounterViewModel(key)));

        _typeCountersByKey = new Dictionary<string, TypeCounterViewModel>();
        foreach (var counter in TypeCounters)
        {
            _typeCountersByKey[counter.TypeKey] = counter;
        }

        PokemonBars = new ObservableCollection<PokemonTrainingBarViewModel>();
        BattleBars = new ObservableCollection<PokemonTrainingBarViewModel>();
        AreaSelectors = new ObservableCollection<AreaSelectionViewModel>();
        _allPokemonBars = new List<PokemonTrainingBarViewModel>();
        _allBattleBars = new List<PokemonTrainingBarViewModel>();

        InitializeRoutes();
        InitializeBattles();
        BuildProgressionLookups();
        UpdateProgressionVisibility();
        SelectArea(AreaSelectors[0]);
        InitializePokedex();
    }

    public IRelayCommand SelectRoutesTabCommand { get; }

    public IRelayCommand SelectBattlesTabCommand { get; }

    public IRelayCommand SelectCollectionsTabCommand { get; }

    public int SelectedMainTab
    {
        get => _selectedMainTab;
        set
        {
            if (SetProperty(ref _selectedMainTab, value))
            {
                OnPropertyChanged(nameof(IsRoutesTabSelected));
                OnPropertyChanged(nameof(IsBattlesTabSelected));
                OnPropertyChanged(nameof(IsCollectionsTabSelected));
                OnPropertyChanged(nameof(RoutesTabBackground));
                OnPropertyChanged(nameof(BattlesTabBackground));
                OnPropertyChanged(nameof(CollectionsTabBackground));
            }
        }
    }

    public bool IsRoutesTabSelected => _selectedMainTab == 0;

    public bool IsBattlesTabSelected => _selectedMainTab == 1;

    public bool IsCollectionsTabSelected => _selectedMainTab == 2;

    public IBrush RoutesTabBackground => _selectedMainTab == 0 ? MainTabSelectedBrush : MainTabUnselectedBrush;

    public IBrush BattlesTabBackground => _selectedMainTab == 1 ? MainTabSelectedBrush : MainTabUnselectedBrush;

    public IBrush CollectionsTabBackground => _selectedMainTab == 2 ? MainTabSelectedBrush : MainTabUnselectedBrush;

    public bool ShowRoutesTabPokeballIcon => !_allPokemonBars.Any(b => b.IsCatching);

    public bool HasBattlesTabProgressIndicator
    {
        get => _hasBattlesTabProgressIndicator;
        private set => SetProperty(ref _hasBattlesTabProgressIndicator, value);
    }

    public double BattlesTabProgressFraction
    {
        get => _battlesTabProgressFraction;
        private set => SetProperty(ref _battlesTabProgressFraction, value);
    }

    public IBrush BattlesTabProgressAccentBrush
    {
        get => _battlesTabProgressAccentBrush;
        private set => SetProperty(ref _battlesTabProgressAccentBrush, value);
    }

    public ObservableCollection<PokemonTrainingBarViewModel> PokemonBars { get; }

    public ObservableCollection<PokemonTrainingBarViewModel> BattleBars { get; }

    public ObservableCollection<AreaSelectionViewModel> AreaSelectors { get; }

    public ObservableCollection<TypeCounterViewModel> TypeCounters { get; }

    public ObservableCollection<PokedexCellViewModel> PokedexCells { get; } = new();

    public string CurrentAreaName
    {
        get => _currentAreaName;
        private set => SetProperty(ref _currentAreaName, value);
    }

    /// <summary>Index of the selected area in <see cref="AreaSelectors"/>; used to keep the route strip centered on the current location.</summary>
    public int SelectedAreaIndex
    {
        get => _selectedAreaIndex;
        private set => SetProperty(ref _selectedAreaIndex, value);
    }

    private void InitializeRoutes()
    {
        foreach (var route in KantoRouteCatalog.All)
        {
            var bars = new List<PokemonTrainingBarViewModel>();
            foreach (var spawn in route.Spawns)
            {
                var catchMultiplier = spawn.IsBoss ? GameBalance.Routes.BossCatchDifficultyMultiplier : 1.0;
                var bar = CreatePokemon(spawn.SpeciesRootName, catchDifficultyMultiplier: catchMultiplier, allowsCatching: spawn.AllowsCatching);
                if (spawn.StartsHidden)
                {
                    bar.IsVisible = false;
                }

                if (spawn.SpeciesRootName == "Flareon")
                {
                    _celadonFlareonBar = bar;
                }
                else if (spawn.SpeciesRootName == "Jolteon")
                {
                    _celadonJolteonBar = bar;
                }

                bars.Add(bar);
            }

            AddArea(route.Id, route.ShortLabel, route.DisplayName, bars.ToArray());
        }
    }

    private void AddArea(
        string routeId,
        string shortLabel,
        string displayName,
        params PokemonTrainingBarViewModel[] pokemonBars)
    {
        foreach (var pokemonBar in pokemonBars)
        {
            _allPokemonBars.Add(pokemonBar);
        }

        AreaSelectors.Add(new AreaSelectionViewModel(routeId, shortLabel, displayName, pokemonBars, SelectArea));
    }

    private PokemonTrainingBarViewModel CreatePokemon(
        string speciesRootName,
        double progressRequired = GameBalance.Training.DefaultBaseProgressRequired,
        double catchDifficultyMultiplier = 1.0,
        bool allowsCatching = true)
    {
        var typeKey = KantoSpeciesCatalog.PrimaryTypeKey(speciesRootName);
        var palette = KantoSpeciesCatalog.ResolveRouteBarPalette(speciesRootName);
        var evolutionChain = KantoSpeciesCatalog.TryGetEvolutionChain(speciesRootName);

        return new PokemonTrainingBarViewModel(
            speciesRootName,
            typeKey,
            palette.AccentColor,
            palette.ForegroundColor,
            ToggleTraining,
            OnPokemonLevelChanged,
            RecordTypeLevelContributions,
            progressRequired,
            GameBalance.Training.RoutePokemonProgressRequiredPerLevelExponent,
            getTrainingProgressMultiplier: GetPokemonTrainingSpeedFromBattleClears,
            evolutionChain: evolutionChain,
            allowsCatching: allowsCatching,
            qualifiesForFirstCatchSpeedBonus: QualifiesForFirstCatchSpeedBonus,
            getCatchProgressMultiplier: GetPokemonCatchSpeedFromBattleClears,
            catchDifficultyMultiplier: catchDifficultyMultiplier);
    }

    private bool QualifiesForFirstCatchSpeedBonus() =>
        !_allPokemonBars.Any(b => b.Level >= GameBalance.Routes.MinPokemonLevelToPassRoute);

    /// <summary>Celadon: Flareon and Jolteon stay hidden until Eevee's bar reaches Vaporeon (level 25).</summary>
    private void TryUnlockCeladonAlternateEeveelutions(PokemonTrainingBarViewModel pokemonBar)
    {
        if (_celadonAlternateEeveelutionsUnlocked
            || _celadonFlareonBar is null
            || _celadonJolteonBar is null
            || pokemonBar.SpeciesLineRoot != "Eevee")
        {
            return;
        }

        var chain = KantoSpeciesCatalog.TryGetEvolutionChain("Eevee");
        if (chain is not { Length: >= 2 })
        {
            return;
        }

        if (pokemonBar.Level < chain[1].MinLevel)
        {
            return;
        }

        _celadonAlternateEeveelutionsUnlocked = true;
        _celadonFlareonBar.IsVisible = true;
        _celadonJolteonBar.IsVisible = true;
        _celadonFlareonBar.GrantAtLevel(25);
        _celadonJolteonBar.GrantAtLevel(25);
    }

    private void SelectArea(AreaSelectionViewModel selectedArea)
    {
        foreach (var area in AreaSelectors)
        {
            area.IsSelected = area == selectedArea;
        }

        var newIndex = AreaSelectors.IndexOf(selectedArea);
        if (_selectedAreaIndex != newIndex)
        {
            SelectedAreaIndex = newIndex;
        }
        else
        {
            // Same index: SetProperty would not notify; the route strip still needs to re-center on tap.
            OnPropertyChanged(nameof(SelectedAreaIndex));
        }

        CurrentAreaName = selectedArea.DisplayName;
        PokemonBars.Clear();

        foreach (var pokemonBar in selectedArea.PokemonBars)
        {
            PokemonBars.Add(pokemonBar);
        }
    }

    private void ToggleTraining(PokemonTrainingBarViewModel selectedBar)
    {
        if (selectedBar.CanCatch)
        {
            ToggleCatching(selectedBar);
            return;
        }

        ToggleRouteTraining(selectedBar);
    }

    private void ToggleCatching(PokemonTrainingBarViewModel selectedBar)
    {
        if (selectedBar.IsCatching)
        {
            selectedBar.SetCatching(false);
            RefreshRoutesTabCatchIndicator();
            return;
        }

        foreach (var bar in _allPokemonBars)
        {
            if (bar != selectedBar && bar.IsCatching)
            {
                bar.SetCatching(false);
            }
        }

        selectedBar.SetCatching(true);
        RefreshRoutesTabCatchIndicator();
    }

    private void RefreshRoutesTabCatchIndicator() => OnPropertyChanged(nameof(ShowRoutesTabPokeballIcon));

    private void ToggleRouteTraining(PokemonTrainingBarViewModel selectedBar)
    {
        if (selectedBar.IsTraining)
        {
            selectedBar.SetTraining(false);
            return;
        }

        foreach (var bar in _allPokemonBars)
        {
            if (bar != selectedBar && bar.IsTraining)
            {
                bar.SetTraining(false);
            }
        }

        selectedBar.SetTraining(true);
    }

    private void InitializeBattles()
    {
        for (var i = 0; i < KantoTrainerCatalog.All.Length; i++)
        {
            var trainer = KantoTrainerCatalog.All[i];
            var baseRequired = ComputeBattleBaseProgressRequired(i + 1);
            AddBattle(trainer.Id, trainer.DisplayName, trainer.TypeKey, baseRequired);
        }
    }

    private void BuildProgressionLookups()
    {
        _areasByRouteId = AreaSelectors.ToDictionary(a => a.RouteId);
        _trainersById = _allBattleBars.ToDictionary(b => b.TrainerId!);
    }

    /// <summary>Later trainers need more progress per cycle (each bar still scales further with <see cref="PokemonTrainingBarViewModel.ProgressRequired"/> by level).</summary>
    private static double ComputeBattleBaseProgressRequired(int battleOrderOneBased)
    {
        return GameBalance.Battles.FirstTrainerBaseProgress
            * Math.Pow(GameBalance.Battles.PerTrainerDifficultyStep, battleOrderOneBased - 1);
    }

    private void AddBattle(string trainerId, string displayName, string typeKey, double progressRequired)
    {
        var palette = KantoSpeciesCatalog.ResolveTrainerBarPalette(typeKey);
        var bar = new PokemonTrainingBarViewModel(
            displayName,
            typeKey,
            palette.AccentColor,
            palette.ForegroundColor,
            ToggleBattleTraining,
            OnBattleLevelChanged,
            static (Game.TypeLevelContribution[] _) => { },
            progressRequired,
            GameBalance.Battles.BattleProgressRequiredPerLevelExponent,
            getTrainingProgressMultiplier: GetBattleSpeedFromTypeLevels,
            trainerId: trainerId);
        _allBattleBars.Add(bar);
        BattleBars.Add(bar);
    }

    private void ToggleBattleTraining(PokemonTrainingBarViewModel selectedBar)
    {
        if (selectedBar.IsTraining)
        {
            selectedBar.SetTraining(false);
            RefreshBattlesTabProgressTracking();
            return;
        }

        foreach (var bar in _allBattleBars)
        {
            if (bar != selectedBar && bar.IsTraining)
            {
                bar.SetTraining(false);
            }
        }

        selectedBar.SetTraining(true);
        RefreshBattlesTabProgressTracking();
    }

    private void OnBattlesTabTrackedBarPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PokemonTrainingBarViewModel.Progress)
            or nameof(PokemonTrainingBarViewModel.ProgressFraction)
            or nameof(PokemonTrainingBarViewModel.AccentBrush))
        {
            UpdateBattlesTabProgressBindings();
        }
    }

    private void RefreshBattlesTabProgressTracking()
    {
        if (_battlesTabTrackedBar != null)
        {
            _battlesTabTrackedBar.PropertyChanged -= OnBattlesTabTrackedBarPropertyChanged;
            _battlesTabTrackedBar = null;
        }

        _battlesTabTrackedBar = _allBattleBars.FirstOrDefault(b => b.IsTraining);
        if (_battlesTabTrackedBar != null)
        {
            _battlesTabTrackedBar.PropertyChanged += OnBattlesTabTrackedBarPropertyChanged;
        }

        UpdateBattlesTabProgressBindings();
    }

    private void UpdateBattlesTabProgressBindings()
    {
        var bar = _battlesTabTrackedBar;
        if (bar is null)
        {
            HasBattlesTabProgressIndicator = false;
            BattlesTabProgressFraction = 0;
            BattlesTabProgressAccentBrush = Brushes.Transparent;
            return;
        }

        HasBattlesTabProgressIndicator = true;
        BattlesTabProgressFraction = bar.ProgressFraction;
        BattlesTabProgressAccentBrush = bar.AccentBrush;
    }

    /// <summary>Sum of every type counter (route Pokémon type points).</summary>
    private int GetTotalTypeLevels() => TypeCounters.Sum(c => c.Count);

    /// <summary>Higher total type levels → faster progress on battle bars.</summary>
    private double GetBattleSpeedFromTypeLevels()
    {
        var sum = GetTotalTypeLevels();
        var multiplier = GameBalance.Battles.BattleSpeedMultiplierBaseline + sum * GameBalance.Battles.BattleSpeedBonusPerTotalTypeLevel;
        return Math.Min(multiplier, GameBalance.Battles.BattleSpeedMultiplierCap);
    }

    /// <summary>Battle clears weighted per trainer → faster progress when leveling Pokémon on routes.</summary>
    private double GetPokemonTrainingSpeedFromBattleClears() => RouteGymTrainingSpeedMultiplier;

    /// <summary>Gym bonus on route catch speed = baseline + (training gym bonus above baseline) × fraction.</summary>
    private double GetPokemonCatchSpeedFromBattleClears()
    {
        var training = RouteGymTrainingSpeedMultiplier;
        var baseline = GameBalance.Battles.RouteTrainingSpeedMultiplierBaseline;
        var gymBonusAboveBaseline = Math.Max(0, training - baseline);
        return baseline + gymBonusAboveBaseline * GameBalance.Battles.RouteCatchFractionOfTrainingGymBonus;
    }

    private double RouteGymTrainingSpeedMultiplier =>
        SpeedMultiplierFromBattleClears(
            GameBalance.Battles.RouteTrainingBonusPerClearByTrainerIndex,
            GameBalance.Battles.RouteTrainingSpeedMultiplierBaseline,
            GameBalance.Battles.RouteTrainingSpeedMultiplierCap);

    private double SpeedMultiplierFromBattleClears(double[] weightsPerTrainer, double baseline, double cap)
    {
        var bonus = 0.0;
        for (var i = 0; i < _allBattleBars.Count; i++)
        {
            var clears = Math.Max(0, _allBattleBars[i].Level);
            bonus += clears * BonusWeightForTrainer(weightsPerTrainer, i);
        }

        return Math.Min(baseline + bonus, cap);
    }

    /// <summary>Out-of-range indices use the last entry.</summary>
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

    private void RecordTypeLevelContributions(Game.TypeLevelContribution[] contributions)
    {
        var any = false;
        foreach (var c in contributions)
        {
            if (c.Points == 0)
            {
                continue;
            }

            any = true;
            if (_typeCountersByKey.TryGetValue(c.TypeKey, out var counter))
            {
                counter.Count += c.Points;
            }
        }

        if (!any)
        {
            return;
        }

        foreach (var battleBar in _allBattleBars)
        {
            battleBar.NotifyTimeRemainingChanged();
        }
    }

    private void OnPokemonLevelChanged(PokemonTrainingBarViewModel pokemonBar)
    {
        TryUnlockCeladonAlternateEeveelutions(pokemonBar);
        RefreshRoutesTabCatchIndicator();
        RefreshPokedexCells();
        UpdateProgressionVisibility();
        foreach (var bar in _allPokemonBars)
        {
            bar.NotifyTimeRemainingChanged();
        }

        foreach (var battleBar in _allBattleBars)
        {
            battleBar.NotifyTimeRemainingChanged();
        }
    }

    private void OnBattleLevelChanged(PokemonTrainingBarViewModel battleBar)
    {
        UpdateProgressionVisibility();
        foreach (var pokemonBar in _allPokemonBars)
        {
            pokemonBar.NotifyTimeRemainingChanged();
        }
    }

    private bool ProgressionLookupsReady => _areasByRouteId is not null && _trainersById is not null;

    private void UpdateProgressionVisibility()
    {
        if (!ProgressionLookupsReady)
        {
            return;
        }

        for (var i = 0; i < KantoProgressionCatalog.Order.Length; i++)
        {
            var unlocked = i == 0 || IsProgressionStepComplete(KantoProgressionCatalog.Order[i - 1]);
            var step = KantoProgressionCatalog.Order[i];

            if (step.Kind == ProgressionStepKind.Route)
            {
                if (_areasByRouteId.TryGetValue(step.TargetId, out var area))
                {
                    area.IsVisible = unlocked;
                }
            }
            else if (_trainersById.TryGetValue(step.TargetId, out var trainer))
            {
                trainer.IsVisible = unlocked;
            }
        }

        foreach (var optional in KantoProgressionCatalog.OptionalRouteUnlocks)
        {
            if (_areasByRouteId.TryGetValue(optional.RouteId, out var area))
            {
                area.IsVisible = IsProgressionStepComplete(optional.UnlockWhen);
            }
        }
    }

    private bool IsProgressionStepComplete(ProgressionStep step)
    {
        if (!ProgressionLookupsReady)
        {
            return false;
        }

        return step.Kind switch
        {
            ProgressionStepKind.Route => _areasByRouteId.TryGetValue(step.TargetId, out var area)
                && area.PokemonBars.Any(p => p.Level >= GameBalance.Routes.MinPokemonLevelToPassRoute),
            ProgressionStepKind.Trainer => _trainersById.TryGetValue(step.TargetId, out var trainer)
                && trainer.Level >= GameBalance.Battles.MinTrainerLevelToRevealNextBattle,
            _ => false,
        };
    }

    private void InitializePokedex()
    {
        PokedexCells.Clear();
        for (var i = 0; i < KantoSpeciesCatalog.NationalDexCellCount; i++)
        {
            PokedexCells.Add(new PokedexCellViewModel());
        }

        RefreshPokedexCells();
    }

    /// <summary>Kanto #001–#150: black until a route bar reaches level 1+, then each reached species uses its primary type accent.</summary>
    private void RefreshPokedexCells()
    {
        foreach (var cell in PokedexCells)
        {
            cell.FillBrush = PokedexCellViewModel.UncaughtFill;
        }

        foreach (var bar in _allPokemonBars)
        {
            if (bar.Level < 1)
            {
                continue;
            }

            var chain = KantoSpeciesCatalog.TryGetEvolutionChain(bar.SpeciesLineRoot);
            if (chain is { Length: > 0 })
            {
                foreach (var stage in chain)
                {
                    if (stage.MinLevel > bar.Level)
                    {
                        continue;
                    }

                    if (KantoSpeciesCatalog.CellIndexBySpeciesName.TryGetValue(stage.Name, out var idx))
                    {
                        PokedexCells[idx].FillBrush = Brush.Parse(TypeCatalog.AccentHexForTypeKey(stage.TypeKey));
                    }
                }
            }
            else if (KantoSpeciesCatalog.CellIndexBySpeciesName.TryGetValue(bar.Name, out var idx))
            {
                PokedexCells[idx].FillBrush = Brush.Parse(TypeCatalog.AccentHexForTypeKey(bar.TypeKey));
            }
        }
    }
}
