using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Ascendex.Game;
using Ascendex.Game.Content;
using Ascendex.Game.Save;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;

namespace Ascendex.ViewModels;

public class MainViewModel : ViewModelBase, IDisposable
{
    private static readonly IBrush MainTabSelectedBrush = Brush.Parse(MagicNumbersUI.Tabs.MainTabSelectedBackground);
    private static readonly IBrush MainTabUnselectedBrush = Brush.Parse(MagicNumbersUI.Tabs.MainTabUnselectedBackground);
    private static readonly IBrush ChampionResetReadyBrush = Brush.Parse("#4A6A3A");
    private static readonly IBrush PokedexResetReadyBrush = Brush.Parse("#3A5D7A");
    private static readonly IBrush ResetUnavailableBrush = Brush.Parse("#3A3F47");
    private static readonly IBrush SpeedBoostActiveBackground = Brush.Parse(MagicNumbersUI.SpeedBoost.ActiveBackground);
    private static readonly IBrush SpeedBoostIdleBackground = Brush.Parse(MagicNumbersUI.SpeedBoost.IdleBackground);
    private static readonly IBrush SpeedBoostActiveForeground = Brush.Parse(MagicNumbersUI.SpeedBoost.ActiveForeground);
    private static readonly IBrush SpeedBoostIdleForeground = Brush.Parse(MagicNumbersUI.SpeedBoost.IdleForeground);
    private static readonly Dictionary<string, IBrush> PokedexBrushes = new(StringComparer.OrdinalIgnoreCase);

    private readonly GameSession _session;
    private readonly SaveGameService _saveService;
    private readonly GameTickLoop _tickLoop;
    private readonly List<PokemonTrainingBarViewModel> _allPokemonBars;
    private readonly List<PokemonTrainingBarViewModel> _allBattleBars;
    private string _currentAreaName = string.Empty;
    private int _selectedAreaIndex;
    private int _selectedMainTab;
    private PokemonTrainingBarViewModel? _battlesTabTrackedBar;
    private double _battlesTabProgressFraction;
    private bool _hasBattlesTabProgressIndicator;
    private IBrush _battlesTabProgressNormalBrush = Brushes.Transparent;
    private bool _hasBankTime;
    private bool _isSpeedBoostActive;
    private string _speedBoostIndicatorText = string.Empty;
    private IBrush _speedBoostIndicatorBackground = Brushes.Transparent;
    private IBrush _speedBoostIndicatorForeground = Brushes.White;
    private int _lastDisplayedBankSeconds = -1;
    private bool _lastDisplayedSpeedBoostActive;
    private bool _routesInitialized;
    private bool _battlesInitialized;
    private bool _shopInitialized;
    private bool _collectionsInitialized;
    private bool _isSuspended;
    private DateTimeOffset? _suspendedAtUtc;
    private string? _directTrackedTrainerId;
    private IBrush _directTrackedTrainerBrush = Brushes.Transparent;
    private bool _disposed;

    public static MainViewModel Create(SaveGameService? saveService = null)
    {
        saveService ??= SaveGameService.CreateDefault();
        var (session, selectedTab) = saveService.LoadOrCreateNew();
        return new MainViewModel(session, saveService, selectedTab);
    }

    public MainViewModel()
        : this(_designTimeDefaults.Value.Session, _designTimeDefaults.Value.SaveService, _designTimeDefaults.Value.SelectedTab)
    {
    }

    private static readonly Lazy<(GameSession Session, SaveGameService SaveService, int SelectedTab)> _designTimeDefaults = new(() =>
    {
        var saveService = SaveGameService.CreateDefault();
        var (session, selectedTab) = saveService.LoadOrCreateNew();
        return (session, saveService, selectedTab);
    });

    private static (GameSession Session, SaveGameService SaveService, int SelectedTab) CreateDefaultSessionAndService()
    {
        var saveService = SaveGameService.CreateDefault();
        var (session, selectedTab) = saveService.LoadOrCreateNew();
        return (session, saveService, selectedTab);
    }

