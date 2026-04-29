# Audit Notes

Checked against the included data files:

- 50 citizens.
- 25 buildings and locations.
- 3-5 schedule blocks per citizen.
- Schedule location IDs resolve to `city_buildings.json`.
- Route graph edge references resolve to route graph node IDs.
- `B02_CAVE_CULVERT` and `B25_EXIT_TUNNEL` are special entry/exit locations and are intentionally not daily schedule destinations.

Gameplay correction made in v2:

- The pack now connects route choices, loot, escape, distractions, and alarm pressure to the city loop.
- The pack no longer includes the image-to-3D model pipeline. It only documents how to import finished model outputs.
