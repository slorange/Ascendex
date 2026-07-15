# Ascendex

Pokémon-themed incremental / idle game built with Avalonia (Desktop, Android, iOS, Browser). Kanto-first scope.

Train and catch Pokémon on routes, clear gyms and the Elite Four, fill the Pokédex, then prestige for Exp Shares and Shiny Charms. Progress auto-saves; offline time banks into a 3× speed boost while training.

## Docs

- **[GAME_DESIGN.md](GAME_DESIGN.md)** — what the build does today, balance/content locations, and planned features.

## Project layout

```
Ascendex/
  Game/           Runtime rules, RunState, GameSession, TrainingSimulator, GameBalance
  Game/Content/   Static Kanto catalogs (species, routes, trainers, badges, progression)
  Game/Save/      Versioned JSON save / load
  ViewModels/     UI bindings, tick loop
  Views/          Routes, Battles, Collections, Prestige
```

## Build

```bash
dotnet build Ascendex/Ascendex.csproj
```

Platform-specific projects live under `Ascendex.Desktop/`, `Ascendex.Android/`, `Ascendex.iOS/`, and `Ascendex.Browser/` as applicable.
