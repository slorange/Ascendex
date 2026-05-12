using System.Collections.ObjectModel;

namespace Ascendex.ViewModels;

public class MainViewModel : ViewModelBase
{
    public ObservableCollection<PokemonTrainingBarViewModel> PokemonBars { get; } =
        new(
        [
            new("Bulbasaur", "#5AC85C", "#F5FFF5"),
            new("Charmander", "#E35B4F", "#FFF5F3"),
            new("Squirtle", "#4DA6E8", "#F4FBFF"),
            new("Pikachu", "#F2D34C", "#2B2400"),
            new("Eevee", "#F1F1F1", "#1B1B1B")
        ]);
}
