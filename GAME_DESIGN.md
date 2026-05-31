# Ascendex — Game Design

Living design doc for the project. **Describes what the build does today**, what we plan to change in code structure next, and ideas on the horizon. Older brainstorming in git history should be treated skeptically if it contradicts the **Current features** section.

**High concept:** Pokémon-themed incremental game (EthosIdle-style progression lanes) built with Avalonia. Kanto-first scope.

**Design principles**

- Theme first — mechanics should feel Pokémon-native, not renamed tech trees.
- Clarity over obscurity — unlock rules should be understandable in play.
- Satisfying growth — meaningful gains every few minutes.
- Scalable systems — today’s prototype should not block regions, prestige, or collections meta later.

---

## Current features

What the app actually does in the current prototype. Balance numbers live in `Ascendex/ViewModels/GameBalance.cs`; UI-only constants in `MagicNumbersUI.cs`.

### Platform and shell

- Avalonia UI targeting Desktop, Android, iOS, and Browser.
- Single `MainViewModel` owns all game state; constructed directly in `App.axaml.cs` (no DI, no save service).
- Bottom tabs: **Routes**, **Battles**, **Collections**.

### Routes tab

- **Areas:** 22 Kanto locations (Pallet Town through Cerulean Cave), selected via a horizontal strip of short labels (PT, R1, VF, …).
- **Progress bars:** One bar per catchable/trainable species in the current area. Tap a bar to activate it.
- **Progress model:** Timer-driven fill (~16 ms tick). When the bar fills, the species gains a level and the bar resets. Required progress scales with level (`GameBalance.Training`).
- **Catch vs train:** Uncaught species (level 0) use a separate, slower catch fill (`GameBalance.Routes.CatchSpeedMultiplier`). After level 1, the same bar is used for training. First catch in the run gets a large catch-speed bonus.
- **Concurrency:** Only one route bar may be catching and only one may be training at a time (global, not per area).
- **Evolution:** Species with evolution data change name, primary type, and bar colors at level thresholds. Evolution is automatic when the bar level crosses a stage threshold — not player choice.
- **Route unlock:** Linear world order in `KantoProgressionCatalog.Order`. A route step completes when **any** species in that area reaches level ≥ 1 (`GameBalance.Routes.MinPokemonLevelToPassRoute`). Trainer steps complete when that trainer’s battle bar reaches level ≥ 1. Victory Road unlocks optionally after Giovanni.
- **Boss species:** Some routes use a harder catch multiplier (legendaries, Snorlax, etc.); training speed is unchanged.
- **Type counters:** Sixteen types shown at the bottom of the Routes tab. Route level-ups grant type points; dual typings split points between primary and secondary type keys in evolution data. Counter totals speed up battle bar fill.
- **Cross-mode bonus:** Clearing gym / Elite Four battle bars (their level = number of clears) speeds up route training; a fraction of that bonus applies to catch speed.

### Battles tab

- Thirteen trainer bars in order: eight Kanto gyms, Elite Four (Lorelei → Lance), then Blue.
- Same bar UI as routes, but no catch mode — only repeatable “clear” cycles.
- Trainers unlock in sync with `KantoProgressionCatalog.Order`.
- Battle difficulty scales per trainer index (`GameBalance.Battles`).

### Collections tab

- **Pokédex grid:** 10 × 15 cells for Kanto national dex #001–#150 (`KantoSpeciesCatalog`).
- Cells stay black until a matching route species has level ≥ 1; evolved forms light up as stages are reached. Fill color is the species’ **primary** type accent (`TypeCatalog`).
- No badges, shinies, or per-cell metadata yet — only fill color.

### Static content (today’s pain points)

Game content is spread across several large static lists under `ViewModels/`:

| Data | Location |
|------|----------|
| Dex order, evolution chains, standalone palettes | `Game/Content/KantoSpeciesCatalog.cs` |
| Route definitions + spawns | `Game/Content/KantoRouteCatalog.cs` |
| Trainer lineup | `Game/Content/KantoTrainerCatalog.cs` |
| Unlock order (stable route/trainer ids) | `Game/Content/KantoProgressionCatalog.cs` |
| Type colors + counter list | `Game/Content/TypeCatalog.cs` |
| Balance knobs | `ViewModels/GameBalance.cs` |

Progression and content are linked by **display name strings** (e.g. `"Route 1"`, `"Brock"`). Typos fail silently.

### Intentional simplifications

- **Types:** Only sixteen types are tracked — the set relevant to Kanto content. Steel and Dark are omitted (no pure Steel/Dark species in this scope; Magnemite line uses Electric as primary). Secondary types exist in evolution data for type-point splitting but are not fully modeled elsewhere.
- **Eevee (Celadon):** Intentional exception. Evolution data covers Eevee → Vaporeon at level 25. Flareon and Jolteon are separate hidden bars that unlock when Eevee reaches that threshold; they are granted at level 25 without a normal catch flow. This is special-case code in `MainViewModel`, not a general branching-evolution system. Future catalog work should leave room for one-off rules like this.

