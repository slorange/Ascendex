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
    private IBrush _battlesTabProgressAccentBrush = Brushes.Transparent;
    private bool _hasBankTime;
    private bool _isSpeedBoostActive;
    private string _speedBoostIndicatorText = string.Empty;
    private IBrush _speedBoostIndicatorBackground = Brushes.Transparent;
    private IBrush _speedBoostIndicatorForeground = Brushes.White;
    private int _lastDisplayedBankSeconds = -1;
    private bool _lastDisplayedSpeedBoostActive;

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

        SelectRoutesTabCommand = new RelayCommand(() => SelectedMainTab = 0);
        SelectBattlesTabCommand = new RelayCommand(() => SelectedMainTab = 1);
        SelectCollectionsTabCommand = new RelayCommand(() => SelectedMainTab = 2);

        TypeCounters = new ObservableCollection<TypeCounterViewModel>(
            TypeCatalog.CounterTypeKeys.Select(key => new TypeCounterViewModel(key)));

        SyncTypeCountersFromSession();

        PokemonBars = new ObservableCollection<PokemonTrainingBarViewModel>();
        BattleBars = new ObservableCollection<PokemonTrainingBarViewModel>();
        AreaSelectors = new ObservableCollection<AreaSelectionViewModel>();
        _allPokemonBars = new List<PokemonTrainingBarViewModel>();
        _allBattleBars = new List<PokemonTrainingBarViewModel>();

        _session.SpeciesLevelChanged += OnSessionSpeciesLevelChanged;
        _session.TrainerLevelChanged += OnSessionTrainerLevelChanged;
        _session.TypeCountersChanged += OnSessionTypeCountersChanged;
        _session.ProgressionChanged += OnSessionProgressionChanged;
        _session.BankTimeChanged += OnSessionBankTimeChanged;
        _session.ActiveBarsChanged += OnSessionActiveBarsChanged;

        InitializeRoutes();
        InitializeBattles();
        UpdateProgressionVisibility();
        RestoreSelectedArea();
        InitializePokedex();
        InitializeBadges();
        SelectedMainTab = Math.Clamp(selectedMainTab, 0, 2);
        RefreshBattlesTabProgressTracking();
        _saveService.BindAutoSave(_session, () => SelectedMainTab);
        RefreshSpeedBoostIndicator();
        _saveService.SaveNow();
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

    public IBrush BattlesTabProgressAccentBrush
    {
        get => _battlesTabProgressAccentBrush;
        private set => SetProperty(ref _battlesTabProgressAccentBrush, value);
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

    public ObservableCollection<TypeCounterViewModel> TypeCounters { get; }

    public ObservableCollection<PokedexCellViewModel> PokedexCells { get; } = new();

    public ObservableCollection<BadgeSlotViewModel> GymBadgeSlots { get; } = new();

    public ObservableCollection<BadgeSlotViewModel> LeagueHonorSlots { get; } = new();

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
        _session.SpeciesLevelChanged -= OnSessionSpeciesLevelChanged;
        _session.TrainerLevelChanged -= OnSessionTrainerLevelChanged;
        _session.TypeCountersChanged -= OnSessionTypeCountersChanged;
        _session.ProgressionChanged -= OnSessionProgressionChanged;
        _session.BankTimeChanged -= OnSessionBankTimeChanged;
        _session.ActiveBarsChanged -= OnSessionActiveBarsChanged;
        _tickLoop.Dispose();
        _saveService.Dispose();
    }

    private void RestoreSelectedArea()
    {
        var routeId = _session.State.SelectedRouteId;
        var area = AreaSelectors.FirstOrDefault(candidate => candidate.RouteId == routeId) ?? AreaSelectors[0];
        SelectArea(area);
    }

    private void InitializeRoutes()
    {
        foreach (var route in KantoRouteCatalog.All)
        {
            var bars = new List<PokemonTrainingBarViewModel>();
            foreach (var spawn in route.Spawns)
            {
                var progress = _session.GetSpecies(spawn.SpeciesRootName);
                var typeKey = KantoSpeciesCatalog.PrimaryTypeKey(spawn.SpeciesRootName);
                var palette = KantoSpeciesCatalog.ResolveRouteBarPalette(spawn.SpeciesRootName);
                var bar = new PokemonTrainingBarViewModel(
                    _session,
                    progress,
                    typeKey,
                    palette.AccentColor,
                    palette.ForegroundColor,
                    ToggleTraining);
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
        foreach (var trainer in KantoTrainerCatalog.All)
        {
            var progress = _session.GetTrainer(trainer.Id);
            var palette = KantoSpeciesCatalog.ResolveTrainerBarPalette(trainer.TypeKey);
            var bar = new PokemonTrainingBarViewModel(
                _session,
                progress,
                trainer.DisplayName,
                trainer.TypeKey,
                palette.AccentColor,
                palette.ForegroundColor,
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

    private void OnSessionSpeciesLevelChanged(string speciesRootName)
    {
        RefreshRoutesTabCatchIndicator();
        RefreshPokedexCells();
        NotifyAllBarsTimeRemainingChanged();
    }

    private void OnSessionTrainerLevelChanged()
    {
        RefreshBadgeSlots();
        NotifyAllBarsTimeRemainingChanged();
    }

    private void OnSessionTypeCountersChanged()
    {
        SyncTypeCountersFromSession();
        foreach (var battleBar in _allBattleBars)
        {
            battleBar.NotifyTimeRemainingChanged();
        }
    }

    private void OnSessionProgressionChanged() => UpdateProgressionVisibility();

    private void OnSessionBankTimeChanged() => RefreshSpeedBoostIndicatorThrottled();

    private void OnSessionActiveBarsChanged() => RefreshSpeedBoostIndicator();

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
        SpeedBoostIndicatorBackground = Brush.Parse(
            isActive ? MagicNumbersUI.SpeedBoost.ActiveBackground : MagicNumbersUI.SpeedBoost.IdleBackground);
        SpeedBoostIndicatorForeground = Brush.Parse(
            isActive ? MagicNumbersUI.SpeedBoost.ActiveForeground : MagicNumbersUI.SpeedBoost.IdleForeground);

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

    private void SyncTypeCountersFromSession()
    {
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
        foreach (var area in AreaSelectors)
        {
            area.IsVisible = _session.IsRouteVisible(area.RouteId);
        }

        foreach (var trainerBar in _allBattleBars)
        {
            if (trainerBar.TrainerId is not null)
            {
                trainerBar.IsVisible = _session.IsTrainerVisible(trainerBar.TrainerId);
            }
        }
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
            cell.TooltipText = CollectionsTooltipFormatter.FormatPokedexCell(cell.SpeciesName, _session.State);
        }

        foreach (var fill in PokedexRules.GetFilledCells(_session.State))
        {
            PokedexCells[fill.CellIndex].FillBrush = Brush.Parse(TypeCatalog.AccentHexForTypeKey(fill.TypeKey));
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
}
