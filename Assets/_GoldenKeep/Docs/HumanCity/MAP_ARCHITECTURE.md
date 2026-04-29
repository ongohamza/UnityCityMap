# Human City Map Architecture

## Primitive

The primitive is a city ID:

- Building IDs such as `B17_STOREHOUSE`.
- Route node IDs such as `R05_BAKERY_DISTRACTION`.
- Citizen IDs such as `HCN_001`.

The scene, schedules, alarm triggers, objectives, and art all connect through those IDs. This keeps the map replaceable without rewriting the gameplay systems.

## Black Boxes

- Data: JSON files in `Assets/_GoldenKeep/Data/HumanCity/`.
- Runtime scene builder: `HumanCityRuntimeBootstrap` creates the playable map from data and art when the scene starts.
- Editor scene builder: `HumanCityMapBuilder` can bake the same idea into edit-time objects later.
- Schedule simulation: `CityScheduleManager` and `CitizenScheduleAgent`.
- Alarm/objective loop: `CityAlarmDirector`, route triggers, loot, escape, and distraction scripts.
- Player test harness: `HumanCityPlayerController`.

Each box can be replaced as long as it keeps the same ID-based interface.

## Build Command

The current scene contains a `HumanCityRuntimeBootstrap` component. Press Play in `HUMAN_CITY_01` and it builds the map at runtime.

Optional edit-time bake command:

```text
Golden Keep > Human City > Rebuild HUMAN_CITY_01
```

Validation command:

```text
Golden Keep > Human City > Validate Current Scene
```

This creates:

- `Assets/_GoldenKeep/Scenes/Cities/HUMAN_CITY_01.unity`
- `Assets/_GoldenKeep/Prefabs/HumanCity/PF_Citizen_Civilian.prefab`
- `Assets/_GoldenKeep/Prefabs/HumanCity/PF_Citizen_Guard.prefab`
- `Assets/_GoldenKeep/Prefabs/HumanCity/PF_Player_RaidTester.prefab`

The generated scene uses `Assets/Human Art/Level Blueprint.png` as the visual reference backdrop and builds playable colliders, ladders, waypoints, route triggers, loot, escape, city systems, and test player on top.
