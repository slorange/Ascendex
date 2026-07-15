# Ascendex — Game Design

Living design doc for the project. **Describes what the build does today** and ideas on the horizon. Older brainstorming in git history should be treated skeptically if it contradicts the **Current features** section.

**High concept:** Pokémon-themed incremental game (EthosIdle-style progression lanes) built with Avalonia. Kanto-first scope.

**Design principles**

- Theme first — mechanics should feel Pokémon-native, not renamed tech trees.
- Clarity over obscurity — unlock rules should be understandable in play.
- Satisfying growth — meaningful gains every few minutes.
- Scalable systems — today’s prototype should not block regions, prestige, or collections meta later.

---

## Current features

What the app actually does today. Balance numbers live in `Ascendex/Game/GameBalance.cs`; UI-only constants in `ViewModels/MagicNumbersUI.cs`; persistence settings in `Game/Save/SaveGameSettings.cs`.

### Platform and shell

- Avalonia UI targeting Desktop, Android, iOS, and Browser.
- `MainViewModel.Create()` loads save via `SaveGameService`; `App.axaml.cs` wires the main view.
- Bottom tabs: **Routes**, **Battles**, **Shop**, **Collections**, **Prestige**.
- Bank-time banner at top when offline time is stored or being spent at 3× speed.

### Architecture (current)

```
Game/Content/   read-only catalogs (species, routes, trainers, progression, types, badges)
Game/           RunState, GameSession, TrainingSimulator, rules, GameBalance
Game/Save/      versioned JSON save (v4), periodic auto-save, Android OnPause flush
ViewModels/     thin bindings, GameTickLoop, MagicNumbersUI
Views/          Avalonia UI (Routes, Battles, Shop, Collections, Prestige)
```

Content catalogs, runtime state + `GameSession`, and simulation off bar VMs are in place. Meta progression (Exp Share, Shiny Charms, lifetime shinies) lives on `RunState` today — no separate `MetaState` yet.

### Routes tab

- **Areas:** 22 Kanto locations (Pallet Town through Cerulean Cave), selected via a horizontal strip of short labels (PT, R1, VF, …).
- **Progress bars:** One bar per catchable/trainable species in the current area. Tap a bar to activate it.
- **Progress model:** Timer-driven fill (~16 ms tick). When the bar fills, the species gains a level and the bar resets. Required progress scales with level (`GameBalance.Training`).
- **Catch vs train:** Uncaught species (level 0) use a separate, slower catch fill (`GameBalance.Routes.CatchSpeedMultiplier`). After level 1, the same bar is used for training. First catch in the run gets a large catch-speed bonus.
- **Concurrency:** Only one route bar may be catching at a time (global). Training slots = 1 + Exp Share count; tapping a new species while full drops the oldest trainee (FIFO).
- **Evolution:** Species with evolution data change name, primary type, and bar colors at level thresholds. Evolution is automatic when the bar level crosses a stage threshold — not player choice. Shiny species use the stage’s shiny palette for the bar fill.
- **Route unlock:** Linear world order in `KantoProgressionCatalog.Order`, keyed by stable `RouteIds` / `TrainerIds`. A route step completes when **any** species in that area reaches level ≥ 1. Trainer steps complete when that trainer's battle bar reaches level ≥ 1. Victory Road unlocks optionally after Giovanni.
- **Boss species:** Some routes use a harder catch multiplier (legendaries, Snorlax, etc.); training speed is unchanged.
- **Type counters:** Sixteen types shown at the bottom of the Routes tab (no Steel/Dark). Route level-ups grant type points; dual typings split points between primary and secondary type keys in evolution data. Counter totals speed up battle bar fill.
- **Cross-mode bonus:** Clearing gym / Elite Four battle bars (their level = number of clears) speeds up route training; a fraction of that bonus applies to catch speed.
- **Shiny UI:** Caught shinies show `shiny.png` next to the species name on the bar.

### Battles tab

- Thirteen trainer bars in order: eight Kanto gyms, Elite Four (Lorelei → Lance), then Blue.
- Same bar UI as routes, but no catch mode — only repeatable "clear" cycles.
- Only one trainer may be active at a time.
- Trainers unlock in sync with `KantoProgressionCatalog.Order`.
- Battle difficulty scales per trainer index (`GameBalance.Battles`).
- Tab button shows a mini progress bar while a battle is active.