    public MainViewModel(GameSession session, SaveGameService saveService, int selectedMainTab)
    {
        _session = session;
        _saveService = saveService;
        _tickLoop = new GameTickLoop(_session);
        _tickLoop.TimeDisplayRefreshRequested += OnTimeDisplayRefreshRequested;
        _tickLoop.PresentationAdvanced += OnPresentationAdvanced;

        SelectRoutesTabCommand = new RelayCommand(() => SelectedMainTab = 0);
        SelectBattlesTabCommand = new RelayCommand(() => SelectedMainTab = 1);
        SelectShopTabCommand = new RelayCommand(() => SelectedMainTab = 2);
        SelectCollectionsTabCommand = new RelayCommand(() => SelectedMainTab = 3);
        SelectPrestigeTabCommand = new RelayCommand(() => SelectedMainTab = 4);
        ChampionResetCommand = new RelayCommand(PerformChampionReset);
        PokedexResetCommand = new RelayCommand(PerformPokedexReset);

        TypeCounters = new ObservableCollection<TypeCounterViewModel>();
        PokemonBars = new ObservableCollection<PokemonTrainingBarViewModel>();
        BattleBars = new ObservableCollection<PokemonTrainingBarViewModel>();
        AreaSelectors = new ObservableCollection<AreaSelectionViewModel>();
        ShopItems = new ObservableCollection<ShopItemRowViewModel>();
        VitaminTargets = new ObservableCollection<VitaminTargetViewModel>();
        _allPokemonBars = new List<PokemonTrainingBarViewModel>();
        _allBattleBars = new List<PokemonTrainingBarViewModel>();

        _session.SpeciesLevelChanged += OnSessionSpeciesLevelChanged;
        _session.TrainerLevelChanged += OnSessionTrainerLevelChanged;
        _session.TypeCountersChanged += OnSessionTypeCountersChanged;
        _session.ProgressionChanged += OnSessionProgressionChanged;
        _session.BankTimeChanged += OnSessionBankTimeChanged;
        _session.ActiveBarsChanged += OnSessionActiveBarsChanged;
        _session.ShopStateChanged += OnSessionShopStateChanged;

        _selectedMainTab = -1;
        SelectedMainTab = Math.Clamp(selectedMainTab, 0, 4);
        _saveService.BindAutoSave(_session, () => SelectedMainTab);
        RefreshSpeedBoostIndicator();
        _saveService.SaveNow();
    }

    public IRelayCommand SelectRoutesTabCommand { get; }

    public IRelayCommand SelectBattlesTabCommand { get; }

    public IRelayCommand SelectShopTabCommand { get; }

    public IRelayCommand SelectCollectionsTabCommand { get; }

    public IRelayCommand SelectPrestigeTabCommand { get; }

    public IRelayCommand ChampionResetCommand { get; }

    public IRelayCommand PokedexResetCommand { get; }

    public int SelectedMainTab
    {
        get => _selectedMainTab;
        set
        {
            if (!IsShopTabUnlocked && value == 2)
            {
                value = 0;
            }

            if (_selectedMainTab != value)
            {
                EnsureTabInitialized(value);
            }

            if (SetProperty(ref _selectedMainTab, value))
            {
                OnPropertyChanged(nameof(IsRoutesTabSelected));
                OnPropertyChanged(nameof(IsBattlesTabSelected));
                OnPropertyChanged(nameof(IsShopTabSelected));
                OnPropertyChanged(nameof(IsCollectionsTabSelected));
                OnPropertyChanged(nameof(IsPrestigeTabSelected));
                OnPropertyChanged(nameof(RoutesTabBackground));
                OnPropertyChanged(nameof(BattlesTabBackground));
                OnPropertyChanged(nameof(ShopTabBackground));
                OnPropertyChanged(nameof(CollectionsTabBackground));
                OnPropertyChanged(nameof(PrestigeTabBackground));
            }
            else
            {
                EnsureTabInitialized(value);
            }
        }
    }

    public bool IsRoutesTabSelected => _selectedMainTab == 0;

    public bool IsBattlesTabSelected => _selectedMainTab == 1;

    public bool IsShopTabSelected => _selectedMainTab == 2;

    public bool IsCollectionsTabSelected => _selectedMainTab == 3;

    public bool IsPrestigeTabSelected => _selectedMainTab == 4;

