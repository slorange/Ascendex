# Ascendex — Game Design

Living design doc for the project. **Describes what the build does today**, recent structural work, and ideas on the horizon. Older brainstorming in git history should be treated skeptically if it contradicts the **Current features** section.

**High concept:** Pokémon-themed incremental game (EthosIdle-style progression lanes) built with Avalonia. Kanto-first scope.

**Design principles**

- Theme first — mechanics should feel Pokémon-native, not renamed tech trees.
- Clarity over obscurity — unlock rules should be understandable in play.
- Satisfying growth — meaningful gains every few minutes.
- Scalable systems — today's prototype should not block regions, prestige, or collections meta later.

---

## Current features

What the app actually does today. Balance numbers live in `Ascendex/Game/GameBalance.cs`; UI-only constants in `ViewModels/MagicNumbersUI.cs`; persistence settings in `Game/Save/SaveGameSettings.cs`.

### Platform and shell

- Avalonia UI targeting Desktop, Android, iOS, and Browser.
- `MainViewModel.Create()` loads save via `SaveGameService`; `App.axaml.cs` wires the main view.
- Bottom tabs: **Routes**, **Battles**, **Collections**.
- Bank-time banner at top when offline time is stored or being spent at 3× speed.

### Architecture (current)

```
Game/Content/   read-only catalogs (species, routes, trainers, progression, types, badges)
Game/           RunState, GameSession, TrainingSimulator, rules, GameBalance
Game/Save/      versioned JSON save, periodic auto-save, Android OnPause flush
ViewModels/     thin bindings, GameTickLoop, MagicNumbersUI
Views/          Avalonia UI
```

**Refactor steps 1–3 (done):** content catalogs, runtime state + `GameSession`, simulation off bar VMs.

### Routes tab

- **Areas:** 22 Kanto locations (Pallet Town through Cerulean Cave), selected via a horizontal strip of short labels (PT, R1, VF, …).
- **Progress bars:** One bar per catchable/trainable species in the current area. Tap a bar to activate it.
- **Progress model:** Timer-driven fill (~16 ms tick). When the bar fills, the species gains a level and the bar resets. Required progress scales with level (`GameBalance.Training`).
- **Catch vs train:** Uncaught species (level 0) use a separate, slower catch fill (`GameBalance.Routes.CatchSpeedMultiplier`). After level 1, the same bar is used for training. First catch in the run gets a large catch-speed bonus.
- **Concurrency:** Only one route bar may be catching and only one may be training at a time (global, not per area).
- **Evolution:** Species with evolution data change name, primary type, and bar colors at level thresholds. Evolution is automatic when the bar level crosses a stage threshold — not player choice.
- **Route unlock:** Linear world order in `KantoProgressionCatalog.Order`, keyed by stable `RouteIds` / `TrainerIds`. A route step completes when **any** species in that area reaches level ≥ 1. Trainer steps complete when that trainer's battle bar reaches level ≥ 1. Victory Road unlocks optionally after Giovanni.
- **Boss species:** Some routes use a harder catch multiplier (legendaries, Snorlax, etc.); training speed is unchanged.
- **Type counters:** Sixteen types shown at the bottom of the Routes tab (no Steel/Dark). Route level-ups grant type points; dual typings split points between primary and secondary type keys in evolution data. Counter totals speed up battle bar fill.
- **Cross-mode bonus:** Clearing gym / Elite Four battle bars (their level = number of clears) speeds up route training; a fraction of that bonus applies to catch speed.

### Battles tab

- Thirteen trainer bars in order: eight Kanto gyms, Elite Four (Lorelei → Lance), then Blue.
- Same bar UI as routes, but no catch mode — only repeatable "clear" cycles.
- Trainers unlock in sync with `KantoProgressionCatalog.Order`.
- Battle difficulty scales per trainer index (`GameBalance.Battles`).

### Collections tab

- **Pokédex grid:** 15 × 10 square cells for Kanto national dex #001–#150 (`KantoSpeciesCatalog`). Wider layout leaves vertical room for badges below.
- Cells stay black until a matching route species has level ≥ 1; evolved forms light up as stages are reached. Fill color is the species' **primary** type accent (`TypeCatalog`).
- **Badges (in progress):** Two rows under the dex — **Gym Badges** (8 canonical Kanto badges) and **Indigo League** (Elite Four + Champion). Earned slots fill with the trainer's type color and a tier-specific border; unearned slots are dim placeholders. Currently earned when the linked trainer bar reaches level ≥ 1 (same as progression "clear"); dedicated badge flags in save data can split later. Custom art for gym badges vs league honors is planned (see below).

