# Human City Map Architecture

## Primitive

The primitive is a city ID:

- Building IDs such as `B17_STOREHOUSE`.
- Route node IDs such as `R05_BAKERY_DISTRACTION`.
- Citizen IDs such as `HCN_001`.
- Road areas generated from route nodes and route graph edges.
- Provided art sprites cropped from the source sheets in `Assets/Human Art`.

The scene, schedules, alarm triggers, objectives, and art all connect through those IDs. This keeps the map replaceable without rewriting the gameplay systems.

## Black Boxes

- Data: JSON files in `Assets/_GoldenKeep/Data/HumanCity/`.
- Runtime scene builder: `HumanCityRuntimeBootstrap` creates the playable map from data and art when the scene starts.
- Editor scene builder: `HumanCityMapBuilder` can bake the same idea into edit-time objects later.
- Portable prefab: `PF_HumanCityRuntimeMap` stores the bootstrap plus all data/art references for drop-in use in other scenes or projects.
- City art layer: provided sprite strips and panels fill the city while staying separate from colliders/triggers.
- Schedule simulation: `CityScheduleManager` and `CitizenScheduleAgent`.
- Alarm/objective loop: `CityAlarmDirector`, route triggers, loot, escape, and distraction scripts.
- Player road traversal: `HumanCityPlayerController` reads WASD input and stays inside `HumanCityRoadArea` bounds.
- Camera traversal view: `HumanCityCameraFollow` tracks the player inside city bounds.

Each box can be replaced as long as it keeps the same ID-based interface.

## Build Command

The current scene contains a `HumanCityRuntimeBootstrap` component. Press Play in `HUMAN_CITY_01` and it builds the map at runtime.

Optional edit-time bake command:

```text
Golden Keep > Human City > Rebuild HUMAN_CITY_01
```

Portable prefab commands:

```text
Golden Keep > Human City > Create Runtime Prefab
Golden Keep > Human City > Export Runtime Prefab Package
```

`Create Runtime Prefab` writes `Assets/_GoldenKeep/Prefabs/HumanCity/PF_HumanCityRuntimeMap.prefab`.
`Export Runtime Prefab Package` writes `HumanCityRuntimePrefab.unitypackage` with the prefab, runtime scripts, data, and referenced art.
Import that package into another Unity project, drop `PF_HumanCityRuntimeMap` into a scene, and press Play.

Validation command:

```text
Golden Keep > Human City > Validate Current Scene
```

This creates:

- `Assets/_GoldenKeep/Scenes/Cities/HUMAN_CITY_01.unity`
- `Assets/_GoldenKeep/Prefabs/HumanCity/PF_HumanCityRuntimeMap.prefab`
- `Assets/_GoldenKeep/Prefabs/HumanCity/PF_Citizen_Civilian.prefab`
- `Assets/_GoldenKeep/Prefabs/HumanCity/PF_Citizen_Guard.prefab`
- `Assets/_GoldenKeep/Prefabs/HumanCity/PF_Player_RaidTester.prefab`

The generated scene and prefab use the provided Human City art as the clean playable backdrop, while optional reference/debug layers can be toggled on from the bootstrap when layout work needs them.
