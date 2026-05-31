using Ascendex.Game.Content;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ascendex.ViewModels;

public partial class BadgeSlotViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isEarned;

    [ObservableProperty]
    private IBrush _backgroundBrush = Brushes.Black;

    [ObservableProperty]
    private IBrush _borderBrush = Brushes.Gray;

    [ObservableProperty]
    private string _tooltipText = string.Empty;

    public BadgeDefinition Definition { get; }

    public string DisplayName => Definition.DisplayName;

    public BadgeTier Tier => Definition.Tier;

    public BadgeSlotViewModel(BadgeDefinition definition)
    {
        Definition = definition;
        ApplyEarnedVisuals(false, typeKey: null);
    }

    public void SetEarned(bool earned, string? typeKey)
    {
        IsEarned = earned;
        ApplyEarnedVisuals(earned, typeKey);
    }

    private void ApplyEarnedVisuals(bool earned, string? typeKey)
    {
        if (!earned)
        {
            BackgroundBrush = Brush.Parse(MagicNumbersUI.BadgeGrid.UnearnedBackground);
            BorderBrush = Brush.Parse(MagicNumbersUI.BadgeGrid.UnearnedBorder);
            return;
        }

        var fill = string.IsNullOrEmpty(typeKey)
            ? MagicNumbersUI.BadgeGrid.UnearnedBackground
            : TypeCatalog.AccentHexForTypeKey(typeKey);
        BackgroundBrush = Brush.Parse(fill);
        BorderBrush = Brush.Parse(Definition.Tier switch
        {
            BadgeTier.Gym => MagicNumbersUI.BadgeGrid.EarnedGymBorder,
            BadgeTier.EliteFour => MagicNumbersUI.BadgeGrid.EarnedLeagueBorder,
            BadgeTier.Champion => MagicNumbersUI.BadgeGrid.EarnedChampionBorder,
            _ => MagicNumbersUI.BadgeGrid.EarnedLeagueBorder,
        });
    }
}