### Collections tab

- **Pokédex grid:** 15 × 10 square cells for Kanto national dex #001–#150 (`KantoSpeciesCatalog`).
- Cells stay black until a matching route species has level ≥ 1; evolved forms light up as stages are reached. Fill color is the species’ **primary type** (`TypeCatalog`), not the route-bar palette.
- **Shinies in the dex:** Gold border when that dex entry is shiny in the current run; fill stays type-colored.
- **Tooltips:** Species name, level, shiny status, and lifetime type stats (hover on desktop; tap flyout on mobile).
- **Badges:** Two rows under the dex — **Gym Badges** (8) and **Indigo League** (Elite Four + Champion). Earned when the linked trainer bar reaches level ≥ 1. Earned slots use the trainer’s type color and a tier-specific border; unearned slots are dim placeholders. Tooltips show catch-speed bonus from clears. Custom art is still planned (see below).

### Shop tab

- Bottom tab appears once **Cerulean / Misty** unlocks (not shown earlier).
- Single flat item list (no city headers). Items still unlock with their gym / Indigo gate and appear as those gates open.
- **Pokédollars** from each trainer clear: `floor(Base × IndexScale^trainerIndex × clearCount)` (`GameBalance.Shop`). Wiped on prestige.
- **Balls / X-items / evo items:** one-time buy for the run; wiped on prestige. Best owned ball multiplies catch speed; each X-item multiplies battle speed by 1.5× (product stack). Evolution stones / Link Cable can be bought but have no gameplay effect yet.
- **Vitamins (Indigo):** consumable → unassigned pool → apply to a caught species family. **Buy All** spends current ₽ on as many vitamins as possible. Training speed `1 + doses × 0.05` (cap **20** normal / **50** bosses). Apply list is route order with bosses last; names show current evolution stage. Doses persist across prestige. After the first vitamin purchase, the Apply Vitamins section stays visible across resets (even at 0 unassigned).
- Prices and multipliers live in `GameBalance.Shop`; catalog in `KantoShopCatalog`.

### Prestige tab

- **Meta inventory:** Exp Share count and Shiny Charm count.
- **Champion Reset:** Unlocks after Blue reaches level ≥ 1. Increments champion-reset count, recomputes Exp Shares (triangular: 1st reset → 1 share, after 3 total → 2, after 6 → 3, …), then resets run progress.
- **Pokedex Reset:** Unlocks when all 150 dex cells are caught. Increments pokedex-reset count, grants +1 Shiny Charm, then resets run progress.
- **Planned later resets (not implemented):** **Shiny Reset** (after Pokedex / shiny track), then **Vitamin Reset** after that (4th prestige layer — wipe or re-spec vitamin doses; details TBD).
- **Reset clears:** Species/trainer levels and progress, type counters, selected route (back to Pallet), Celadon Eevee unlock, training queue, current-run shiny flags on species, Pokédollars, and owned shop upgrades.
- **Carries over:** Bank time, prestige counts, Exp Shares, Shiny Charms, lifetime shiny species roots, pending guaranteed shiny count, vitamin doses (until Vitamin Reset exists).

### Shinies

- Rolled on **first catch only** (level 0 → 1) via `ShinyRules`.
- Base rate `1 / 100` (`GameBalance.Shinies.BaseCatchRate`). Each Shiny Charm multiplies: effective rate = base × (1 + charmCount × multiplier).
- Species roots ever caught shiny are stored in `LifetimeShinySpeciesRoots`; after a prestige reset, catching that species again is **guaranteed shiny**.
- Guaranteed catches still roll; if the roll also hits, `PendingGuaranteedShinies` increments so the extra shiny applies to a later catch (can stack).
- Pending guarantees are not shown in the UI yet.

### Save / load and offline time

- **Save file:** `%LocalApplicationData%/Ascendex/save.json` (app-private on Android and Windows).
- **Auto-save:** Every 5 seconds while playing (`SaveGameSettings.AutoSaveInterval`); flush on dispose (desktop close) and Android `OnPause`.
- **Versioned DTO (v5):** Species/trainer progress (including `IsShiny`), type counters, selected route/tab, Celadon unlock, bank time, prestige meta, training order, shop (Pokédollars, owned items, unassigned vitamins, vitamin doses).
- **Offline model:** Time away adds to **bank time** (capped at 24 h). While bank remains and a bar is active, simulation runs at **3×** and bank drains at 3 seconds per real second (24 h bank ≈ 8 h of boosted play). Instant offline tick catch-up is intentionally disabled.

