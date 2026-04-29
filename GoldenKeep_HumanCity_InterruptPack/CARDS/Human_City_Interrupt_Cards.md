# Golden Keep Interrupt Project - First Human City v2

Do these cards top to bottom. Hamza is the default owner. David input is optional except where image/art choice is required.

## CITY-001 - Pause Current Phase Safely
Owner: Hamza
Goal: Pause Phase 3 at card 38 without losing progress.
Steps:
1. Create branch `interrupt-human-city-v1`.
2. Create `Assets/_GoldenKeep/Docs/HumanCity/HUMAN_CITY_INTERRUPT_START.txt`.
3. Write current card number, current build status, and known bugs.
4. Commit.
Done when: branch and note exist.
Do not: continue card 39 yet.

## CITY-002 - Create Human City Folder Structure
Owner: Hamza
Goal: Organize the city project.
Steps:
1. Create `Assets/_GoldenKeep/Scenes/Cities/`.
2. Create `Assets/_GoldenKeep/Docs/HumanCity/`.
3. Create `Assets/_GoldenKeep/Data/HumanCity/`.
4. Create `Assets/_GoldenKeep/Art/HumanCity/`.
5. Create `Assets/_GoldenKeep/Prefabs/HumanCity/`.
6. Create `Assets/_GoldenKeep/Scripts/HumanCity/`.
Done when: folders exist.
Do not: move unrelated files.

## CITY-003 - Import City Data And Scripts
Owner: Hamza
Goal: Add generated city data and scripts to Unity.
Steps:
1. Copy `city_buildings.json` to `Assets/_GoldenKeep/Data/HumanCity/`.
2. Copy `city_citizens.json` to `Assets/_GoldenKeep/Data/HumanCity/`.
3. Copy `route_graph.json` to `Assets/_GoldenKeep/Data/HumanCity/`.
4. Copy `city_layout_ascii.md` to `Assets/_GoldenKeep/Docs/HumanCity/`.
5. Copy all files from `UNITY_SCRIPTS/` to `Assets/_GoldenKeep/Scripts/HumanCity/`.
Done when: data and scripts exist in project.
Do not: edit data during import.

## CITY-004 - Create City Scene
Owner: Hamza
Goal: Create the first human city test scene.
Steps:
1. Create scene `Assets/_GoldenKeep/Scenes/Cities/HUMAN_CITY_01.unity`.
2. Add root object `HUMAN_CITY_01`.
3. Add child roots: `CITY_MAIN_STREET`, `CITY_ROOFTOPS`, `CITY_SEWER`, `CITY_BUILDINGS`, `CITY_CITIZENS`, `CITY_SYSTEMS`, `CITY_TRIGGERS`.
4. Save scene.
Done when: scene and roots exist.
Do not: add citizens yet.

## CITY-005 - Block Out Three City Routes
Owner: Hamza
Goal: Build minimal route structure.
Steps:
1. Build one horizontal main street with gray colliders.
2. Build one upper rooftop path above it.
3. Build one lower sewer/back-alley path below it.
4. Add two vertical connections between layers.
5. Place player start near left cave/culvert.
6. Save.
Done when: player can traverse all three routes.
Do not: add art yet.

## CITY-006 - Place Building Waypoints
Owner: Hamza
Goal: Create schedule targets for citizens.
Steps:
1. Add `CityWaypoint` script to 25 empty objects.
2. Name each object using building IDs from `city_buildings.json`.
3. Set each `waypointId` to the matching building ID.
4. Place waypoints roughly along main street, rooftops, and sewer.
Done when: all 25 building IDs exist as waypoints.
Do not: create extra waypoints yet.

## CITY-007 - Add City Systems
Owner: Hamza
Goal: Add basic city simulation systems.
Steps:
1. Add `CityClock` to `CITY_SYSTEMS`.
2. Add `CityAlarmDirector` to `CITY_SYSTEMS`.
3. Add `CityRaidObjective` to `CITY_SYSTEMS`.
4. Set day length to 600 seconds.
5. Press Play and confirm clock runs.
Done when: clock, alarm, and objective systems exist.
Do not: create UI yet.

## CITY-008 - Create Citizen Prefabs
Owner: Hamza
Goal: Create simple citizen stand-ins.
Steps:
1. Create prefab `PF_Citizen_Civilian` with SpriteRenderer, Collider2D, Rigidbody2D, and `CitizenScheduleAgent`.
2. Create prefab `PF_Citizen_Guard` with the same components and a different color/sprite.
3. Set Rigidbody2D to kinematic if needed.
4. Save prefabs in `Prefabs/HumanCity/`.
Done when: two prefabs exist.
Do not: make 50 manual prefabs.

