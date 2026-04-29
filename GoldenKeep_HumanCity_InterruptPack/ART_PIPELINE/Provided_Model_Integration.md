# Provided Model Integration

This pack does not own the image-to-3D conversion pipeline. Use the separate townspeople model process for that work, then bring only finished test outputs into this city prototype.

## First Import Batch

Import a tiny proof batch first:

1. Three civilian models.
2. One guard model.
3. One simple 2D fallback sprite for each imported model.

Do not wait for all 50 citizens to have models. The city loop should work with stand-ins before art production expands.

## Unity Setup

For each imported model output:

1. Put the asset under `Assets/_GoldenKeep/Art/HumanCity/Models/`.
2. Create or update a prefab under `Assets/_GoldenKeep/Prefabs/HumanCity/`.
3. Add or preserve `CitizenScheduleAgent`.
4. Add a child trigger with `CitizenAlarmSensor`.
5. Assign the prefab to the matching civilian or guard slot used by `CityScheduleManager`.

## Acceptance Check

The model is ready for this pack when it:

- Spawns through `CityScheduleManager`.
- Moves between city waypoints.
- Raises alarm when the player enters its sensor.
- Reacts during full alarm according to role.
- Has a fallback sprite or capsule stand-in if the model breaks.