### Static content

| Data | Location |
|------|----------|
| Dex order, evolution chains, normal/shiny palettes | `Game/Content/KantoSpeciesCatalog.cs` |
| Route definitions + spawns | `Game/Content/KantoRouteCatalog.cs` |
| Trainer lineup | `Game/Content/KantoTrainerCatalog.cs` |
| Badge / league honor definitions | `Game/Content/KantoBadgeCatalog.cs` |
| Unlock order (stable ids) | `Game/Content/KantoProgressionCatalog.cs` |
| Type colors + counter list | `Game/Content/TypeCatalog.cs` |
| Balance knobs | `Game/GameBalance.cs` |

### Intentional simplifications

- **Types:** Sixteen types tracked — the set relevant to Kanto content. Steel and Dark omitted for this scope.
- **Eevee (Celadon):** Flareon and Jolteon are hidden until Eevee hits the Vaporeon threshold; unlock and level grant live in `GameSession`, not a general branching-evolution framework.
- **Badge earn:** Tied to trainer bar level ≥ 1; no separate `BadgeEarned` save flags yet.
- **`ChampionResetUnlocked`:** Persisted after the first champion reset but not consulted by other systems yet.

### Not implemented

- Achievements, challenge run modes, additional regions.
- Final badge / league honor artwork (placeholders use type color + border tier).
- Separate badge-earned flags in save (uses trainer level today).
- Separate catch / train tabs (mode is by species level on Routes).
- iOS lifecycle save flush (Android `OnPause` only).
- Evolution stones / Link Cable **effects** (purchasable stubs only).
- Master Ball, Amulet Coin, held items.

---

## Shop design (live core; polish TBD)

Core shop loop is **implemented**. Held items deferred. Master Ball last among items. Amulet Coin later. Evolution stones / Link Cable mechanical effect still parked (items are purchasable stubs).

All balance multipliers, prices, and dollar payouts live in `GameBalance.Shop`.

### UI

New bottom tab (**Shop**). Tab is hidden until **Cerulean (Misty)** unlocks. Items listed in one flat shop (no city section headers); each item still gated by its gym / Indigo unlock.

### Unlock

Shops unlock **with the matching gym / league gate** (same progression as that trainer becoming available). Pewter exists as a location but sells nothing — Brock alone is enough new content there.

| Shop | Unlocks with | Stock |
|------|--------------|-------|
| Pewter | Brock | *(empty)* |
| Cerulean | Misty | Great Balls |
| Vermilion | Lt. Surge | X Attack |
| Celadon | Erika | Evolution stones, Ultra Balls, X Defense |
| Fuchsia | Koga | Dusk Balls |
| Saffron | Sabrina | Link Cables, X Special |
| Cinnabar | Blaine | Quick Balls, X Speed |
| Viridian | Giovanni | Timer Balls |
| Indigo Plateau | Giovanni (same gate as Lorelei) | Vitamins |

Indigo unlocks **after Giovanni** — same progression gate as Lorelei / first E4.

### Currency

- **Pokédollars**, earned from **beating trainers** (each battle-bar clear).
- Payout **scales with trainer index and clear count** (exact curve in `GameBalance`).
- **Resets on prestige** (Champion / Pokedex reset) — does **not** carry over.
- Item prices: TBD, in `GameBalance`.
- **Amulet Coin** later (not in this shop pass) — boosts dollar earn; candidate reward around a future shiny-oriented reset.

### Purchase model

| Family | Model | Prestige |
|--------|--------|----------|
| **Poké Balls** | One-time purchase → owned upgrade (toggle-style: bought = active) | Run-scoped: **lost on reset** (re-buy) |
| **X-items** | One-time purchase → owned buff (same toggle-style) | Run-scoped: **lost on reset** |
| **Vitamins** | **Consumable**; applied to a **specific species family** | Effect **persists** across resets |
| **E-stones / Link Cables** | Same handling as each other | Buy model + gameplay effect **TBD** (parked) |
| **Master Ball** | Special case | **Last** item to design/implement |

“Toggle” here means: you buy the upgrade once and it stays on for the run — not a stack of consumable uses. (UI on/off switch optional later; default is always-on once owned.)