    public IBrush RoutesTabBackground => _selectedMainTab == 0 ? MainTabSelectedBrush : MainTabUnselectedBrush;

    public IBrush BattlesTabBackground => _selectedMainTab == 1 ? MainTabSelectedBrush : MainTabUnselectedBrush;

    public IBrush ShopTabBackground => _selectedMainTab == 2 ? MainTabSelectedBrush : MainTabUnselectedBrush;

    public IBrush CollectionsTabBackground => _selectedMainTab == 3 ? MainTabSelectedBrush : MainTabUnselectedBrush;

    public IBrush PrestigeTabBackground => _selectedMainTab == 4 ? MainTabSelectedBrush : MainTabUnselectedBrush;

    public bool ShowRoutesTabPokeballIcon => !_session.AnySpeciesCatching();

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

    public IBrush BattlesTabProgressNormalBrush
    {
        get => _battlesTabProgressNormalBrush;
        private set => SetProperty(ref _battlesTabProgressNormalBrush, value);
    }

    public bool HasBankTime
    {
        get => _hasBankTime;
        private set => SetProperty(ref _hasBankTime, value);
    }

    public bool IsSpeedBoostActive
    {
        get => _isSpeedBoostActive;
        private set => SetProperty(ref _isSpeedBoostActive, value);
    }

    public string SpeedBoostIndicatorText
    {
        get => _speedBoostIndicatorText;
        private set => SetProperty(ref _speedBoostIndicatorText, value);
    }

    public IBrush SpeedBoostIndicatorBackground
    {
        get => _speedBoostIndicatorBackground;
        private set => SetProperty(ref _speedBoostIndicatorBackground, value);
    }

    public IBrush SpeedBoostIndicatorForeground
    {
        get => _speedBoostIndicatorForeground;
        private set => SetProperty(ref _speedBoostIndicatorForeground, value);
    }

    public ObservableCollection<PokemonTrainingBarViewModel> PokemonBars { get; }

    public ObservableCollection<PokemonTrainingBarViewModel> BattleBars { get; }

    public ObservableCollection<AreaSelectionViewModel> AreaSelectors { get; }

    public ObservableCollection<ShopItemRowViewModel> ShopItems { get; }

    public ObservableCollection<VitaminTargetViewModel> VitaminTargets { get; }

    public ObservableCollection<TypeCounterViewModel> TypeCounters { get; }

    public ObservableCollection<PokedexCellViewModel> PokedexCells { get; } = new();

    public ObservableCollection<BadgeSlotViewModel> GymBadgeSlots { get; } = new();

    public ObservableCollection<BadgeSlotViewModel> LeagueHonorSlots { get; } = new();

    public int ChampionResetCount => _session.State.ChampionResetCount;

    public int PokedexResetCount => _session.State.PokedexResetCount;

    public int ExpShareCount => _session.State.ExpShareCount;

    public int ShinyCharmCount => _session.State.ShinyCharmCount;

    public long Pokedollars => _session.State.Pokedollars;

    public string PokedollarsText => $"{Pokedollars:N0} ₽";

    public int UnassignedVitaminCount => _session.State.UnassignedVitaminCount;

    public bool HasUnassignedVitamins => UnassignedVitaminCount > 0;

    public bool ShowVitaminApplySection => _session.State.VitaminApplySectionUnlocked;

    public string UnassignedVitaminsText => $"Unassigned vitamins: {UnassignedVitaminCount}";

    public bool IsShopTabUnlocked => _session.IsShopTabUnlocked();

    public bool CanChampionReset => _session.CanChampionReset();

    public bool CanPokedexReset => _session.CanPokedexReset();

    public string ChampionResetRequirementText => CanChampionReset
        ? "Ready: Blue has been defeated in this run."
        : "Requires Blue defeated in this run.";

    public string PokedexResetRequirementText => CanPokedexReset
        ? "Unlocked: all 150 Pokedex entries caught."
        : $"Requires all 150 caught ({CaughtPokedexCount}/150).";

    public int CaughtPokedexCount => PokedexRules.GetFilledCells(_session.State).Select(fill => fill.CellIndex).Distinct().Count();

    public IBrush ChampionResetButtonBackground => CanChampionReset
        ? ChampionResetReadyBrush
        : ResetUnavailableBrush;

