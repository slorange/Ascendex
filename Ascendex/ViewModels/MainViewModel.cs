using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            ["Weedle"] = new("#D5A14D", "#241304"),
            ["Ekans"] = new("#8B63B7", "#15091F"),
            ["Sandshrew"] = new("#D6B46E", "#241908"),
            ["Jigglypuff"] = new("#F0A9D6", "#2B1020"),
            ["Magikarp"] = new("#E36B4E", "#2A0C05"),
            ["Clefairy"] = new("#F4B8DE", "#2A1020"),
            ["Zubat"] = new("#6F74C9", "#F3F4FF"),
            ["Paras"] = new("#D4744B", "#2B1207"),
            ["Oddish"] = new("#4F85D1", "#07192B"),
            ["Bellsprout"] = new("#A6D45C", "#102106"),
            ["Venonat"] = new("#A267BE", "#16091F"),
            ["Abra"] = new("#E4C559", "#2A2106"),
            ["Vulpix"] = new("#F3B16A", "#2A1404"),
            ["Meowth"] = new("#DCC584", "#241A08"),
            ["Growlithe"] = new("#E88B49", "#2B1102"),
            ["Psyduck"] = new("#F1D25A", "#2A2103"),
            ["Poliwag"] = new("#7C9BE8", "#09122B"),
            ["Goldeen"] = new("#F39A74", "#2B1207"),
            ["Machop"] = new("#8BA4C8", "#0B1320"),
            ["Geodude"] = new("#A68D63", "#211708"),
            ["Onix"] = new("#8F9A9D", "#0F1517"),
            ["Gastly"] = new("#7B6AD0", "#F4F3FF"),
            ["Cubone"] = new("#C7AF8F", "#21180B"),
            ["Scyther"] = new("#9DDA75", "#102108"),
            ["Pinsir"] = new("#A06A4B", "#231108"),
            ["Porygon"] = new("#E06D8E", "#270A13"),
            ["Dratini"] = new("#8C85EE", "#0E1030"),
            ["Diglett"] = new("#A46855", "#210E08"),
            ["Farfetch'd"] = new("#9DBF69", "#112008"),
            ["Drowzee"] = new("#E1C95B", "#271F05"),
            ["Mr. Mime"] = new("#F09CC3", "#2B0C19"),
            ["Hitmonlee"] = new("#C79062", "#241206"),
            ["Hitmonchan"] = new("#B77A68", "#24110A"),
            ["Lapras"] = new("#76B7E9", "#07202B"),
            ["Ponyta"] = new("#F28C54", "#2A1103"),
            ["Doduo"] = new("#C9A067", "#211407"),
            ["Tentacool"] = new("#5FB4D5", "#072028"),
            ["Slowpoke"] = new("#E8A8BC", "#2A1020"),
            ["Horsea"] = new("#5A8FE5", "#071A2A"),
            ["Staryu"] = new("#C48E4D", "#241507"),
            ["Exeggcute"] = new("#F0B5D2", "#2A0F1D"),
            ["Rhyhorn"] = new("#A78B71", "#22180D"),
            ["Tauros"] = new("#8B5F49", "#F8F3F0"),
            ["Lickitung"] = new("#F3A7C2", "#2A0E19"),
            ["Chansey"] = new("#F5C6D9", "#2A1320"),
            ["Tangela"] = new("#5978D8", "#F4F5FF"),
            ["Kangaskhan"] = new("#B89E79", "#22170B"),
            ["Magnemite"] = new("#B6C4D7", "#10161D"),
            ["Voltorb"] = new("#E65B5B", "#2A0808"),
            ["Electabuzz"] = new("#E6C447", "#261E02"),
            ["Seel"] = new("#D7EEF8", "#0C1C22"),
            ["Shellder"] = new("#7766C8", "#F4F3FF"),
            ["Krabby"] = new("#E3714D", "#2A0C05"),
            ["Jynx"] = new("#C173C8", "#17081B"),
            ["Omanyte"] = new("#7A92D6", "#09142A"),
            ["Kabuto"] = new("#8A7363", "#1E120C"),
            ["Aerodactyl"] = new("#8D7ACF", "#100F22"),
            ["Koffing"] = new("#9162B2", "#14091C"),
            ["Magmar"] = new("#EF8240", "#291003"),
            ["Ditto"] = new("#B889D9", "#13081A"),
            ["Grimer"] = new("#7A5C97", "#F7F4FF")
        };

    private readonly List<PokemonTrainingBarViewModel> _allPokemonBars;
    private readonly Dictionary<string, TypeCounterViewModel> _typeCountersByKey;
    private string _currentAreaName = string.Empty;
    private int _selectedAreaIndex;

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

        AddArea(
            "R3",
            "Route 3",
            CreatePokemon("Ekans", "poison"),
            CreatePokemon("Sandshrew", "ground"),
            CreatePokemon("Jigglypuff", "fairy"),
            CreatePokemon("Magikarp", "water"));

        AddArea(
            "MM",
            "Mt Moon",
            CreatePokemon("Clefairy", "fairy"),
            CreatePokemon("Zubat", "flying"),
            CreatePokemon("Paras", "bug"));

        AddArea(
            "R24",
            "Route 24",
            CreatePokemon("Oddish", "grass"),
            CreatePokemon("Bellsprout", "grass"),
            CreatePokemon("Venonat", "bug"),
            CreatePokemon("Abra", "psychic"));

        AddArea(
            "R7",
            "Route 7",
            CreatePokemon("Vulpix", "fire"),
            CreatePokemon("Meowth", "normal"),
            CreatePokemon("Growlithe", "fire"));

        AddArea(
            "GR",
            "Good Rod",
            CreatePokemon("Psyduck", "water"),
            CreatePokemon("Poliwag", "water"),
            CreatePokemon("Goldeen", "water"));

        AddArea(
            "RT",
            "Rock Tunnel",
            CreatePokemon("Machop", "fighting"),
            CreatePokemon("Geodude", "rock"),
            CreatePokemon("Onix", "rock"));

        AddArea(
            "TWR",
            "Pokemon Tower",
            CreatePokemon("Gastly", "ghost"),
            CreatePokemon("Cubone", "ground"));

        AddArea(
            "GC",
            "Celadon Game Corner",
            CreatePokemon("Scyther", "bug"),
            CreatePokemon("Pinsir", "bug"),
            CreatePokemon("Porygon", "normal"),
            CreatePokemon("Dratini", "dragon"));

        AddArea(
            "RX",
            "Route X",
            CreatePokemon("Diglett", "ground"),
            CreatePokemon("Farfetch'd", "flying"),
            CreatePokemon("Drowzee", "psychic"),
            CreatePokemon("Mr. Mime", "psychic"));

        AddArea(
            "SC",
            "Saffron City",
            CreatePokemon("Hitmonlee", "fighting"),
            CreatePokemon("Hitmonchan", "fighting"),
            CreatePokemon("Lapras", "ice"));

        AddArea(
            "CR",
            "Cycling Road",
            CreatePokemon("Ponyta", "fire"),
            CreatePokemon("Doduo", "flying"));

        AddArea(
            "SR",
            "Super Rod",
            CreatePokemon("Tentacool", "water"),
            CreatePokemon("Slowpoke", "water"),
            CreatePokemon("Horsea", "water"),
            CreatePokemon("Staryu", "water"));

        AddArea(
            "SZ1",
            "Safari Zone 1",
            CreatePokemon("Exeggcute", "grass"),
            CreatePokemon("Rhyhorn", "ground"),
            CreatePokemon("Tauros", "normal"));

        AddArea(
            "SZ2",
            "Safari Zone 2",
            CreatePokemon("Lickitung", "normal"),
            CreatePokemon("Chansey", "normal"),
            CreatePokemon("Tangela", "grass"),
            CreatePokemon("Kangaskhan", "normal"));

        AddArea(
            "PP",
            "Power Plant",
            CreatePokemon("Magnemite", "electric"),
            CreatePokemon("Voltorb", "electric"),
            CreatePokemon("Electabuzz", "electric"));

        AddArea(
            "SFI",
            "Seafoam Islands",
            CreatePokemon("Seel", "water"),
            CreatePokemon("Shellder", "water"),
            CreatePokemon("Krabby", "water"),
            CreatePokemon("Jynx", "ice"));

        AddArea(
            "LAB",
            "Pokemon Lab Cinnabar",
            CreatePokemon("Omanyte", "rock"),
            CreatePokemon("Kabuto", "rock"),
            CreatePokemon("Aerodactyl", "rock"));

        AddArea(
            "PM",
            "Pokemon Mansion",
            CreatePokemon("Koffing", "poison"),
            CreatePokemon("Magmar", "fire"),
            CreatePokemon("Ditto", "normal"),
            CreatePokemon("Grimer", "poison"));

        UpdateAreaVisibility();
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

    /// <summary>Index of the selected area in <see cref="AreaSelectors"/>; used to keep the route strip centered on the current location.</summary>
    public int SelectedAreaIndex
    {
        get => _selectedAreaIndex;
        private set => SetProperty(ref _selectedAreaIndex, value);
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
        double progressRequired = 30)
    {
        var palette = ResolveBarPalette(name, typeKey);

        return new PokemonTrainingBarViewModel(
            name,
            typeKey,
            palette.AccentColor,
            palette.ForegroundColor,
            ToggleTraining,
            OnPokemonLevelChanged,
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

        SelectedAreaIndex = AreaSelectors.IndexOf(selectedArea);
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

    private void OnPokemonLevelChanged(PokemonTrainingBarViewModel pokemonBar)
    {
        UpdateAreaVisibility();
    }

    private void UpdateAreaVisibility()
    {
        for (var index = 0; index < AreaSelectors.Count; index++)
        {
            if (index == 0)
            {
                AreaSelectors[index].IsVisible = true;
                continue;
            }

            var previousArea = AreaSelectors[index - 1];
            AreaSelectors[index].IsVisible =
                previousArea.IsVisible &&
                previousArea.PokemonBars.Any(pokemonBar => pokemonBar.Level >= 5);
        }
    }

    private readonly record struct PokemonBarPalette(string AccentColor, string ForegroundColor);
}
