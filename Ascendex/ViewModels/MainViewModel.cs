using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Ascendex.ViewModels;

public class MainViewModel : ViewModelBase
{
    private static readonly bool UsePrimaryTypeBarColors = false;

    private static readonly IReadOnlyDictionary<string, PokemonBarPalette> TypeBarPalettes =
        new Dictionary<string, PokemonBarPalette>
        {
            ["normal"] = new("#B9B1A0", "#1E1A14"),
            ["fighting"] = new("#C95B49", "#FFF4F0"),
            ["flying"] = new("#8FA6F2", "#0F1430"),
            ["poison"] = new("#A668C7", "#FFF5FF"),
            ["ground"] = new("#D0B05C", "#241904"),
            ["rock"] = new("#B89F52", "#211807"),
            ["bug"] = new("#99B63B", "#152006"),
            ["ghost"] = new("#6E5AAE", "#F7F4FF"),
            ["steel"] = new("#8E9AA8", "#0F141A"),
            ["fire"] = new("#E6823D", "#281003"),
            ["water"] = new("#5D8FE8", "#F4F8FF"),
            ["grass"] = new("#5FB85B", "#081807"),
            ["electric"] = new("#E7C63A", "#261E00"),
            ["psychic"] = new("#E96C9B", "#2A0713"),
            ["ice"] = new("#7FD6D8", "#082022"),
            ["dragon"] = new("#6D6AE6", "#F4F5FF"),
            ["dark"] = new("#5D4A47", "#F8F3F1"),
            ["fairy"] = new("#E5A5D3", "#2B1020")
        };

    private static readonly IReadOnlyDictionary<string, PokemonBarPalette> PokemonBarPalettes =
        new Dictionary<string, PokemonBarPalette>
        {
            ["Bulbasaur"] = new("#78C86A", "#10210C"),
            ["Charmander"] = new("#E78A43", "#2B1202"),
            ["Squirtle"] = new("#73B9F2", "#071C2A"),
            ["Pikachu"] = new("#F3D44F", "#2B2400"),
            ["Eevee"] = new("#C9A27A", "#24150A"),
            ["Pidgey"] = new("#D8C3A2", "#20150B"),
            ["Rattata"] = new("#B98AD3", "#130819"),
            ["Spearow"] = new("#C68A58", "#201109"),
            ["Nidoran♀"] = new("#6CC6F8", "#06121A"),
            ["Nidoran♂"] = new("#B98AD3", "#170A1E"),
            ["Mankey"] = new("#D7C1AE", "#1D1106"),
            ["Caterpie"] = new("#91D469", "#102009"),
            ["Weedle"] = new("#D5A14D", "#241304")
        };

    private readonly List<PokemonTrainingBarViewModel> _allPokemonBars;
    private readonly Dictionary<string, TypeCounterViewModel> _typeCountersByKey;
    private string _currentAreaName = string.Empty;

    public MainViewModel()
    {
        TypeCounters =
            new ObservableCollection<TypeCounterViewModel>
            {
                new("normal"),
                new("fighting"),
                new("flying"),
                new("poison"),
                new("ground"),
                new("rock"),
                new("bug"),
                new("ghost"),
                new("steel"),
                new("fire"),
                new("water"),
                new("grass"),
                new("electric"),
                new("psychic"),
                new("ice"),
                new("dragon"),
                new("dark"),
                new("fairy"),
            };

        _typeCountersByKey = new Dictionary<string, TypeCounterViewModel>();
        foreach (var counter in TypeCounters)
        {
            _typeCountersByKey[counter.TypeKey] = counter;
        }

        PokemonBars = new ObservableCollection<PokemonTrainingBarViewModel>();
        AreaSelectors = new ObservableCollection<AreaSelectionViewModel>();
        _allPokemonBars = new List<PokemonTrainingBarViewModel>();

        AddArea(
            "PT",
            "Pallet Town",
            CreatePokemon("Bulbasaur", "grass"),
            CreatePokemon("Charmander", "fire"),
            CreatePokemon("Squirtle", "water"),
            CreatePokemon("Pikachu", "electric"),
            CreatePokemon("Eevee", "normal"));

        AddArea(
            "R1",
            "Route 1",
            CreatePokemon("Pidgey", "flying"),
            CreatePokemon("Rattata", "normal"));

        AddArea(
            "R22",
            "Route 22",
            CreatePokemon("Spearow", "flying"),
            CreatePokemon("Nidoran♀", "poison"),
            CreatePokemon("Nidoran♂", "poison"),
            CreatePokemon("Mankey", "fighting"));

        AddArea(
            "VF",
            "Viridian Forest",
            CreatePokemon("Caterpie", "bug"),
            CreatePokemon("Weedle", "bug"));

        SelectArea(AreaSelectors[0]);
    }

    public ObservableCollection<PokemonTrainingBarViewModel> PokemonBars { get; }

    public ObservableCollection<AreaSelectionViewModel> AreaSelectors { get; }

    public ObservableCollection<TypeCounterViewModel> TypeCounters { get; }

    public string CurrentAreaName
    {
        get => _currentAreaName;
        private set => SetProperty(ref _currentAreaName, value);
    }

    private void AddArea(
        string shortLabel,
        string displayName,
        params PokemonTrainingBarViewModel[] pokemonBars)
    {
        foreach (var pokemonBar in pokemonBars)
        {
            _allPokemonBars.Add(pokemonBar);
        }

        AreaSelectors.Add(new AreaSelectionViewModel(shortLabel, displayName, pokemonBars, SelectArea));
    }

    private PokemonTrainingBarViewModel CreatePokemon(
        string name,
        string typeKey,
        double progressRequired = 120)
    {
        var palette = ResolveBarPalette(name, typeKey);

        return new PokemonTrainingBarViewModel(
            name,
            typeKey,
            palette.AccentColor,
            palette.ForegroundColor,
            ToggleTraining,
            RecordTypeLevelUp,
            progressRequired);
    }

    private static PokemonBarPalette ResolveBarPalette(string name, string typeKey)
    {
        if (UsePrimaryTypeBarColors)
        {
            return TypeBarPalettes[typeKey];
        }

        if (PokemonBarPalettes.TryGetValue(name, out var pokemonPalette))
        {
            return pokemonPalette;
        }

        return TypeBarPalettes[typeKey];
    }

    private void SelectArea(AreaSelectionViewModel selectedArea)
    {
        foreach (var area in AreaSelectors)
        {
            area.IsSelected = area == selectedArea;
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

    private void RecordTypeLevelUp(string typeKey)
    {
        if (_typeCountersByKey.TryGetValue(typeKey, out var counter))
        {
            counter.Count++;
        }
    }

    private readonly record struct PokemonBarPalette(string AccentColor, string ForegroundColor);
}