    public IBrush PokedexResetButtonBackground => CanPokedexReset
        ? PokedexResetReadyBrush
        : ResetUnavailableBrush;

    public string CurrentAreaName
    {
        get => _currentAreaName;
        private set => SetProperty(ref _currentAreaName, value);
    }

    public int SelectedAreaIndex
    {
        get => _selectedAreaIndex;
        private set => SetProperty(ref _selectedAreaIndex, value);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _session.SpeciesLevelChanged -= OnSessionSpeciesLevelChanged;
        _session.TrainerLevelChanged -= OnSessionTrainerLevelChanged;
        _session.TypeCountersChanged -= OnSessionTypeCountersChanged;
        _session.ProgressionChanged -= OnSessionProgressionChanged;
        _session.BankTimeChanged -= OnSessionBankTimeChanged;
        _session.ActiveBarsChanged -= OnSessionActiveBarsChanged;
        _session.ShopStateChanged -= OnSessionShopStateChanged;
        _tickLoop.TimeDisplayRefreshRequested -= OnTimeDisplayRefreshRequested;
        _tickLoop.PresentationAdvanced -= OnPresentationAdvanced;
        _tickLoop.Dispose();
        _saveService.Dispose();
    }

    public void Suspend()
    {
        if (_disposed || _isSuspended)
        {
            return;
        }

        _isSuspended = true;
        _suspendedAtUtc = DateTimeOffset.UtcNow;
        _tickLoop.Suspend();
        _saveService.SuspendAutoSave();
        _saveService.SaveNow();
    }

    public void Resume()
    {
        if (_disposed || !_isSuspended)
        {
            return;
        }

        if (_suspendedAtUtc is { } suspendedAt)
        {
            _session.ApplyOfflineBankTime(suspendedAt);
        }

        _suspendedAtUtc = null;
        _isSuspended = false;
        _saveService.ResumeAutoSave();
        _tickLoop.Resume();
        RefreshSpeedBoostIndicator();
    }

    private void EnsureTabInitialized(int tab)
    {
        switch (tab)
        {
            case 0:
                InitializeRoutes();
                UpdateProgressionVisibility();
                RestoreSelectedArea();
                break;
            case 1:
                InitializeBattles();
                UpdateProgressionVisibility();
                RefreshBattlesTabProgressTracking();
                break;
            case 2:
                InitializeShop();
                RefreshShopState();
                break;
            case 3:
                InitializeCollections();
                break;
            case 4:
                RefreshPrestigeState();
                break;
        }
    }

    private void RestoreSelectedArea()
    {
        if (!_routesInitialized || AreaSelectors.Count == 0)
        {
            return;
        }

        var routeId = _session.State.SelectedRouteId;
        var area = AreaSelectors.FirstOrDefault(candidate => candidate.RouteId == routeId) ?? AreaSelectors[0];
        SelectArea(area);
    }

    private void InitializeRoutes()
    {
        if (_routesInitialized)
        {
            return;
        }

        _routesInitialized = true;
        foreach (var typeKey in TypeCatalog.CounterTypeKeys)
        {
            TypeCounters.Add(new TypeCounterViewModel(typeKey));
        }

        foreach (var route in KantoRouteCatalog.All)
        {
            var bars = new List<PokemonTrainingBarViewModel>();
            foreach (var spawn in route.Spawns)
            {
                var progress = _session.GetSpecies(spawn.SpeciesRootName);
                var typeKey = KantoSpeciesCatalog.PrimaryTypeKey(spawn.SpeciesRootName);
                var normalColor = KantoSpeciesCatalog.ResolveRouteBarColor(spawn.SpeciesRootName);
                var bar = new PokemonTrainingBarViewModel(
                    _session,
                    progress,
                    typeKey,
                    normalColor,
                    ToggleTraining);
                bars.Add(bar);
            }

            AddArea(route.Id, route.ShortLabel, route.DisplayName, bars.ToArray());
        }

        SyncTypeCountersFromSession();
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
            OnPropertyChanged(nameof(SelectedAreaIndex));
        }

        _session.SelectRoute(selectedArea.RouteId);
        CurrentAreaName = selectedArea.DisplayName;
        PokemonBars.Clear();