### Not implemented

- Save / load (all progress is in memory for the session).
- Prestige, currency, shop, achievements.
- Multiple simultaneous training targets.
- Shiny Pokémon, challenge run modes, additional regions.

---

## Planned design update

Near-term **code structure** work — not new player-facing features. Goal: pull game logic and content out of view models, without a big-bang rewrite.

### Problems we are fixing

1. **Duplicated catalogs** — species names, colors, routes, and progression authored in multiple places; values can drift.
2. **Game logic in VMs** — `MainViewModel` and `PokemonTrainingBarViewModel` mix simulation, unlock rules, and UI binding.
3. **No stable state shape** — progress lives on view models wired with callbacks; hard to test or serialize later.

### Target layering (incremental)

```
Content/     read-only catalogs (species, routes, trainers, progression, types)
Game/        rules + runtime state (when introduced)
ViewModels/  thin bindings to state
Views/       Avalonia UI
```

Keep `GameBalance` and `MagicNumbersUI` as the homes for tunable numbers and presentation constants (may move folders later).

### Refactor phases (small steps)

Do these in order; each step should keep the game playable.

**Step 1 — Content catalogs (next code change)**  
Consolidate static lists into `Ascendex/Game/Content/` (or similar):

- Single Kanto species table: dex #, name, evolution stages (types + colors), boss/catch flags.
- Route table: id, labels, spawn list referencing species.
- Trainer table: id, name, type, order.
- Progression graph: ordered steps with **stable ids**, not display-name dictionary keys.

Remove duplicate palette dictionaries from `MainViewModel`. Eevee’s Celadon behavior stays explicit special-case logic until we need a second exception.

**Step 2 — Separate runtime state from VMs**  
Introduce plain state objects (e.g. per-species level/progress/activity flags, type counter totals, selected area). View models become projections updated from that state. Enables unit tests on rules without Avalonia.

**Step 3 — Move simulation off bar VMs**  
Relocate tick/level-up/evolution/type-point logic out of `PokemonTrainingBarViewModel` into game-layer types. Exact shape (central tick vs per-entity) can be decided in Step 2; default behavior stays one active catch + one active train.

Steps 2–3 prepare for save/load but **do not require implementing save/load yet**.

### Data model sketch (for Step 2+)

Lightweight targets — names flexible:

| Concept | Role |
|---------|------|
| `SpeciesId`, `RouteId`, `TrainerId` | Stable keys for catalogs and save data |
| `SpeciesDefinition` | Dex #, stages, palettes, catch rules |
| `RouteDefinition` | Spawns, display metadata |
| `TrainerDefinition` | Type, order, battle pacing inputs |
| `SpeciesProgress` | Level, progress, catching/training flags |
| `RunState` | All progress for the current play session / run |
| `SaveGame` | Versioned `{ run, … }` blob when persistence is added |

Meta-progression (`MetaState`: badges, prestige, dex ownership bits) waits until a feature needs it — no empty scaffolding required upfront.

### Out of scope for this refactor pass

- Save/load implementation  
- Central tick / offline progress  
- Multi-training slots  
- Badges UI, shop, shinies, new regions  
- Generalizing Eevee into a branching-evolution framework  

---

## Planned future features

Ideas on the horizon — **not scheduled**. Listed so catalog and state design do not paint us into a corner.

| Feature | Notes |
|---------|--------|
| **Save / load** | Needs versioned `RunState` (and eventually meta state). Blocked on Step 2. |
| **Badges in Collections** | Track gym/badge completion separately from battle bar level; display in Collections tab. |
| **Separate catch / train tabs** | UI split; may share one species progress object with two activity modes. |
| **Prestige / resets** | Run boundary + persistent bonuses; likely triggered post–Elite Four or dex milestones. |
| **Multi-training** | Replace global single-active-bar rule with configurable training slots. |
| **Shiny Pokémon** | Per-species or per-instance flags beyond dex fill color. |
| **Challenge runs** | Run config (e.g. solo type) filtering available content. |
| **Shop / Pokédollars** | Currency + items; new tab or section. |
| **Johto, Hoenn, …** | Region catalog, regional dex layout, progression graph per region. |

When picking up a feature, check whether Step 1–2 are far enough along; most of these extend `RunState` / `MetaState` rather than `MainViewModel` special cases.

---

## Reference

- **Inspiration:** EthosIdle-style progression lanes, Pokémon theming.
- **Code map:** `ViewModels/MainViewModel.cs` (orchestration), `PokemonTrainingBarViewModel.cs` (bar simulation + UI), `Game/Content/*` (catalogs), `GameBalance.cs`.
