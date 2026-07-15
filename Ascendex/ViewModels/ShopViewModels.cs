using System;
using System.Collections.Generic;
using Ascendex.Game;
using Ascendex.Game.Content;
using CommunityToolkit.Mvvm.Input;

namespace Ascendex.ViewModels;

public sealed class ShopItemRowViewModel : ViewModelBase
{
    private readonly GameSession _session;
    private readonly Action _onChanged;
    private bool _isVisible;
    private bool _isOwned;
    private bool _canPurchase;
    private bool _canBuyAll;
    private string _statusText = string.Empty;

    public ShopItemRowViewModel(ShopItemDefinition item, GameSession session, Action onChanged)
    {
        Item = item;
        _session = session;
        _onChanged = onChanged;
        PurchaseCommand = new RelayCommand(Purchase, () => CanPurchase);
        BuyAllCommand = new RelayCommand(BuyAll, () => CanBuyAll);
        Refresh();
    }

    public ShopItemDefinition Item { get; }

    public bool IsVitamin => Item.Kind == ShopItemKind.Vitamin;

    public string DisplayName => Item.DisplayName;

    public string Description => Item.Description;

    public string PriceText => $"{Item.Price:N0} ₽";

    public IRelayCommand PurchaseCommand { get; }

    public IRelayCommand BuyAllCommand { get; }

    public bool IsVisible
    {
        get => _isVisible;
        private set => SetProperty(ref _isVisible, value);
    }

    public bool IsOwned
    {
        get => _isOwned;
        private set => SetProperty(ref _isOwned, value);
    }

    public bool CanPurchase
    {
        get => _canPurchase;
        private set
        {
            if (SetProperty(ref _canPurchase, value))
            {
                PurchaseCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public bool CanBuyAll
    {
        get => _canBuyAll;
        private set
        {
            if (SetProperty(ref _canBuyAll, value))
            {
                BuyAllCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public void Refresh()
    {
        IsVisible = ShopRules.IsItemUnlocked(_session.State, Item.Id);
        IsOwned = Item.Kind != ShopItemKind.Vitamin && ShopRules.OwnsOneTimeItem(_session.State, Item.Id);
        CanPurchase = ShopRules.CanPurchase(_session.State, Item);
        CanBuyAll = IsVitamin
            && ShopRules.IsItemUnlocked(_session.State, Item.Id)
            && _session.State.Pokedollars >= Item.Price;
        StatusText = Item.Kind switch
        {
            ShopItemKind.Vitamin => _session.State.UnassignedVitaminCount > 0
                ? $"Owned unassigned: {_session.State.UnassignedVitaminCount}"
                : "Consumable",
            _ when IsOwned => "Owned",
            _ => CanPurchase ? "Buy" : "Can't afford",
        };
    }

    private void Purchase()
    {
        if (_session.TryPurchaseShopItem(Item.Id))
        {
            _onChanged();
        }
    }

    private void BuyAll()
    {
        if (_session.TryBuyAllVitamins() > 0)
        {
            _onChanged();
        }
    }
}

public sealed class VitaminTargetViewModel : ViewModelBase
{
    private readonly GameSession _session;
    private readonly Action _onChanged;
    private int _doses;
    private bool _canApply;
    private string _displayName = string.Empty;

    public VitaminTargetViewModel(string speciesRootName, bool isBoss, GameSession session, Action onChanged)
    {
        SpeciesRootName = speciesRootName;
        IsBoss = isBoss;
        MaxDoses = ShopRules.MaxVitaminDoses(speciesRootName);
        _session = session;
        _onChanged = onChanged;
        ApplyCommand = new RelayCommand(Apply, () => CanApply);
        Refresh();
    }

    public string SpeciesRootName { get; }

    public bool IsBoss { get; }

    public int MaxDoses { get; }

    public string DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }

    public IRelayCommand ApplyCommand { get; }

    public int Doses
    {
        get => _doses;
        private set => SetProperty(ref _doses, value);
    }

    public bool CanApply
    {
        get => _canApply;
        private set
        {
            if (SetProperty(ref _canApply, value))
            {
                ApplyCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public string DosesText => $"Doses: {Doses}/{MaxDoses}";

    public void Refresh()
    {
        var progress = _session.GetSpecies(SpeciesRootName);
        var config = _session.GetSpeciesBarConfig(SpeciesRootName);
        DisplayName = TrainingSimulator.TryGetResolvedEvolutionStage(config.EvolutionChain, progress.Level, out var stage)
            ? stage.Name
            : SpeciesRootName;

        _session.State.VitaminDosesBySpeciesRoot.TryGetValue(SpeciesRootName, out var doses);
        Doses = doses;
        OnPropertyChanged(nameof(DosesText));
        CanApply = _session.State.UnassignedVitaminCount > 0
            && doses < MaxDoses
            && progress.Level >= GameBalance.Routes.MinPokemonLevelToPassRoute;
    }

    private void Apply()
    {
        if (_session.TryApplyVitamin(SpeciesRootName))
        {
            _onChanged();
        }
    }
}