## CITY-009 - Spawn Citizens From Data
Owner: Hamza
Goal: Automate citizens using JSON.
Steps:
1. Add `CityScheduleManager` to `CITY_SYSTEMS`.
2. Assign `city_citizens.json` as TextAsset.
3. Assign civilian prefab.
4. Assign guard prefab.
5. Set citizens parent to `CITY_CITIZENS`.
6. Press Play.
Done when: citizens spawn and move toward schedule waypoints.
Do not: hand-place 50 citizens.

## CITY-010 - Add Alarm Sensors To Citizens
Owner: Hamza
Goal: Make city residents react to player presence.
Steps:
1. Add trigger collider child to civilian prefab.
2. Add `CitizenAlarmSensor` to that child.
3. Set civilian alarm amount to 1.
4. Add same to guard prefab with alarm amount 2.
5. Test player entering sensor.
Done when: alarm level rises when player is seen.
Do not: create complex vision cones yet.

## CITY-011 - Add Route Choice Triggers
Owner: Hamza
Goal: Connect the route graph to gameplay.
Steps:
1. Add `CityRouteChoiceTrigger` to main route nodes from `route_graph.json`.
2. Mark main gate/direct rush as `VeryHigh`.
3. Mark sewer/back alley as `Low`.
4. Mark rooftop/watchtower choices as `Medium`.
5. Mark chapel bell and storehouse front as `High`.
Done when: entering route nodes changes alarm pressure.
Do not: make every route node perfect yet.

## CITY-012 - Create Main Objective
Owner: Hamza
Goal: Add the first city win condition.
Steps:
1. Add storehouse loot object near `B17_STOREHOUSE`.
2. Add `CityLootPickup` to the loot object.
3. Add escape trigger near `B25_EXIT_TUNNEL`.
4. Add `CityEscapeTrigger` to the escape trigger.
5. Player must collect loot then reach escape trigger.
Done when: loot plus escape loop works.
Do not: add multiple objectives yet.

## CITY-013 - Make Direct Rush Dangerous
Owner: Hamza
Goal: Ensure unplanned main-street rushing fails often.
Steps:
1. Place 3 guard citizens along main street.
2. Place 1 guard near barracks.
3. Place 1 guard near storehouse.
4. Put `VeryHigh` route trigger on the direct gate/main street rush.
5. Confirm full alarm makes guards pursue and civilians flee.
Done when: direct rush is very dangerous.
Do not: make it impossible with no alternate route.

## CITY-014 - Add Three Viable Planned Routes
Owner: Hamza
Goal: Ensure city has choices.
Steps:
1. Route A: sewer/back-alley path to storehouse.
2. Route B: rooftop path through chapel yard.
3. Route C: market distraction then gatehouse path.
4. Make each route physically traversable.
5. Add one risk to each route.
Done when: three routes can reach the objective.
Do not: build more than three routes now.

## CITY-015 - Add Distraction And Timing Hooks
Owner: Hamza
Goal: Make planned routes feel deliberate.
Steps:
1. Add `CityDistractionTrigger` at bakery smoke/noise.
2. Set `distractionId` to `bakery_smoke`.
3. Set a gatehouse route trigger to require `bakery_smoke`.
4. Add a barracks timing route trigger with low risk.
5. Test the market/gatehouse route with and without the distraction.
Done when: a distraction creates a safer timing window.
Do not: build a full stealth UI yet.

## CITY-016 - Add First Townsperson Art Pass
Owner: Hamza
Goal: Use provided human images or finished model outputs as stand-ins.
Steps:
1. Pick 5 townspeople images or finished outputs.
2. Clean 2D stand-ins in GIMP using the checklist.
3. Import sprites as Point / Compression None.
4. If finished models exist, follow `Provided_Model_Integration.md`.
5. Assign art to 5 visible citizen prefabs or instances.
Done when: 5 citizens have human-readable art.
Do not: convert all 50 yet.

## CITY-017 - City QA Pass
Owner: Hamza
Goal: Test the city prototype.
Steps:
1. Copy `Human_City_QA_Checklist.md` into docs.
2. Run every checkbox.
3. Create `HUMAN_CITY_QA_01.txt`.
4. List required fixes only.
Done when: QA file exists.
Do not: fix while testing.

## CITY-018 - Fix Required City QA Items
Owner: Hamza
Goal: Make the prototype playable.
Steps:
1. Open `HUMAN_CITY_QA_01.txt`.
2. Fix only required items.
3. Retest each fix.
4. Update QA file.
Done when: required city QA items pass.
Do not: add new features.

## CITY-019 - Human City Candidate Build
Owner: Hamza
Goal: Create a build of the city interrupt project.
Steps:
1. Add `HUMAN_CITY_01` to Build Settings.
2. Build to `Builds/GK_HumanCity_Prototype_01`.
3. Run build.
4. Test one direct rush and one planned route.
5. Create `HUMAN_CITY_BUILD_TEST_01.txt`.
Done when: build exists and has been run.
Do not: submit editor-only screenshots.