### Catch balls (multipliers)

Applied to catch speed. Numbers → `GameBalance`. **Best owned ball wins** (not product of all owned balls).

| Ball | Catch multiplier |
|------|------------------|
| (default) | 1.0 |
| Great | 1.5 |
| Ultra | 2 |
| Dusk | 3 |
| Quick | 4 |
| Timer | 5 |
| Master | *special — later* |

### X-items (multipliers)

All X-items multiply the **same axis: battle speed**. Each owned item is a separate **1.5×**; they stack by product (four owned ≈ **1.5⁴ ≈ 5.06×** battle speed).

| Item | Shop | Multiplier |
|------|------|------------|
| X Attack | Vermilion | 1.5× battle |
| X Defense | Celadon | 1.5× battle |
| X Special | Saffron | 1.5× battle |
| X Speed | Cinnabar | 1.5× battle |

### Evolution items

**Evolution stones (Celadon) and Link Cables (Saffron) use the same rules** and can ship in the same pass. Mechanical effect and buy model **parked** for a later decision (auto-evolution + Celadon Eevee special case).

### Vitamins (Indigo Plateau)

- Consumable, **per species family** (root).
- Weaker than X-items; **persist across prestige** (until a future Vitamin Reset).
- Cap **20** doses per normal family, **50** for boss species (`GameBalance.Shop`).
- Apply UI: route order, bosses last; show current evolution name.
- **Buy All** spends all affordable vitamins from current Pokédollars.

### Persistence sketch

| Data | On prestige reset |
|------|-------------------|
| Pokédollars | **Wiped** |
| Owned balls / X-items | **Wiped** (re-buy next run) |
| Vitamin levels per family | **Kept** |
| E-stones / Link Cables | TBD with their buy model |

### Still open

- E-stones / Link Cables: buy model + interaction with auto-evo / Eevee (parked).
- Exact dollar payout formula and item prices (`GameBalance`).
- Vitamin bonus per dose / caps.
- How ball catch multipliers stack with existing gym / first-catch bonuses (product vs replace — likely product).

### Implementation todo (remaining)

1. ~~New Shop bottom tab + shell wiring~~  
2. ~~Currency from trainer clears + wipe on prestige + save fields~~  
3. ~~Shop catalog + unlock gates~~  
4. ~~Ball upgrades + best-owned catch multiplier~~  
5. ~~X-items (1.5× battle each)~~  
6. ~~Vitamins (consumable, per family, meta)~~  
7. E-stones + Link Cables **effects** (after design unpark)  
8. Master Ball (last)  
9. Amulet Coin / shiny-reset reward (later)  
10. Held items (separate discussion)  
11. Tune `GameBalance.Shop` prices / payout curve in play

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

Ideas on the horizon — **not scheduled** unless noted.

| Feature | Notes |
|---------|-------|
| **Shop polish** | See **Shop design** — evo-item effects, Master Ball, Amulet Coin |
| **Shiny Reset** | Planned 3rd prestige layer (after Pokedex / shiny track); Amulet Coin candidate reward |
| **Vitamin Reset** | Planned **4th** prestige layer, after Shiny Reset — re-spec / wipe vitamin doses (TBD) |
| **Amulet Coin** | Later earn multiplier; candidate shiny-reset reward |
| **Held items** | Deferred; own design pass after core shop |
| **Badge artwork** | PNG/SVG per `BadgeDefinition`; gym vs league templates in XAML |
| **Separate catch / train tabs** | UI split; may share one species progress object |
| **Challenge runs** | Run config filtering available content |
| **Johto, Hoenn, …** | Region catalog, regional dex, progression graph |
| **iOS lifecycle save** | Same pattern as Android `OnPause` |
| **MetaState split** | Peel prestige/shiny/shop meta off `RunState` if meta grows |

Most features extend `RunState` / a future `MetaState` rather than `MainViewModel` special cases.

---

## Reference

- **Inspiration:** EthosIdle-style progression lanes, Pokémon theming.
- **Code map:** `ViewModels/MainViewModel.cs` (orchestration), `Game/GameSession.cs` (rules + state), `Game/TrainingSimulator.cs`, `Game/ShinyRules.cs`, `Game/Save/*`, `Game/Content/*`, `Game/GameBalance.cs`.
