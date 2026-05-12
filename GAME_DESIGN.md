# Ascendex Game Design Notes

## High Concept

`Ascendex` is a Pokemon-themed incremental game for Android inspired by the broad progression feel of `EthosIdle`, but with different design priorities:

- stronger thematic identity through Pokemon regions, routes, gyms, and the Pokedex
- clearer progression and unlock rules
- satisfying idle/incremental growth without unnecessary friction
- long-term prestige and reset systems tied to Pokemon milestones

This document is the working source of truth for early ideas. It is intentionally rough and should evolve as the game becomes more concrete.

## Core Fantasy

The player starts small in `Pallet Town` with basic Pokemon training and gradually grows into a world-spanning trainer who:

- levels Pokemon and catches new ones
- unlocks new routes, towns, and regions
- builds teams around type synergies
- defeats gyms and the Elite Four
- completes the Pokedex over multiple runs
- uses resets/prestige to accelerate future progression

## Reference Inspiration

Primary inspiration: `EthosIdle`

Key borrowed idea:

- a list of tappable or assignable progression lanes that fill bars over time and level up when full

Key changes desired:

- replace abstract civilization techs with Pokemon training, catches, routes, and progression systems
- make unlock paths feel more thematic and intuitive
- support longer-term progression through gyms, dex completion, and prestige loops

## First Gameplay Loop

The first minutes of play should be simple and readable:

1. Start in `Pallet Town`
2. Choose or receive a starter Pokemon
3. Tap the active training target to fill a progress bar
4. When the bar completes, that Pokemon gains a level
5. Reaching certain level thresholds unlocks the next area or content
6. New routes introduce additional catchable Pokemon and progression lanes

Early example progression:

- `Pallet Town`
  - starter Pokemon progression bar
  - level starter to `5`
- unlock `Route 1`
  - catch or train `Pidgey`
  - catch or train `Rattata`
- continued progression unlocks the next town, route, or challenge

## Core Mechanics

### 1. Progress Bars

Each bar represents a progression lane, such as:

- training a Pokemon
- catching a Pokemon
- researching a type bonus
- preparing for a gym

Basic behavior:

- the player taps a lane to focus it early on
- progress fills over time or per tap
- when full, the lane gains a level/rank/count
- leveling a lane increases derived stats or unlocks content

Later upgrades may allow:

- multiple active lanes at once
- automation
- passive progress while idle
- strategic assignment of limited training slots

### 2. Unlock Rules

A simple early rule is useful:

- when a Pokemon or lane reaches a target value, the next location or lane unlocks

Examples:

- starter reaches level `5` -> unlock `Route 1`
- a route's core Pokemon reach level `10` -> unlock next tier/location
- clearing a gym -> unlock next badge tier and new mechanics

### 3. Stats and Bonuses

Pokemon, locations, and upgrades can feed into global stats.

Possible stat categories:

- training speed
- catch chance
- type effectiveness bonus
- money or resource generation
- experience gain
- gym preparation speed
- idle efficiency

### 4. Team and Type Systems

Pokemon should eventually matter beyond just being bars.

Planned uses:

- type matchups affecting gyms, routes, and bosses
- team composition bonuses
- unlocks gated by having enough strength in a given type
- region or route challenges that reward broader rosters instead of one overleveled favorite

## Long-Term Progression

### Regions and Route Progression

The world can be structured in layers:

- town or city hub
- nearby route progression
- gym challenge
- badge unlock
- next region segment

This gives a strong Pokemon identity while preserving the incremental tier structure.

### Gyms

Gyms can act as milestone walls and strategic checks.

Possible design:

- unlock once route requirements are met
- require minimum team strength or specific type coverage
- reward permanent account bonuses, badges, and new progression systems

### Elite Four

Beating the Elite Four is a natural prestige trigger.

Possible rewards:

- reset the current run
- gain a prestige currency
- unlock account-wide boosts
- accelerate early progression on future runs

### Pokedex Completion

The Pokedex is a second long-term meta progression layer.

Possible uses:

- unique completion bonuses
- unlock rare species
- provide prestige multipliers
- encourage diversified runs and route choices

### Achievements

Achievements can reinforce both short-term and long-term goals.

Examples:

- first level `10` Pokemon
- first gym clear
- full route completion
- first prestige
- catch a full type family
- complete a regional dex page

## Prestige and Reset Ideas

Potential reset layers:

### Run Reset

- triggered by Elite Four completion
- grants prestige currency
- improves future leveling, catch speed, or automation

### Dex Reset / Collection Milestones

- tied to Pokedex completion goals
- unlocks stronger persistent bonuses
- encourages full roster growth rather than a single optimal path

### Regional Progression Reset

- later-game option for larger gains and broader replayability

## Early MVP Scope

To keep the first playable version realistic, the MVP should stay narrow.

Suggested MVP:

- Android-first UI
- one main screen with progression bars
- `Pallet Town` start
- one starter Pokemon choice or a single fixed starter
- starter leveling bar
- unlock `Route 1` at level `5`
- `Pidgey` and `Rattata` as additional progression lanes
- simple passive or tap-based progress
- save/load local progress

Nice-to-have if still small:

- simple type tags
- very basic reset
- first gym placeholder

## Design Principles

These are the instincts behind the project and can guide future choices:

- theme first: mechanics should feel Pokemon-native, not just renamed tech trees
- clarity over obscurity: unlock rules should be visible and understandable
- satisfying growth: every few minutes should produce a meaningful gain
- strategic breadth: many Pokemon should matter, not just one best lane
- scalable systems: early mechanics should naturally grow into gyms, regions, and prestige

## Open Questions

These need answers before architecture hardens:

- Is progress primarily tap-driven, time-driven, or hybrid?
- Does the player directly train Pokemon, or assign them to activities?
- Are catches separate bars from training bars?
- Do Pokemon evolve automatically at thresholds or through player choice?
- How many simultaneous active lanes should the player get early vs late?
- Is combat simulated, abstracted, or mostly represented through progression checks?
- How closely should the project follow canon region order and Pokemon availability?
- Will this be portrait-only on Android?

## Next Planning Steps

The next useful docs to create are:

1. a tighter `MVP` spec with exact first-screen behavior
2. a progression map for `Pallet Town -> Route 1 -> first gym`
3. a data model sketch for Pokemon, routes, unlocks, and save data
4. a UI wireframe for the main incremental screen

## Immediate Build Suggestion

The best next implementation step is likely:

- build a single-screen prototype with three bars and one unlock condition

That would let us validate:

- whether the main loop feels good
- how much tapping vs passive progress is fun
- whether the Pokemon theme reads clearly in the UI