### Save / load and offline time

- **Save file:** `%LocalApplicationData%/Ascendex/save.json` (app-private on Android and Windows).
- **Auto-save:** Every 5 seconds while playing (`SaveGameSettings.AutoSaveInterval`); flush on dispose (desktop close) and Android `OnPause`.
- **Versioned DTO:** Species/trainer progress, type counters, selected route/tab, Celadon unlock flag, bank time.
- **Offline model:** Time away adds to **bank time** (capped at 24 h). While bank remains and a bar is active, simulation runs at **3×** and bank drains at 3 seconds per real second (24 h bank ≈ 8 h of boosted play). Instant offline tick catch-up is intentionally disabled.

### Static content

| Data | Location |
|------|----------|
| Dex order, evolution chains, palettes | `Game/Content/KantoSpeciesCatalog.cs` |
| Route definitions + spawns | `Game/Content/KantoRouteCatalog.cs` |
| Trainer lineup | `Game/Content/KantoTrainerCatalog.cs` |
| Badge / league honor definitions | `Game/Content/KantoBadgeCatalog.cs` |
| Unlock order (stable ids) | `Game/Content/KantoProgressionCatalog.cs` |
| Type colors + counter list | `Game/Content/TypeCatalog.cs` |
| Balance knobs | `Game/GameBalance.cs` |

### Intentional simplifications

- **Types:** Sixteen types tracked — the set relevant to Kanto content. Steel and Dark omitted for this scope.
- **Eevee (Celadon):** Flareon and Jolteon are hidden until Eevee hits the Vaporeon threshold; unlock and level grant live in `GameSession`, not a general branching-evolution framework.

### Not implemented

- Prestige, currency, shop, achievements.
- Multiple simultaneous training targets.
- Shiny Pokémon, challenge run modes, additional regions.
- Final badge / league honor artwork (placeholders use type color + border tier).
- Separate badge-earned flags in save (uses trainer level today).

---

## Badge design (Collections)

Kanto has **8 gym badges** but **13 battle milestones** (8 gyms + 4 Elite Four + Champion). Treat these as two visual tiers, not 13 identical "badges":

| Tier | Count | Framing | Earned from |
|------|-------|---------|-------------|
| **Gym** | 8 | Classic badge silhouette (Boulder, Cascade, …) | Defeating each gym leader |
| **Elite Four** | 4 | Distinct **league honors** — medallions / sigils, not gym badges | Defeating each E4 member |
| **Champion** | 1 | Single **Champion emblem** (larger or unique border) | Defeating Blue |

**Why not reuse gym badge art for E4?** In canon, only gym leaders award badges. E4 and Champion are separate milestones; using gym-badge frames for Lorelei would feel wrong. Custom league art (even simple geometric icons per type) keeps the fiction clear.

**Catalog:** `KantoBadgeCatalog.GymBadges` and `LeagueHonors` with `BadgeTier` (`Gym`, `EliteFour`, `Champion`). Asset keys can be added when art exists; until then, type-colored cells + tier borders (`MagicNumbersUI.BadgeGrid`).

**Future:** Persist `BadgeEarned` separately from trainer bar level if clears become repeatable but badge grant should be one-time.

---

## Planned future features

Ideas on the horizon — **not scheduled**.

| Feature | Notes |
|---------|--------|
| **Badge artwork** | PNG/SVG per `BadgeDefinition`; gym vs league templates in XAML |
| **Separate catch / train tabs** | UI split; may share one species progress object |
| **Prestige / resets** | Run boundary + persistent bonuses |
| **Multi-training** | Replace global single-active-bar rule |
| **Shiny Pokémon** | Per-species flags beyond dex fill color |
| **Challenge runs** | Run config filtering available content |
| **Shop / Pokédollars** | Currency + items |
| **Johto, Hoenn, …** | Region catalog, regional dex, progression graph |
| **iOS lifecycle save** | Same pattern as Android `OnPause` |

Most features extend `RunState` / future `MetaState` rather than `MainViewModel` special cases.

---

## Reference

- **Inspiration:** EthosIdle-style progression lanes, Pokémon theming.
- **Code map:** `ViewModels/MainViewModel.cs` (orchestration), `Game/GameSession.cs` (rules + state), `Game/TrainingSimulator.cs`, `Game/Save/*`, `Game/Content/*`, `Game/GameBalance.cs`.