        foreach (var pokemonBar in selectedArea.PokemonBars)
        {
            PokemonBars.Add(pokemonBar);
        }
    }

    private void ToggleTraining(PokemonTrainingBarViewModel selectedBar)
    {
        _session.ToggleSpeciesActivity(selectedBar.SpeciesLineRoot, catchMode: selectedBar.CanCatch);
        RefreshRoutesTabCatchIndicator();
    }

    private void RefreshRoutesTabCatchIndicator() => OnPropertyChanged(nameof(ShowRoutesTabPokeballIcon));

    private void InitializeBattles()
    {
        if (_battlesInitialized)
        {
            return;
        }

        _battlesInitialized = true;
        foreach (var trainer in KantoTrainerCatalog.All)
        {
            var progress = _session.GetTrainer(trainer.Id);
            var normalColor = KantoSpeciesCatalog.ResolveTrainerBarColor(trainer.TypeKey);
            var bar = new PokemonTrainingBarViewModel(
                _session,
                progress,
                trainer.DisplayName,
                trainer.TypeKey,
                normalColor,
                ToggleBattleTraining);
            _allBattleBars.Add(bar);
            BattleBars.Add(bar);
        }
    }

    private void ToggleBattleTraining(PokemonTrainingBarViewModel selectedBar)
    {
        if (selectedBar.TrainerId is null)
        {
            return;
        }

        _session.ToggleTrainerActivity(selectedBar.TrainerId);
        RefreshBattlesTabProgressTracking();
    }

    private void OnBattlesTabTrackedBarPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PokemonTrainingBarViewModel.Progress)
            or nameof(PokemonTrainingBarViewModel.ProgressFraction)
            or nameof(PokemonTrainingBarViewModel.NormalBrush))
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
            BattlesTabProgressNormalBrush = Brushes.Transparent;
            return;
        }

        HasBattlesTabProgressIndicator = true;
        BattlesTabProgressFraction = bar.ProgressFraction;
        BattlesTabProgressNormalBrush = bar.NormalBrush;
    }

    private void UpdateDirectBattlesTabProgress()
    {
        var progress = _session.GetActiveTrainerProgress();
        if (progress is null)
        {
            _directTrackedTrainerId = null;
            HasBattlesTabProgressIndicator = false;
            BattlesTabProgressFraction = 0;
            BattlesTabProgressNormalBrush = Brushes.Transparent;
            return;
        }

        var config = _session.GetTrainerBarConfig(progress.TrainerId);
        var required = _session.GetTrainerProgressRequired(config, progress.Level);
        if (_directTrackedTrainerId != progress.TrainerId)
        {
            _directTrackedTrainerId = progress.TrainerId;
            var trainer = KantoTrainerCatalog.All.First(candidate => candidate.Id == progress.TrainerId);
            _directTrackedTrainerBrush = Brush.Parse(KantoSpeciesCatalog.ResolveTrainerBarColor(trainer.TypeKey));
        }

        HasBattlesTabProgressIndicator = true;
        BattlesTabProgressFraction = required <= 0 ? 0 : progress.Progress / required;
        BattlesTabProgressNormalBrush = _directTrackedTrainerBrush;
    }

    private void OnSessionSpeciesLevelChanged(string speciesRootName)
    {
        RefreshRoutesTabCatchIndicator();
        if (_collectionsInitialized)
        {
            if (string.IsNullOrEmpty(speciesRootName))
            {
                RefreshPokedexCells();
            }
            else
            {
                RefreshPokedexCellsForRoot(speciesRootName);
            }
        }

        RefreshPrestigeState();
        NotifyAllBarsTimeRemainingChanged();
    }

    private void OnSessionTrainerLevelChanged()
    {
        if (_collectionsInitialized)
        {
            RefreshBadgeSlots();
        }

        RefreshPrestigeState();
        RefreshShopState();
        if (_battlesInitialized)
        {
            RefreshBattlesTabProgressTracking();
        }

        NotifyAllBarsTimeRemainingChanged();
    }

    private void OnSessionTypeCountersChanged()
    {
        if (_routesInitialized)
        {
            SyncTypeCountersFromSession();
        }

        foreach (var battleBar in _allBattleBars)
        {
            battleBar.NotifyTimeRemainingChanged();
        }
    }

    private void OnSessionShopStateChanged()
    {
        RefreshShopState();
        NotifyAllBarsTimeRemainingChanged();
    }

    private void OnSessionProgressionChanged()
    {
        UpdateProgressionVisibility();
        RefreshShopState();
    }

    private void OnSessionBankTimeChanged() => RefreshSpeedBoostIndicatorThrottled();

    private void OnSessionActiveBarsChanged()
    {
        RefreshSpeedBoostIndicator();
        if (_battlesInitialized)
        {
            RefreshBattlesTabProgressTracking();
        }
        else
        {
            UpdateDirectBattlesTabProgress();
        }
    }

    private void OnTimeDisplayRefreshRequested()
    {
        foreach (var bar in _allPokemonBars)
        {
            if (bar.IsActivityActive)
            {
                bar.NotifyTimeRemainingChanged();
            }
        }

        foreach (var bar in _allBattleBars)
        {
            if (bar.IsActivityActive)
            {
                bar.NotifyTimeRemainingChanged();
            }
        }
    }

    private void OnPresentationAdvanced()
    {
        if (!_battlesInitialized)
        {
            UpdateDirectBattlesTabProgress();
        }
    }

    private void RefreshSpeedBoostIndicatorThrottled()
    {
        var bankSeconds = (int)Math.Ceiling(_session.State.BankTimeSeconds);
        var isActive = _session.State.BankTimeSeconds > 0 && _session.HasActiveBars();
        if (bankSeconds == _lastDisplayedBankSeconds && isActive == _lastDisplayedSpeedBoostActive)
        {
            return;
        }

        _lastDisplayedBankSeconds = bankSeconds;
        _lastDisplayedSpeedBoostActive = isActive;
        RefreshSpeedBoostIndicator();
    }

    private void RefreshSpeedBoostIndicator()
    {
        var bankSeconds = _session.State.BankTimeSeconds;
        var hasBank = bankSeconds > 0;
        var isActive = hasBank && _session.HasActiveBars();
        var bankLabel = FormatBankDuration(bankSeconds);

        HasBankTime = hasBank;
        IsSpeedBoostActive = isActive;
        SpeedBoostIndicatorText = isActive
            ? $"{GameBalance.SpeedBoost.Multiplier}× · {bankLabel} bank"
            : $"{bankLabel} bank";
        SpeedBoostIndicatorBackground = isActive ? SpeedBoostActiveBackground : SpeedBoostIdleBackground;
        SpeedBoostIndicatorForeground = isActive ? SpeedBoostActiveForeground : SpeedBoostIdleForeground;

        _lastDisplayedBankSeconds = (int)Math.Ceiling(bankSeconds);
        _lastDisplayedSpeedBoostActive = isActive;
    }

    private static string FormatBankDuration(double bankSeconds)
    {
        var totalSeconds = (int)Math.Ceiling(Math.Max(0, bankSeconds));
        if (totalSeconds >= 3600)
        {
            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            return minutes > 0 ? $"{hours}h {minutes}m" : $"{hours}h";
        }

        if (totalSeconds >= 60)
        {
            return $"{totalSeconds / 60}m {totalSeconds % 60}s";
        }

        return $"{totalSeconds}s";
    }

    private void PerformChampionReset()
    {
        if (!_session.PerformChampionReset())
        {
            return;
        }

        RefreshAllAfterReset();
    }

    private void PerformPokedexReset()
    {
        if (!_session.PerformPokedexReset())
        {
            return;
        }

        RefreshAllAfterReset();
    }

    private void RefreshAllAfterReset()
    {
        if (_routesInitialized)
        {
            RestoreSelectedArea();
        }

        _saveService.SaveNow();
    }

    private void InitializeShop()
    {
        if (_shopInitialized)
        {
            return;
        }

        _shopInitialized = true;
        ShopItems.Clear();
        foreach (var (_, item) in KantoShopCatalog.EnumerateItems())
        {
            ShopItems.Add(new ShopItemRowViewModel(item, _session, RefreshShopState));
        }

        RefreshVitaminTargets();
    }

    private void RefreshShopState()
    {
        OnPropertyChanged(nameof(Pokedollars));
        OnPropertyChanged(nameof(PokedollarsText));
        OnPropertyChanged(nameof(UnassignedVitaminCount));
        OnPropertyChanged(nameof(HasUnassignedVitamins));
        OnPropertyChanged(nameof(ShowVitaminApplySection));
        OnPropertyChanged(nameof(UnassignedVitaminsText));
        OnPropertyChanged(nameof(IsShopTabUnlocked));

        if (!IsShopTabUnlocked && SelectedMainTab == 2)
        {
            SelectedMainTab = 0;
        }

        if (!_shopInitialized)
        {
            return;
        }

        foreach (var item in ShopItems)
        {
            item.Refresh();
        }

        RefreshVitaminTargets();
    }

    private void RefreshVitaminTargets()
    {
        if (!_shopInitialized)
        {
            return;
        }

        var caught = _session.State.SpeciesByRoot.Values
            .Where(progress => progress.Level >= GameBalance.Routes.MinPokemonLevelToPassRoute)
            .Select(progress => progress.SpeciesRootName)
            .ToHashSet(StringComparer.Ordinal);

        var orderedRoots = KantoShopCatalog.SpeciesRootsInVitaminApplyOrder()
            .Where(caught.Contains)
            .ToList();

        for (var i = VitaminTargets.Count - 1; i >= 0; i--)
        {
            if (!orderedRoots.Contains(VitaminTargets[i].SpeciesRootName))
            {
                VitaminTargets.RemoveAt(i);
            }
        }

        var existing = VitaminTargets.Select(target => target.SpeciesRootName).ToHashSet(StringComparer.Ordinal);
        foreach (var root in orderedRoots)
        {
            if (!existing.Contains(root))
            {
                VitaminTargets.Add(new VitaminTargetViewModel(
                    root,
                    KantoShopCatalog.IsBossSpeciesRoot(root),
                    _session,
                    RefreshShopState));
            }
        }

        // Keep collection order matching route order (normals then bosses).
        for (var desiredIndex = 0; desiredIndex < orderedRoots.Count; desiredIndex++)
        {
            var root = orderedRoots[desiredIndex];
            var currentIndex = -1;
            for (var i = 0; i < VitaminTargets.Count; i++)
            {
                if (VitaminTargets[i].SpeciesRootName == root)
                {
                    currentIndex = i;
                    break;
                }
            }

            if (currentIndex >= 0 && currentIndex != desiredIndex)
            {
                VitaminTargets.Move(currentIndex, desiredIndex);
            }
        }

        foreach (var target in VitaminTargets)
        {
            target.Refresh();
        }
    }

    private void RefreshPrestigeState()
    {
        OnPropertyChanged(nameof(ChampionResetCount));
        OnPropertyChanged(nameof(PokedexResetCount));
        OnPropertyChanged(nameof(ExpShareCount));
        OnPropertyChanged(nameof(ShinyCharmCount));
        OnPropertyChanged(nameof(CanChampionReset));
        OnPropertyChanged(nameof(CanPokedexReset));
        OnPropertyChanged(nameof(ChampionResetButtonBackground));
        OnPropertyChanged(nameof(PokedexResetButtonBackground));
        OnPropertyChanged(nameof(ChampionResetRequirementText));
        OnPropertyChanged(nameof(PokedexResetRequirementText));
        OnPropertyChanged(nameof(CaughtPokedexCount));
    }

    private void SyncTypeCountersFromSession()
    {
        if (!_routesInitialized)
        {
            return;
        }

        foreach (var counter in TypeCounters)
        {
            counter.Count = _session.GetTypeCounterCount(counter.TypeKey);
        }
    }

    private void NotifyAllBarsTimeRemainingChanged()
    {
        foreach (var bar in _allPokemonBars)
        {
            bar.NotifyTimeRemainingChanged();
        }

        foreach (var battleBar in _allBattleBars)
        {
            battleBar.NotifyTimeRemainingChanged();
        }
    }

    private void UpdateProgressionVisibility()
    {
        if (_routesInitialized)
        {
            foreach (var area in AreaSelectors)
            {
                area.IsVisible = _session.IsRouteVisible(area.RouteId);
            }
        }

        if (_battlesInitialized)
        {
            foreach (var trainerBar in _allBattleBars)
            {
                if (trainerBar.TrainerId is not null)
                {
                    trainerBar.IsVisible = _session.IsTrainerVisible(trainerBar.TrainerId);
                }
            }
        }
    }

    private void InitializeCollections()
    {
        if (_collectionsInitialized)
        {
            return;
        }

        _collectionsInitialized = true;
        InitializePokedex();
        InitializeBadges();
    }

    private void InitializePokedex()
    {
        PokedexCells.Clear();
        for (var i = 0; i < KantoSpeciesCatalog.NationalDexCellCount; i++)
        {
            PokedexCells.Add(new PokedexCellViewModel(i, KantoSpeciesCatalog.NationalDexNames[i]));
        }

        RefreshPokedexCells();
    }

    private void RefreshPokedexCells()
    {
        foreach (var cell in PokedexCells)
        {
            cell.FillBrush = PokedexCellViewModel.UncaughtFill;
            cell.BorderBrush = PokedexCellViewModel.UncaughtBorder;
            cell.TooltipText = CollectionsTooltipFormatter.FormatPokedexCell(cell.SpeciesName, _session.State);
        }

        foreach (var fill in PokedexRules.GetFilledCells(_session.State))
        {
            var cell = PokedexCells[fill.CellIndex];
            cell.FillBrush = GetPokedexBrush(fill.FillColorHex);
            cell.BorderBrush = fill.IsShiny ? PokedexCellViewModel.ShinyBorder : PokedexCellViewModel.NormalBorder;
        }
    }

    private void RefreshPokedexCellsForRoot(string speciesRootName)
    {
        foreach (var cellIndex in PokedexRules.GetCellIndicesForRoot(speciesRootName))
        {
            var cell = PokedexCells[cellIndex];
            cell.FillBrush = PokedexCellViewModel.UncaughtFill;
            cell.BorderBrush = PokedexCellViewModel.UncaughtBorder;
            cell.TooltipText = CollectionsTooltipFormatter.FormatPokedexCell(cell.SpeciesName, _session.State);
        }

        foreach (var fill in PokedexRules.GetFilledCellsForRoot(_session.State, speciesRootName))
        {
            var cell = PokedexCells[fill.CellIndex];
            cell.FillBrush = GetPokedexBrush(fill.FillColorHex);
            cell.BorderBrush = fill.IsShiny ? PokedexCellViewModel.ShinyBorder : PokedexCellViewModel.NormalBorder;
        }
    }

    private void InitializeBadges()
    {
        GymBadgeSlots.Clear();
        foreach (var badge in KantoBadgeCatalog.GymBadges)
        {
            GymBadgeSlots.Add(new BadgeSlotViewModel(badge));
        }

        LeagueHonorSlots.Clear();
        foreach (var honor in KantoBadgeCatalog.LeagueHonors)
        {
            LeagueHonorSlots.Add(new BadgeSlotViewModel(honor));
        }

        RefreshBadgeSlots();
    }

    private void RefreshBadgeSlots()
    {
        if (!_collectionsInitialized)
        {
            return;
        }

        RefreshBadgeRow(GymBadgeSlots);
        RefreshBadgeRow(LeagueHonorSlots);
    }

    private void RefreshBadgeRow(ObservableCollection<BadgeSlotViewModel> slots)
    {
        foreach (var slot in slots)
        {
            var trainerId = slot.Definition.TrainerId;
            var typeKey = KantoTrainerCatalog.All.First(t => t.Id == trainerId).TypeKey;
            var earned = _session.GetTrainer(trainerId).Level
                >= GameBalance.Battles.MinTrainerLevelToRevealNextBattle;
            slot.SetEarned(earned, typeKey);
            slot.TooltipText = CollectionsTooltipFormatter.FormatBadge(slot.Definition, _session.State);
        }
    }

    private static IBrush GetPokedexBrush(string colorHex)
    {
        if (!PokedexBrushes.TryGetValue(colorHex, out var brush))
        {
            brush = Brush.Parse(colorHex);
            PokedexBrushes[colorHex] = brush;
        }

        return brush;
    }
}
