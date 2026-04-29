using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HumanCityMapBuilder
{
    private static readonly bool ShowReferenceBackdrop = false;
    private static readonly bool ShowDetailOverlays = false;
    private static readonly bool ShowDebugGeometry = false;
    private const string ScenePath = "Assets/_GoldenKeep/Scenes/Cities/HUMAN_CITY_01.unity";
    private const string BuildingDataPath = "Assets/_GoldenKeep/Data/HumanCity/city_buildings.json";
    private const string CitizenDataPath = "Assets/_GoldenKeep/Data/HumanCity/city_citizens.json";
    private const string RouteGraphPath = "Assets/_GoldenKeep/Data/HumanCity/route_graph.json";
    private const string RuntimePrefabPath = "Assets/_GoldenKeep/Prefabs/HumanCity/PF_HumanCityRuntimeMap.prefab";
    private const string RuntimePackagePath = "HumanCityRuntimePrefab.unitypackage";
    private const string BackdropPath = "Assets/Human Art/Level Blueprint.png";
    private const string VisualReferencePath = "Assets/Human Art/Level 1 Blueprint best version so far.png";
    private const string CivilianSpritePath = "Assets/Human Art/1.png";
    private const string GuardSpritePath = "Assets/Human Art/10.png";
    private const string PlayerSpritePath = "Assets/Human Art/gargoyle.png";
    private const string SolidSpritePath = "Assets/_GoldenKeep/Art/HumanCity/Generated/solid_pixel.png";
    private const string SurfaceCityArtPath = "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_surface_city_strip.png";
    private const string UndergroundCityArtPath = "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_underground_city_strip.png";

    private static readonly string[] LocationArtPaths =
    {
        "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_farmstead_panel.png",
        "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_market_panel.png",
        "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_gate_panel.png",
        "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_castle_gate_panel.png",
        "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_forge_panel.png",
        "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_gate_panel.png",
        "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_underground_spawn_panel.png",
        "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_forge_panel.png",
        "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_secret_tunnel_panel.png"
    };

    private static readonly string[] PropArtPaths =
    {
        "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_market_props_strip.png",
        "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_gate_props_strip.png",
        "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_back_alley_props_strip.png",
        "Assets/_GoldenKeep/Art/HumanCity/Provided/provided_underground_props_strip.png"
    };

    private static readonly string[] RuntimePackageAssetPaths =
    {
        RuntimePrefabPath,
        "Assets/_GoldenKeep/Scripts/HumanCity/Runtime",
        "Assets/_GoldenKeep/Data/HumanCity",
        "Assets/_GoldenKeep/Art/HumanCity/Provided",
        BackdropPath,
        VisualReferencePath,
        CivilianSpritePath,
        GuardSpritePath,
        PlayerSpritePath,
        "Assets/_GoldenKeep/Docs/HumanCity/MAP_ARCHITECTURE.md"
    };

    private static readonly Dictionary<string, Vector2> RoutePositions = new Dictionary<string, Vector2>
    {
        { "R01_CAVE_EXIT", new Vector2(4f, -2.4f) },
        { "R02_SEWER_CULVERT", new Vector2(8f, -3.2f) },
        { "R03_NORTH_GATE", new Vector2(1f, 0.2f) },
        { "R04_MARKET_OBSERVE", new Vector2(18f, 0.2f) },
        { "R05_BAKERY_DISTRACTION", new Vector2(22f, 0.2f) },
        { "R06_BLACKSMITH_NOISE", new Vector2(28f, 0.2f) },
        { "R07_STABLE_PANIC", new Vector2(40f, -1.2f) },
        { "R08_BACK_ALLEY", new Vector2(36f, -2f) },
        { "R09_ROOFTOP_LINE", new Vector2(50f, 5.2f) },
        { "R10_CHAPEL_BELL", new Vector2(64f, 4.4f) },
        { "R11_CHURCH_YARD", new Vector2(62f, 0.2f) },
        { "R12_BARRACKS_ROTATION", new Vector2(48f, 0.2f) },
        { "R13_GATEHOUSE", new Vector2(74f, 0.2f) },
        { "R14_STOREHOUSE_FRONT", new Vector2(88f, 0.2f) },
        { "R15_STOREHOUSE_SEWER", new Vector2(88f, -4f) },
        { "R16_MAYOR_KEY", new Vector2(76f, 0.2f) },
        { "R17_RETURN_TUNNEL", new Vector2(96f, -2.5f) },
        { "R18_FULL_ALARM", new Vector2(54f, 1.4f) }
    };

    [MenuItem("Golden Keep/Human City/Rebuild HUMAN_CITY_01")]
    public static void RebuildHumanCity01()
    {
        EnsureProjectFolders();
        EnsurePlayerTag();
        ConfigureSpriteImport(BackdropPath, 16f);
        ConfigureSpriteImport(VisualReferencePath, 16f);
        ConfigureSpriteImport(CivilianSpritePath, 512f);
        ConfigureSpriteImport(GuardSpritePath, 512f);
        ConfigureSpriteImport(PlayerSpritePath, 512f);
        ConfigureProvidedArtImports();

        Sprite solidSprite = GetSolidSprite();
        CitizenScheduleAgent civilianPrefab = CreateCitizenPrefab("PF_Citizen_Civilian", CivilianSpritePath, new Color(0.86f, 0.78f, 0.58f), 1);
        CitizenScheduleAgent guardPrefab = CreateCitizenPrefab("PF_Citizen_Guard", GuardSpritePath, new Color(0.55f, 0.72f, 1f), 2);
        GameObject playerPrefab = CreatePlayerPrefab();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject root = CreateRoot("HUMAN_CITY_01", null);
        GameObject visualRoot = CreateRoot("CITY_VISUALS", root.transform);
        GameObject buildingRoot = CreateRoot("CITY_BUILDINGS", root.transform);
        GameObject routeRoot = CreateRoot("CITY_TRIGGERS", root.transform);
        GameObject citizenRoot = CreateRoot("CITY_CITIZENS", root.transform);
        GameObject systemsRoot = CreateRoot("CITY_SYSTEMS", root.transform);
        GameObject platformRoot = CreateRoot("CITY_PLATFORMS", root.transform);
        GameObject roadRoot = CreateRoot("CITY_ROADS", root.transform);

        Camera camera = CreateCamera();
        CreateBackdrop(visualRoot.transform);
        CreateProvidedArtwork(visualRoot.transform);
        CreatePlatforms(platformRoot.transform, solidSprite);
        CreateRoadNetwork(roadRoot.transform, solidSprite);
        Dictionary<string, Transform> waypoints = CreateBuildingWaypoints(buildingRoot.transform, solidSprite);
        CreateRouteTriggers(routeRoot.transform, solidSprite);
        CreateObjectiveObjects(routeRoot.transform, solidSprite);
        GameObject player = CreatePlayer(playerPrefab);
        if (player != null)
            AttachCameraFollow(camera, player.transform);
        CreateSystems(systemsRoot.transform, citizenRoot.transform, civilianPrefab, guardPrefab);
        CreateManifest(root);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Built human city map with " + waypoints.Count + " building waypoints at " + ScenePath);
    }

    [MenuItem("Golden Keep/Human City/Create Runtime Prefab")]
    public static void CreateRuntimePrefabMenuItem()
    {
        CreateRuntimePrefabAsset();
    }

    [MenuItem("Golden Keep/Human City/Export Runtime Prefab Package")]
    public static void ExportRuntimePrefabPackage()
    {
        GameObject prefab = CreateRuntimePrefabAsset();
        if (prefab == null)
        {
            Debug.LogError("Human City runtime prefab package export failed: prefab could not be created.");
            return;
        }

        AssetDatabase.ExportPackage(RuntimePackageAssetPaths, RuntimePackagePath, ExportPackageOptions.Recurse | ExportPackageOptions.IncludeDependencies);
        Debug.Log("Exported portable human city prefab package to " + RuntimePackagePath);
    }

    private static GameObject CreateRuntimePrefabAsset()
    {
        EnsureProjectFolders();
        ConfigureSpriteImport(BackdropPath, 16f);
        ConfigureSpriteImport(VisualReferencePath, 16f);
        ConfigureSpriteImport(CivilianSpritePath, 512f);
        ConfigureSpriteImport(GuardSpritePath, 512f);
        ConfigureSpriteImport(PlayerSpritePath, 512f);
        ConfigureProvidedArtImports();

        GameObject prefabRoot = new GameObject("PF_HumanCityRuntimeMap");
        HumanCityRuntimeBootstrap bootstrap = prefabRoot.AddComponent<HumanCityRuntimeBootstrap>();
        ConfigureRuntimeBootstrap(bootstrap);

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, RuntimePrefabPath);
        Object.DestroyImmediate(prefabRoot);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Created portable human city runtime prefab at " + RuntimePrefabPath);
        return savedPrefab;
    }

    private static void ConfigureRuntimeBootstrap(HumanCityRuntimeBootstrap bootstrap)
    {
        bootstrap.buildingsJson = AssetDatabase.LoadAssetAtPath<TextAsset>(BuildingDataPath);
        bootstrap.citizensJson = AssetDatabase.LoadAssetAtPath<TextAsset>(CitizenDataPath);
        bootstrap.routeGraphJson = AssetDatabase.LoadAssetAtPath<TextAsset>(RouteGraphPath);
        bootstrap.backdropSprite = LoadSpriteAsset(BackdropPath, 16f);
        bootstrap.civilianSprite = LoadSpriteAsset(CivilianSpritePath, 512f);
        bootstrap.guardSprite = LoadSpriteAsset(GuardSpritePath, 512f);
        bootstrap.playerSprite = LoadSpriteAsset(PlayerSpritePath, 512f);
        bootstrap.surfaceCityArtSprite = LoadSpriteAsset(SurfaceCityArtPath, 100f);
        bootstrap.undergroundCityArtSprite = LoadSpriteAsset(UndergroundCityArtPath, 100f);
        bootstrap.locationArtSprites = LoadSpriteAssets(LocationArtPaths, 100f);
        bootstrap.propArtSprites = LoadSpriteAssets(PropArtPaths, 100f);
        bootstrap.buildOnAwake = true;
        bootstrap.clearExistingGeneratedMap = true;
        bootstrap.showReferenceBackdrop = ShowReferenceBackdrop;
        bootstrap.showDetailOverlays = ShowDetailOverlays;
        bootstrap.showDebugGeometry = ShowDebugGeometry;
    }

    private static void EnsureProjectFolders()
    {
        string[] folders =
        {
            "Assets/_GoldenKeep",
            "Assets/_GoldenKeep/Art",
            "Assets/_GoldenKeep/Art/HumanCity",
            "Assets/_GoldenKeep/Art/HumanCity/Generated",
            "Assets/_GoldenKeep/Data",
            "Assets/_GoldenKeep/Data/HumanCity",
            "Assets/_GoldenKeep/Docs",
            "Assets/_GoldenKeep/Docs/HumanCity",
            "Assets/_GoldenKeep/Prefabs",
            "Assets/_GoldenKeep/Prefabs/HumanCity",
            "Assets/_GoldenKeep/Scenes",
            "Assets/_GoldenKeep/Scenes/Cities"
        };

        foreach (string folder in folders)
        {
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
        }
    }

    private static GameObject CreateRoot(string name, Transform parent)
    {
        GameObject root = new GameObject(name);
        if (parent != null)
            root.transform.SetParent(parent);
        return root;
    }

    private static Camera CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 6.2f;
        camera.backgroundColor = new Color(0.09f, 0.08f, 0.08f);
        camera.transform.position = new Vector3(48f, 0.5f, -20f);
        AssignTag(cameraObject, "MainCamera");
        return camera;
    }

    private static void CreateBackdrop(Transform parent)
    {
        if (!ShowReferenceBackdrop)
            return;

        Sprite backdrop = AssetDatabase.LoadAssetAtPath<Sprite>(BackdropPath);
        if (backdrop == null) return;

        GameObject backdropObject = new GameObject("VISUAL_REFERENCE_Level_Blueprint");
        backdropObject.transform.SetParent(parent);
        backdropObject.transform.position = new Vector3(48f, -4.2f, 8f);

        SpriteRenderer renderer = backdropObject.AddComponent<SpriteRenderer>();
        renderer.sprite = backdrop;
        renderer.sortingOrder = -100;
        renderer.color = new Color(1f, 1f, 1f, 0.65f);
    }

    private static void CreateProvidedArtwork(Transform parent)
    {
        CreateArtworkBand(parent, "ART_SurfaceCityCleanBand", AssetDatabase.LoadAssetAtPath<Sprite>(SurfaceCityArtPath), new Vector2(50f, 2.45f), new Vector2(112f, 8.2f), -92, Color.white);
        CreateArtworkBand(parent, "ART_UndergroundCityCleanBand", AssetDatabase.LoadAssetAtPath<Sprite>(UndergroundCityArtPath), new Vector2(50f, -4.25f), new Vector2(112f, 6.9f), -91, Color.white);

        if (!ShowDetailOverlays)
            return;

        CreateLocationArtwork(parent);
        CreatePropArtwork(parent);
    }

    private static void CreateLocationArtwork(Transform parent)
    {
        Vector2[] positions =
        {
            new Vector2(10f, 2.15f),
            new Vector2(24f, 2.05f),
            new Vector2(74f, 2.15f),
            new Vector2(17f, 1.1f),
            new Vector2(62f, 2.6f),
            new Vector2(84f, 2.2f),
            new Vector2(8f, -4.25f),
            new Vector2(31f, -4.25f),
            new Vector2(87f, -4.25f)
        };

        float[] widths = { 12f, 12f, 12f, 11f, 11f, 11f, 10f, 10f, 10f };
        int count = Mathf.Min(LocationArtPaths.Length, positions.Length);

        for (int i = 0; i < count; i++)
            CreateArtworkSprite(parent, "ART_Location_" + i, AssetDatabase.LoadAssetAtPath<Sprite>(LocationArtPaths[i]), positions[i], widths[i], -12, new Color(1f, 1f, 1f, 0.82f));
    }

    private static void CreatePropArtwork(Transform parent)
    {
        Vector2[] positions =
        {
            new Vector2(22f, 0.9f),
            new Vector2(74f, 0.95f),
            new Vector2(37f, -2.1f),
            new Vector2(54f, -3.8f)
        };

        float[] widths = { 16f, 14f, 14f, 14f };
        int count = Mathf.Min(PropArtPaths.Length, positions.Length);

        for (int i = 0; i < count; i++)
            CreateArtworkSprite(parent, "ART_Props_" + i, AssetDatabase.LoadAssetAtPath<Sprite>(PropArtPaths[i]), positions[i], widths[i], 2, new Color(1f, 1f, 1f, 0.8f));
    }

    private static void CreateArtworkSprite(Transform parent, string objectName, Sprite sprite, Vector2 position, float targetWidth, int sortingOrder, Color color)
    {
        if (sprite == null || targetWidth <= 0f)
            return;

        GameObject artObject = new GameObject(objectName);
        artObject.transform.SetParent(parent);
        artObject.transform.localPosition = new Vector3(position.x, position.y, 0f);

        SpriteRenderer renderer = artObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        float spriteWidth = Mathf.Max(0.01f, sprite.bounds.size.x);
        float scale = targetWidth / spriteWidth;
        artObject.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private static void CreateArtworkBand(Transform parent, string objectName, Sprite sprite, Vector2 position, Vector2 targetSize, int sortingOrder, Color color)
    {
        if (sprite == null || targetSize.x <= 0f || targetSize.y <= 0f)
            return;

        GameObject artObject = new GameObject(objectName);
        artObject.transform.SetParent(parent);
        artObject.transform.localPosition = new Vector3(position.x, position.y, 0f);

        SpriteRenderer renderer = artObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;

        Vector2 spriteSize = sprite.bounds.size;
        float scaleX = targetSize.x / Mathf.Max(0.01f, spriteSize.x);
        float scaleY = targetSize.y / Mathf.Max(0.01f, spriteSize.y);
        artObject.transform.localScale = new Vector3(scaleX, scaleY, 1f);
    }

    private static void CreatePlatforms(Transform parent, Sprite solidSprite)
    {
        CreateBox(parent, "MainStreet_Collider", new Vector2(48f, -0.65f), new Vector2(100f, 0.75f), solidSprite, new Color(0.24f, 0.19f, 0.15f, 0.35f), false);
        CreateBox(parent, "BackAlley_Collider", new Vector2(36f, -2.55f), new Vector2(42f, 0.55f), solidSprite, new Color(0.18f, 0.16f, 0.14f, 0.3f), false);
        CreateBox(parent, "Sewer_Collider", new Vector2(49f, -4.55f), new Vector2(92f, 0.7f), solidSprite, new Color(0.12f, 0.12f, 0.11f, 0.35f), false);
        CreateBox(parent, "Rooftop_West_Collider", new Vector2(28f, 4.75f), new Vector2(38f, 0.55f), solidSprite, new Color(0.35f, 0.25f, 0.16f, 0.32f), false);
        CreateBox(parent, "Rooftop_East_Collider", new Vector2(70f, 4.75f), new Vector2(46f, 0.55f), solidSprite, new Color(0.35f, 0.25f, 0.16f, 0.32f), false);

        CreateLadder(parent, "Ladder_Culvert_To_Main", new Vector2(8f, -2.45f), 3.8f, solidSprite);
        CreateLadder(parent, "Ladder_Alley_To_Rooftop", new Vector2(36f, 1.2f), 7.0f, solidSprite);
        CreateLadder(parent, "Ladder_Chapel_To_Sewer", new Vector2(62f, -2f), 5.3f, solidSprite);
        CreateLadder(parent, "Ladder_Storehouse_Grate", new Vector2(88f, -2.1f), 4.2f, solidSprite);
    }

    private static void CreateRoadNetwork(Transform parent, Sprite solidSprite)
    {
        foreach (KeyValuePair<string, Vector2> route in RoutePositions)
            CreateRoadArea(parent, solidSprite, "ROAD_NODE_" + route.Key, route.Key, string.Empty, route.Value, new Vector2(2.4f, 2.4f));

        TextAsset routeGraphJson = AssetDatabase.LoadAssetAtPath<TextAsset>(RouteGraphPath);
        if (routeGraphJson == null)
            return;

        CityRouteGraphDatabase graph = JsonUtility.FromJson<CityRouteGraphDatabase>(routeGraphJson.text);
        if (graph == null || graph.edges == null)
            return;

        foreach (CityRouteEdgeRecord edge in graph.edges)
        {
            if (!RoutePositions.TryGetValue(edge.from, out Vector2 from) || !RoutePositions.TryGetValue(edge.to, out Vector2 to))
                continue;

            Vector2 center = (from + to) * 0.5f;
            Vector2 size = new Vector2(Mathf.Abs(from.x - to.x) + 2.4f, Mathf.Abs(from.y - to.y) + 2.4f);
            CreateRoadArea(parent, solidSprite, "ROAD_EDGE_" + edge.from + "_TO_" + edge.to, edge.from, edge.to, center, size);
        }
    }

    private static void CreateRoadArea(Transform parent, Sprite solidSprite, string objectName, string fromRouteNodeId, string toRouteNodeId, Vector2 position, Vector2 size)
    {
        GameObject road = CreateBox(parent, objectName, position, size, solidSprite, new Color(0.18f, 0.34f, 0.42f, 0.14f), true);

        SpriteRenderer renderer = road.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = 1;

        HumanCityRoadArea roadArea = road.AddComponent<HumanCityRoadArea>();
        roadArea.roadAreaId = objectName;
        roadArea.fromRouteNodeId = fromRouteNodeId;
        roadArea.toRouteNodeId = toRouteNodeId;
    }

    private static Dictionary<string, Transform> CreateBuildingWaypoints(Transform parent, Sprite solidSprite)
    {
        Dictionary<string, Transform> waypoints = new Dictionary<string, Transform>();
        TextAsset buildingJson = AssetDatabase.LoadAssetAtPath<TextAsset>(BuildingDataPath);
        if (buildingJson == null) return waypoints;

        CityBuildingDatabase database = JsonUtility.FromJson<CityBuildingDatabase>(buildingJson.text);
        if (database == null || database.buildings == null) return waypoints;

        foreach (CityBuildingRecord building in database.buildings)
        {
            GameObject waypoint = new GameObject("WP_" + building.id + "_" + SanitizeName(building.name));
            waypoint.transform.SetParent(parent);
            waypoint.transform.position = new Vector3(building.x, building.y + 0.25f, 0f);

            CityWaypoint cityWaypoint = waypoint.AddComponent<CityWaypoint>();
            cityWaypoint.waypointId = building.id;
            cityWaypoint.displayName = building.name;

            Color markerColor = GetBuildingColor(building.type);
            CreateBox(waypoint.transform, "Marker_" + building.id, Vector2.zero, new Vector2(0.5f, 0.5f), solidSprite, markerColor, true);
            waypoints.Add(building.id, waypoint.transform);
        }

        return waypoints;
    }

    private static void CreateRouteTriggers(Transform parent, Sprite solidSprite)
    {
        AddRouteTrigger(parent, solidSprite, "R01_CAVE_EXIT", "Cave exit", CityRouteRisk.Low, Vector2.one * 1.5f);
        AddRouteTrigger(parent, solidSprite, "R02_SEWER_CULVERT", "Sewer culvert", CityRouteRisk.Low, Vector2.one * 1.5f);
        AddRouteTrigger(parent, solidSprite, "R03_NORTH_GATE", "Rush north gate", CityRouteRisk.VeryHigh, new Vector2(3.5f, 2.5f));
        AddRouteTrigger(parent, solidSprite, "R04_MARKET_OBSERVE", "Observe market", CityRouteRisk.Low, new Vector2(3.5f, 2f));
        AddRouteTrigger(parent, solidSprite, "R06_BLACKSMITH_NOISE", "Use forge noise", CityRouteRisk.Medium, new Vector2(3f, 2f));
        AddRouteTrigger(parent, solidSprite, "R07_STABLE_PANIC", "Panic stable", CityRouteRisk.VeryHigh, new Vector2(3f, 2f));
        AddRouteTrigger(parent, solidSprite, "R08_BACK_ALLEY", "Back alley", CityRouteRisk.Low, new Vector2(4f, 2f));
        AddRouteTrigger(parent, solidSprite, "R09_ROOFTOP_LINE", "Rooftop line", CityRouteRisk.Medium, new Vector2(4f, 2f));
        AddRouteTrigger(parent, solidSprite, "R10_CHAPEL_BELL", "Chapel bell", CityRouteRisk.High, new Vector2(2.5f, 2.5f));
        AddRouteTrigger(parent, solidSprite, "R11_CHURCH_YARD", "Church yard", CityRouteRisk.Medium, new Vector2(4f, 2f));
        AddRouteTrigger(parent, solidSprite, "R12_BARRACKS_ROTATION", "Barracks timing", CityRouteRisk.Low, new Vector2(4f, 2f));
        AddRouteTrigger(parent, solidSprite, "R13_GATEHOUSE", "Gatehouse during bakery smoke", CityRouteRisk.Medium, new Vector2(4f, 2.5f), "bakery_smoke");
        AddRouteTrigger(parent, solidSprite, "R14_STOREHOUSE_FRONT", "Storehouse front", CityRouteRisk.High, new Vector2(4f, 2.5f));
        AddRouteTrigger(parent, solidSprite, "R15_STOREHOUSE_SEWER", "Storehouse sewer grate", CityRouteRisk.Medium, new Vector2(4f, 2f));
        AddRouteTrigger(parent, solidSprite, "R16_MAYOR_KEY", "Mayor key opportunity", CityRouteRisk.Medium, new Vector2(3f, 2f));
        AddRouteTrigger(parent, solidSprite, "R17_RETURN_TUNNEL", "Return tunnel", CityRouteRisk.Low, new Vector2(3f, 2f), string.Empty, true);

        GameObject bakery = CreateBox(parent, "TRIGGER_R05_BAKERY_DISTRACTION", RoutePositions["R05_BAKERY_DISTRACTION"], new Vector2(3f, 2f), solidSprite, new Color(1f, 0.78f, 0.25f, 0.35f), true);
        CityDistractionTrigger distraction = bakery.AddComponent<CityDistractionTrigger>();
        distraction.distractionId = "bakery_smoke";
        distraction.durationSeconds = 18f;
        distraction.alarmChange = 0;
    }

    private static void AddRouteTrigger(Transform parent, Sprite solidSprite, string routeId, string choiceLabel, CityRouteRisk risk, Vector2 size, string requiredDistractionId = "", bool requiresLoot = false)
    {
        if (!RoutePositions.TryGetValue(routeId, out Vector2 position)) return;

        GameObject trigger = CreateBox(parent, "TRIGGER_" + routeId, position, size, solidSprite, GetRouteColor(risk), true);
        CityRouteChoiceTrigger routeChoice = trigger.AddComponent<CityRouteChoiceTrigger>();
        routeChoice.routeNodeId = routeId;
        routeChoice.choiceLabel = choiceLabel;
        routeChoice.risk = risk;
        routeChoice.requiredDistractionId = requiredDistractionId;
        routeChoice.requiresLoot = requiresLoot;
    }

    private static void CreateObjectiveObjects(Transform parent, Sprite solidSprite)
    {
        GameObject loot = CreateBox(parent, "OBJ_Storehouse_Loot", new Vector2(90f, 0.75f), new Vector2(1.1f, 1.1f), solidSprite, new Color(1f, 0.86f, 0.18f, 0.9f), true);
        CityLootPickup lootPickup = loot.AddComponent<CityLootPickup>();
        lootPickup.lootAmount = 1;
        lootPickup.alarmOnPickup = 1;

        GameObject escape = CreateBox(parent, "OBJ_Return_Tunnel_Escape", new Vector2(96f, -2.35f), new Vector2(2.2f, 2.2f), solidSprite, new Color(0.25f, 0.9f, 0.55f, 0.6f), true);
        escape.AddComponent<CityEscapeTrigger>();
    }

    private static void CreateSystems(Transform systemsRoot, Transform citizenRoot, CitizenScheduleAgent civilianPrefab, CitizenScheduleAgent guardPrefab)
    {
        GameObject clockObject = new GameObject("CityClock");
        clockObject.transform.SetParent(systemsRoot);
        CityClock clock = clockObject.AddComponent<CityClock>();
        clock.dayLengthSeconds = 600f;
        clock.startHour = 6f;

        GameObject alarmObject = new GameObject("CityAlarmDirector");
        alarmObject.transform.SetParent(systemsRoot);
        CityAlarmDirector alarm = alarmObject.AddComponent<CityAlarmDirector>();
        alarm.fullAlarmLevel = 3;
        alarm.alarmDecayDelay = 20f;

        GameObject objectiveObject = new GameObject("CityRaidObjective");
        objectiveObject.transform.SetParent(systemsRoot);
        CityRaidObjective objective = objectiveObject.AddComponent<CityRaidObjective>();
        objective.requiredLootCount = 1;

        GameObject scheduleObject = new GameObject("CityScheduleManager");
        scheduleObject.transform.SetParent(systemsRoot);
        CityScheduleManager schedule = scheduleObject.AddComponent<CityScheduleManager>();
        schedule.citizensJson = AssetDatabase.LoadAssetAtPath<TextAsset>(CitizenDataPath);
        schedule.civilianPrefab = civilianPrefab;
        schedule.guardPrefab = guardPrefab;
        schedule.citizensParent = citizenRoot;

        GameObject hudObject = new GameObject("HumanCityObjectiveHud");
        hudObject.transform.SetParent(systemsRoot);
        hudObject.AddComponent<HumanCityObjectiveHud>();
    }

    private static void CreateManifest(GameObject root)
    {
        HumanCityMapManifest manifest = root.AddComponent<HumanCityMapManifest>();
        manifest.buildingsJson = AssetDatabase.LoadAssetAtPath<TextAsset>(BuildingDataPath);
        manifest.citizensJson = AssetDatabase.LoadAssetAtPath<TextAsset>(CitizenDataPath);
        manifest.routeGraphJson = AssetDatabase.LoadAssetAtPath<TextAsset>(RouteGraphPath);
        manifest.sourceBackdropPath = BackdropPath;
        manifest.visualReferencePath = VisualReferencePath;
    }

    private static GameObject CreatePlayer(GameObject playerPrefab)
    {
        GameObject player = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
        if (player == null) return null;

        player.name = "Player_RaidTester";
        player.transform.position = new Vector3(4f, -2.1f, 0f);
        return player;
    }

    private static void AttachCameraFollow(Camera camera, Transform player)
    {
        HumanCityCameraFollow follow = camera.gameObject.AddComponent<HumanCityCameraFollow>();
        follow.target = player;
        follow.targetOffset = new Vector2(0f, 0.8f);
        follow.minPosition = new Vector2(4f, -3.2f);
        follow.maxPosition = new Vector2(96f, 5.4f);
        follow.clampToViewBounds = true;
        follow.viewMinPosition = new Vector2(-6f, -7.6f);
        follow.viewMaxPosition = new Vector2(106f, 6.4f);
        follow.followSpeed = 18f;
        follow.SnapToTarget();
    }

    private static CitizenScheduleAgent CreateCitizenPrefab(string prefabName, string spritePath, Color tint, int alarmAmount)
    {
        string prefabPath = "Assets/_GoldenKeep/Prefabs/HumanCity/" + prefabName + ".prefab";
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

        GameObject prefabRoot = new GameObject(prefabName);
        prefabRoot.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
        SpriteRenderer renderer = prefabRoot.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = tint;
        renderer.sortingOrder = 20;

        Rigidbody2D body = prefabRoot.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;

        CapsuleCollider2D collider = prefabRoot.AddComponent<CapsuleCollider2D>();
        collider.size = new Vector2(1.2f, 3.0f);

        CitizenScheduleAgent agent = prefabRoot.AddComponent<CitizenScheduleAgent>();
        agent.moveSpeed = prefabName.Contains("Guard") ? 2.6f : 1.8f;

        GameObject sensor = new GameObject("AlarmSensor");
        sensor.transform.SetParent(prefabRoot.transform);
        CircleCollider2D sensorCollider = sensor.AddComponent<CircleCollider2D>();
        sensorCollider.isTrigger = true;
        sensorCollider.radius = prefabName.Contains("Guard") ? 5.0f : 4.5f;

        CitizenAlarmSensor alarmSensor = sensor.AddComponent<CitizenAlarmSensor>();
        alarmSensor.alarmAmount = alarmAmount;

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        Object.DestroyImmediate(prefabRoot);
        return savedPrefab.GetComponent<CitizenScheduleAgent>();
    }

    private static GameObject CreatePlayerPrefab()
    {
        string prefabPath = "Assets/_GoldenKeep/Prefabs/HumanCity/PF_Player_RaidTester.prefab";
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(PlayerSpritePath);

        GameObject prefabRoot = new GameObject("PF_Player_RaidTester");
        prefabRoot.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        AssignTag(prefabRoot, "Player");

        SpriteRenderer renderer = prefabRoot.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = 30;
        renderer.color = new Color(1f, 0.45f, 0.35f);

        Rigidbody2D body = prefabRoot.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;

        CapsuleCollider2D collider = prefabRoot.AddComponent<CapsuleCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1.4f, 2.5f);

        prefabRoot.AddComponent<HumanCityPlayerController>();

        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
        Object.DestroyImmediate(prefabRoot);
        return savedPrefab;
    }

    private static GameObject CreateBox(Transform parent, string name, Vector2 position, Vector2 size, Sprite sprite, Color color, bool trigger)
    {
        GameObject box = new GameObject(name);
        box.transform.SetParent(parent);
        box.transform.localPosition = new Vector3(position.x, position.y, 0f);
        box.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = box.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = trigger ? 5 : 0;
        renderer.enabled = ShowDebugGeometry;

        BoxCollider2D collider = box.AddComponent<BoxCollider2D>();
        collider.isTrigger = trigger;
        return box;
    }

    private static void CreateLadder(Transform parent, string name, Vector2 position, float height, Sprite sprite)
    {
        GameObject ladder = CreateBox(parent, name, position, new Vector2(0.65f, height), sprite, new Color(0.74f, 0.52f, 0.27f, 0.75f), true);
        ladder.AddComponent<HumanCityLadderZone>();
    }

    private static Sprite GetSolidSprite()
    {
        if (!File.Exists(SolidSpritePath))
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            File.WriteAllBytes(SolidSpritePath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(SolidSpritePath);
        }

        ConfigureSpriteImport(SolidSpritePath, 1f);
        return AssetDatabase.LoadAssetAtPath<Sprite>(SolidSpritePath);
    }

    private static Sprite[] LoadSpriteAssets(string[] assetPaths, float pixelsPerUnit)
    {
        Sprite[] sprites = new Sprite[assetPaths.Length];
        for (int i = 0; i < assetPaths.Length; i++)
            sprites[i] = LoadSpriteAsset(assetPaths[i], pixelsPerUnit);

        return sprites;
    }

    private static Sprite LoadSpriteAsset(string assetPath, float pixelsPerUnit)
    {
        ConfigureSpriteImport(assetPath, pixelsPerUnit);
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (sprite != null)
            return sprite;

        Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetPath);
        foreach (Object asset in assets)
        {
            Sprite foundSprite = asset as Sprite;
            if (foundSprite != null)
                return foundSprite;
        }

        Debug.LogWarning("Human City prefab builder could not load sprite at " + assetPath);
        return null;
    }

    private static void ConfigureSpriteImport(string assetPath, float pixelsPerUnit)
    {
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null) return;

        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.spritePivot = new Vector2(0.5f, 0.5f);
        importer.mipmapEnabled = false;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

    private static void ConfigureProvidedArtImports()
    {
        ConfigureSpriteImport(SurfaceCityArtPath, 100f);
        ConfigureSpriteImport(UndergroundCityArtPath, 100f);

        foreach (string artPath in LocationArtPaths)
            ConfigureSpriteImport(artPath, 100f);

        foreach (string artPath in PropArtPaths)
            ConfigureSpriteImport(artPath, 100f);
    }

    private static Color GetBuildingColor(string buildingType)
    {
        switch (buildingType)
        {
            case "military":
            case "gate":
                return new Color(0.35f, 0.55f, 1f, 0.85f);
            case "objective":
                return new Color(1f, 0.82f, 0.18f, 0.9f);
            case "alarm":
                return new Color(1f, 0.25f, 0.18f, 0.9f);
            case "secret_entry":
            case "exit":
            case "route":
                return new Color(0.2f, 0.9f, 0.6f, 0.8f);
            default:
                return new Color(0.95f, 0.95f, 0.95f, 0.75f);
        }
    }

    private static Color GetRouteColor(CityRouteRisk risk)
    {
        switch (risk)
        {
            case CityRouteRisk.Low:
                return new Color(0.18f, 0.9f, 0.55f, 0.28f);
            case CityRouteRisk.Medium:
                return new Color(1f, 0.78f, 0.18f, 0.3f);
            case CityRouteRisk.High:
                return new Color(1f, 0.38f, 0.16f, 0.34f);
            case CityRouteRisk.VeryHigh:
                return new Color(1f, 0.08f, 0.08f, 0.38f);
            default:
                return new Color(0.5f, 0.5f, 0.5f, 0.25f);
        }
    }

    private static string SanitizeName(string value)
    {
        if (string.IsNullOrEmpty(value)) return "Unnamed";
        foreach (char invalid in Path.GetInvalidFileNameChars())
            value = value.Replace(invalid, '_');
        return value.Replace(' ', '_');
    }

    private static void EnsurePlayerTag()
    {
        foreach (string existingTag in InternalEditorUtility.tags)
        {
            if (existingTag == "Player")
                return;
        }

        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == "Player")
                return;
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = "Player";
        tagManager.ApplyModifiedProperties();
    }

    private static void AssignTag(GameObject gameObject, string tagName)
    {
        try
        {
            gameObject.tag = tagName;
        }
        catch
        {
            Debug.LogWarning("Could not assign tag '" + tagName + "' to " + gameObject.name + ". Add the tag in Project Settings if trigger checks fail.");
        }
    }
}
