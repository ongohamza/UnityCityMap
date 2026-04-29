using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HumanCityRuntimeBootstrap : MonoBehaviour
{
    [Header("Primitive data")]
    public TextAsset buildingsJson;
    public TextAsset citizensJson;
    public TextAsset routeGraphJson;

    [Header("Provided art")]
    public Sprite backdropSprite;
    public Sprite civilianSprite;
    public Sprite guardSprite;
    public Sprite playerSprite;

    [Header("Provided city art")]
    public Sprite surfaceCityArtSprite;
    public Sprite undergroundCityArtSprite;
    public Sprite[] locationArtSprites;
    public Sprite[] propArtSprites;

    [Header("Build options")]
    public bool buildOnAwake = true;
    public bool clearExistingGeneratedMap = true;
    public bool showReferenceBackdrop;
    public bool showDetailOverlays;
    public bool showDebugGeometry;

    private const string GeneratedRootName = "HUMAN_CITY_01_GENERATED";
    private Sprite solidSprite;

#if UNITY_EDITOR
    private const string BackdropSpritePath = "Assets/Human Art/Level Blueprint.png";
    private const string CivilianSpritePath = "Assets/Human Art/1.png";
    private const string GuardSpritePath = "Assets/Human Art/10.png";
    private const string PlayerSpritePath = "Assets/Human Art/gargoyle.png";
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
#endif

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

    private void Awake()
    {
        if (buildOnAwake)
            BuildCity();
    }

    public void BuildCity()
    {
#if UNITY_EDITOR
        ResolveEditorSpriteReferences();
#endif

        if (buildingsJson == null || citizensJson == null)
        {
            Debug.LogError("HumanCityRuntimeBootstrap needs buildingsJson and citizensJson.");
            return;
        }

        if (clearExistingGeneratedMap)
            ClearGeneratedMap();

        solidSprite = CreateSolidSprite();

        GameObject root = CreateRoot(GeneratedRootName, null);
        GameObject visualRoot = CreateRoot("CITY_VISUALS", root.transform);
        GameObject buildingRoot = CreateRoot("CITY_BUILDINGS", root.transform);
        GameObject routeRoot = CreateRoot("CITY_TRIGGERS", root.transform);
        GameObject citizenRoot = CreateRoot("CITY_CITIZENS", root.transform);
        GameObject systemsRoot = CreateRoot("CITY_SYSTEMS", root.transform);
        GameObject platformRoot = CreateRoot("CITY_PLATFORMS", root.transform);
        GameObject roadRoot = CreateRoot("CITY_ROADS", root.transform);
        GameObject templateRoot = CreateRoot("CITY_RUNTIME_TEMPLATES", root.transform);

        Camera camera = ConfigureCamera();
        CreateBackdrop(visualRoot.transform);
        CreateProvidedArtwork(visualRoot.transform);
        CreatePlatforms(platformRoot.transform);
        CreateRoadNetwork(roadRoot.transform);
        CreateBuildingWaypoints(buildingRoot.transform);
        CreateRouteTriggers(routeRoot.transform);
        CreateObjectiveObjects(routeRoot.transform);

        CitizenScheduleAgent civilianTemplate = CreateCitizenTemplate(templateRoot.transform, "Template_Citizen_Civilian", civilianSprite, new Color(0.86f, 0.78f, 0.58f), 1, false);
        CitizenScheduleAgent guardTemplate = CreateCitizenTemplate(templateRoot.transform, "Template_Citizen_Guard", guardSprite, new Color(0.55f, 0.72f, 1f), 2, true);

        GameObject player = CreatePlayer(root.transform);
        AttachCameraFollow(camera, player.transform);
        CreateSystems(systemsRoot.transform, citizenRoot.transform, civilianTemplate, guardTemplate);
        CreateManifest(root);
    }

    private void ClearGeneratedMap()
    {
        GameObject existing = GameObject.Find(GeneratedRootName);
        if (existing != null)
            Destroy(existing);
    }

    private GameObject CreateRoot(string rootName, Transform parent)
    {
        GameObject root = new GameObject(rootName);
        if (parent != null)
            root.transform.SetParent(parent);
        return root;
    }

    private Camera ConfigureCamera()
    {
        Camera camera = FindFirstObjectByType<Camera>();
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        camera.orthographic = true;
        camera.orthographicSize = 6.2f;
        camera.backgroundColor = new Color(0.09f, 0.08f, 0.08f);
        camera.transform.position = new Vector3(48f, 0.5f, -20f);

        if (camera.GetComponent<AudioListener>() == null)
            camera.gameObject.AddComponent<AudioListener>();

        return camera;
    }

    private void CreateBackdrop(Transform parent)
    {
        if (!showReferenceBackdrop)
            return;

        if (backdropSprite == null) return;

        GameObject backdrop = new GameObject("VISUAL_REFERENCE_Level_Blueprint");
        backdrop.transform.SetParent(parent);
        backdrop.transform.position = new Vector3(48f, -4.2f, 8f);

        SpriteRenderer renderer = backdrop.AddComponent<SpriteRenderer>();
        renderer.sprite = backdropSprite;
        renderer.sortingOrder = -100;
        renderer.color = new Color(1f, 1f, 1f, 0.65f);
    }

    private void CreateProvidedArtwork(Transform parent)
    {
        CreateArtworkBand(parent, "ART_SurfaceCityCleanBand", surfaceCityArtSprite, new Vector2(50f, 2.45f), new Vector2(112f, 8.2f), -92, Color.white);
        CreateArtworkBand(parent, "ART_UndergroundCityCleanBand", undergroundCityArtSprite, new Vector2(50f, -4.25f), new Vector2(112f, 6.9f), -91, Color.white);

        if (!showDetailOverlays)
            return;

        CreateLocationArtwork(parent);
        CreatePropArtwork(parent);
    }

    private void CreateLocationArtwork(Transform parent)
    {
        if (locationArtSprites == null) return;

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
        int count = Mathf.Min(locationArtSprites.Length, positions.Length);

        for (int i = 0; i < count; i++)
            CreateArtworkSprite(parent, "ART_Location_" + i, locationArtSprites[i], positions[i], widths[i], -12, new Color(1f, 1f, 1f, 0.82f));
    }

    private void CreatePropArtwork(Transform parent)
    {
        if (propArtSprites == null) return;

        Vector2[] positions =
        {
            new Vector2(22f, 0.9f),
            new Vector2(74f, 0.95f),
            new Vector2(37f, -2.1f),
            new Vector2(54f, -3.8f)
        };

        float[] widths = { 16f, 14f, 14f, 14f };
        int count = Mathf.Min(propArtSprites.Length, positions.Length);

        for (int i = 0; i < count; i++)
            CreateArtworkSprite(parent, "ART_Props_" + i, propArtSprites[i], positions[i], widths[i], 2, new Color(1f, 1f, 1f, 0.8f));
    }

    private void CreateArtworkSprite(Transform parent, string objectName, Sprite sprite, Vector2 position, float targetWidth, int sortingOrder, Color color)
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

    private void CreateArtworkBand(Transform parent, string objectName, Sprite sprite, Vector2 position, Vector2 targetSize, int sortingOrder, Color color)
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

    private void CreatePlatforms(Transform parent)
    {
        CreateBox(parent, "MainStreet_Collider", new Vector2(48f, -0.65f), new Vector2(100f, 0.75f), new Color(0.24f, 0.19f, 0.15f, 0.35f), false);
        CreateBox(parent, "BackAlley_Collider", new Vector2(36f, -2.55f), new Vector2(42f, 0.55f), new Color(0.18f, 0.16f, 0.14f, 0.3f), false);
        CreateBox(parent, "Sewer_Collider", new Vector2(49f, -4.55f), new Vector2(92f, 0.7f), new Color(0.12f, 0.12f, 0.11f, 0.35f), false);
        CreateBox(parent, "Rooftop_West_Collider", new Vector2(28f, 4.75f), new Vector2(38f, 0.55f), new Color(0.35f, 0.25f, 0.16f, 0.32f), false);
        CreateBox(parent, "Rooftop_East_Collider", new Vector2(70f, 4.75f), new Vector2(46f, 0.55f), new Color(0.35f, 0.25f, 0.16f, 0.32f), false);

        CreateLadder(parent, "Ladder_Culvert_To_Main", new Vector2(8f, -2.45f), 3.8f);
        CreateLadder(parent, "Ladder_Alley_To_Rooftop", new Vector2(36f, 1.2f), 7.0f);
        CreateLadder(parent, "Ladder_Chapel_To_Sewer", new Vector2(62f, -2f), 5.3f);
        CreateLadder(parent, "Ladder_Storehouse_Grate", new Vector2(88f, -2.1f), 4.2f);
    }

    private void CreateRoadNetwork(Transform parent)
    {
        foreach (KeyValuePair<string, Vector2> route in RoutePositions)
            CreateRoadArea(parent, "ROAD_NODE_" + route.Key, route.Key, string.Empty, route.Value, new Vector2(2.4f, 2.4f));

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
            CreateRoadArea(parent, "ROAD_EDGE_" + edge.from + "_TO_" + edge.to, edge.from, edge.to, center, size);
        }
    }

    private void CreateRoadArea(Transform parent, string objectName, string fromRouteNodeId, string toRouteNodeId, Vector2 position, Vector2 size)
    {
        GameObject road = CreateBox(parent, objectName, position, size, new Color(0.18f, 0.34f, 0.42f, 0.14f), true);

        SpriteRenderer renderer = road.GetComponent<SpriteRenderer>();
        renderer.sortingOrder = 1;

        HumanCityRoadArea roadArea = road.AddComponent<HumanCityRoadArea>();
        roadArea.roadAreaId = objectName;
        roadArea.fromRouteNodeId = fromRouteNodeId;
        roadArea.toRouteNodeId = toRouteNodeId;
    }

    private void CreateBuildingWaypoints(Transform parent)
    {
        CityBuildingDatabase database = JsonUtility.FromJson<CityBuildingDatabase>(buildingsJson.text);
        if (database == null || database.buildings == null) return;

        foreach (CityBuildingRecord building in database.buildings)
        {
            GameObject waypoint = new GameObject("WP_" + building.id + "_" + ToSafeObjectName(building.name));
            waypoint.transform.SetParent(parent);
            waypoint.transform.position = new Vector3(building.x, building.y + 0.25f, 0f);

            CityWaypoint cityWaypoint = waypoint.AddComponent<CityWaypoint>();
            cityWaypoint.waypointId = building.id;
            cityWaypoint.displayName = building.name;

            CreateBox(waypoint.transform, "Marker_" + building.id, Vector2.zero, new Vector2(0.5f, 0.5f), GetBuildingColor(building.type), true);
        }
    }

    private void CreateRouteTriggers(Transform parent)
    {
        AddRouteTrigger(parent, "R01_CAVE_EXIT", "Cave exit", CityRouteRisk.Low, Vector2.one * 1.5f);
        AddRouteTrigger(parent, "R02_SEWER_CULVERT", "Sewer culvert", CityRouteRisk.Low, Vector2.one * 1.5f);
        AddRouteTrigger(parent, "R03_NORTH_GATE", "Rush north gate", CityRouteRisk.VeryHigh, new Vector2(3.5f, 2.5f));
        AddRouteTrigger(parent, "R04_MARKET_OBSERVE", "Observe market", CityRouteRisk.Low, new Vector2(3.5f, 2f));
        AddRouteTrigger(parent, "R06_BLACKSMITH_NOISE", "Use forge noise", CityRouteRisk.Medium, new Vector2(3f, 2f));
        AddRouteTrigger(parent, "R07_STABLE_PANIC", "Panic stable", CityRouteRisk.VeryHigh, new Vector2(3f, 2f));
        AddRouteTrigger(parent, "R08_BACK_ALLEY", "Back alley", CityRouteRisk.Low, new Vector2(4f, 2f));
        AddRouteTrigger(parent, "R09_ROOFTOP_LINE", "Rooftop line", CityRouteRisk.Medium, new Vector2(4f, 2f));
        AddRouteTrigger(parent, "R10_CHAPEL_BELL", "Chapel bell", CityRouteRisk.High, new Vector2(2.5f, 2.5f));
        AddRouteTrigger(parent, "R11_CHURCH_YARD", "Church yard", CityRouteRisk.Medium, new Vector2(4f, 2f));
        AddRouteTrigger(parent, "R12_BARRACKS_ROTATION", "Barracks timing", CityRouteRisk.Low, new Vector2(4f, 2f));
        AddRouteTrigger(parent, "R13_GATEHOUSE", "Gatehouse during bakery smoke", CityRouteRisk.Medium, new Vector2(4f, 2.5f), "bakery_smoke");
        AddRouteTrigger(parent, "R14_STOREHOUSE_FRONT", "Storehouse front", CityRouteRisk.High, new Vector2(4f, 2.5f));
        AddRouteTrigger(parent, "R15_STOREHOUSE_SEWER", "Storehouse sewer grate", CityRouteRisk.Medium, new Vector2(4f, 2f));
        AddRouteTrigger(parent, "R16_MAYOR_KEY", "Mayor key opportunity", CityRouteRisk.Medium, new Vector2(3f, 2f));
        AddRouteTrigger(parent, "R17_RETURN_TUNNEL", "Return tunnel", CityRouteRisk.Low, new Vector2(3f, 2f), string.Empty, true);

        GameObject bakery = CreateRouteBox(parent, "TRIGGER_R05_BAKERY_DISTRACTION", "R05_BAKERY_DISTRACTION", new Vector2(3f, 2f), new Color(1f, 0.78f, 0.25f, 0.35f));
        CityDistractionTrigger distraction = bakery.AddComponent<CityDistractionTrigger>();
        distraction.distractionId = "bakery_smoke";
        distraction.durationSeconds = 18f;
        distraction.alarmChange = 0;
    }

    private void AddRouteTrigger(Transform parent, string routeId, string choiceLabel, CityRouteRisk risk, Vector2 size, string requiredDistractionId = "", bool requiresLoot = false)
    {
        GameObject trigger = CreateRouteBox(parent, "TRIGGER_" + routeId, routeId, size, GetRouteColor(risk));
        CityRouteChoiceTrigger routeChoice = trigger.AddComponent<CityRouteChoiceTrigger>();
        routeChoice.routeNodeId = routeId;
        routeChoice.choiceLabel = choiceLabel;
        routeChoice.risk = risk;
        routeChoice.requiredDistractionId = requiredDistractionId;
        routeChoice.requiresLoot = requiresLoot;
    }

    private GameObject CreateRouteBox(Transform parent, string objectName, string routeId, Vector2 size, Color color)
    {
        Vector2 position = RoutePositions.TryGetValue(routeId, out Vector2 foundPosition) ? foundPosition : Vector2.zero;
        return CreateBox(parent, objectName, position, size, color, true);
    }

    private void CreateObjectiveObjects(Transform parent)
    {
        GameObject loot = CreateBox(parent, "OBJ_Storehouse_Loot", new Vector2(90f, 0.75f), new Vector2(1.1f, 1.1f), new Color(1f, 0.86f, 0.18f, 0.9f), true);
        CityLootPickup lootPickup = loot.AddComponent<CityLootPickup>();
        lootPickup.lootAmount = 1;
        lootPickup.alarmOnPickup = 1;

        GameObject escape = CreateBox(parent, "OBJ_Return_Tunnel_Escape", new Vector2(96f, -2.35f), new Vector2(2.2f, 2.2f), new Color(0.25f, 0.9f, 0.55f, 0.6f), true);
        escape.AddComponent<CityEscapeTrigger>();
    }

    private CitizenScheduleAgent CreateCitizenTemplate(Transform parent, string objectName, Sprite sprite, Color tint, int alarmAmount, bool guard)
    {
        GameObject template = new GameObject(objectName);
        template.transform.SetParent(parent);
        template.transform.position = new Vector3(-1000f, -1000f, 0f);
        template.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

        SpriteRenderer renderer = template.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = tint;
        renderer.sortingOrder = 20;

        Rigidbody2D body = template.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;

        CapsuleCollider2D collider = template.AddComponent<CapsuleCollider2D>();
        collider.size = new Vector2(1.2f, 3.0f);

        CitizenScheduleAgent agent = template.AddComponent<CitizenScheduleAgent>();
        agent.moveSpeed = guard ? 2.6f : 1.8f;

        GameObject sensor = new GameObject("AlarmSensor");
        sensor.transform.SetParent(template.transform);
        CircleCollider2D sensorCollider = sensor.AddComponent<CircleCollider2D>();
        sensorCollider.isTrigger = true;
        sensorCollider.radius = guard ? 5.0f : 4.5f;

        CitizenAlarmSensor alarmSensor = sensor.AddComponent<CitizenAlarmSensor>();
        alarmSensor.alarmAmount = alarmAmount;
        return agent;
    }

    private GameObject CreatePlayer(Transform parent)
    {
        GameObject player = new GameObject("Player_RaidTester");
        player.transform.SetParent(parent);
        player.transform.position = new Vector3(4f, -2.1f, 0f);
        player.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        player.tag = "Player";

        SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
        renderer.sprite = playerSprite;
        renderer.sortingOrder = 30;
        renderer.color = new Color(1f, 0.45f, 0.35f);

        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;

        CapsuleCollider2D collider = player.AddComponent<CapsuleCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(1.4f, 2.5f);

        player.AddComponent<HumanCityPlayerController>();
        CreatePlayerTraversalMarker(player.transform);

        return player;
    }

    private void AttachCameraFollow(Camera camera, Transform player)
    {
        if (camera == null || player == null)
            return;

        HumanCityCameraFollow follow = camera.GetComponent<HumanCityCameraFollow>();
        if (follow == null)
            follow = camera.gameObject.AddComponent<HumanCityCameraFollow>();

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

    private void CreatePlayerTraversalMarker(Transform parent)
    {
        GameObject marker = new GameObject("PlayerTraversalMarker");
        marker.transform.SetParent(parent);
        marker.transform.localPosition = Vector3.zero;
        marker.transform.localScale = new Vector3(1.25f, 1.65f, 1f);

        SpriteRenderer markerRenderer = marker.AddComponent<SpriteRenderer>();
        markerRenderer.sprite = solidSprite;
        markerRenderer.color = new Color(0.2f, 1f, 0.75f, 0.9f);
        markerRenderer.sortingOrder = 31;
        markerRenderer.enabled = showDebugGeometry;
    }

    private void CreateSystems(Transform systemsRoot, Transform citizenRoot, CitizenScheduleAgent civilianTemplate, CitizenScheduleAgent guardTemplate)
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
        schedule.citizensJson = citizensJson;
        schedule.civilianPrefab = civilianTemplate;
        schedule.guardPrefab = guardTemplate;
        schedule.citizensParent = citizenRoot;

        GameObject hudObject = new GameObject("HumanCityObjectiveHud");
        hudObject.transform.SetParent(systemsRoot);
        hudObject.AddComponent<HumanCityObjectiveHud>();
    }

    private void CreateManifest(GameObject root)
    {
        HumanCityMapManifest manifest = root.AddComponent<HumanCityMapManifest>();
        manifest.buildingsJson = buildingsJson;
        manifest.citizensJson = citizensJson;
        manifest.routeGraphJson = routeGraphJson;
        manifest.sourceBackdropPath = "Assets/Human Art/Level Blueprint.png";
        manifest.visualReferencePath = "Assets/Human Art/Level 1 Blueprint best version so far.png";
    }

    private GameObject CreateBox(Transform parent, string objectName, Vector2 position, Vector2 size, Color color, bool trigger)
    {
        GameObject box = new GameObject(objectName);
        box.transform.SetParent(parent);
        box.transform.localPosition = new Vector3(position.x, position.y, 0f);
        box.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = box.AddComponent<SpriteRenderer>();
        renderer.sprite = solidSprite;
        renderer.color = color;
        renderer.sortingOrder = trigger ? 5 : 0;
        renderer.enabled = showDebugGeometry;

        BoxCollider2D collider = box.AddComponent<BoxCollider2D>();
        collider.isTrigger = trigger;
        return box;
    }

    private void CreateLadder(Transform parent, string objectName, Vector2 position, float height)
    {
        GameObject ladder = CreateBox(parent, objectName, position, new Vector2(0.65f, height), new Color(0.74f, 0.52f, 0.27f, 0.75f), true);
        ladder.AddComponent<HumanCityLadderZone>();
    }

    private Sprite CreateSolidSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }

    private Color GetBuildingColor(string buildingType)
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

    private Color GetRouteColor(CityRouteRisk risk)
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

    private string ToSafeObjectName(string value)
    {
        return string.IsNullOrEmpty(value) ? "Unnamed" : value.Replace(' ', '_').Replace('/', '_');
    }

#if UNITY_EDITOR
    private void ResolveEditorSpriteReferences()
    {
        backdropSprite = ResolveSpriteReference(backdropSprite, BackdropSpritePath, 16f);
        civilianSprite = ResolveSpriteReference(civilianSprite, CivilianSpritePath, 512f);
        guardSprite = ResolveSpriteReference(guardSprite, GuardSpritePath, 512f);
        playerSprite = ResolveSpriteReference(playerSprite, PlayerSpritePath, 512f);
        surfaceCityArtSprite = ResolveSpriteReference(surfaceCityArtSprite, SurfaceCityArtPath, 100f);
        undergroundCityArtSprite = ResolveSpriteReference(undergroundCityArtSprite, UndergroundCityArtPath, 100f);
        locationArtSprites = ResolveSpriteReferences(locationArtSprites, LocationArtPaths, 100f);
        propArtSprites = ResolveSpriteReferences(propArtSprites, PropArtPaths, 100f);
    }

    private static Sprite[] ResolveSpriteReferences(Sprite[] currentSprites, string[] fallbackPaths, float pixelsPerUnit)
    {
        int currentLength = currentSprites == null ? 0 : currentSprites.Length;
        int fallbackLength = fallbackPaths == null ? 0 : fallbackPaths.Length;
        int resolvedLength = currentLength > 0 ? currentLength : fallbackLength;
        Sprite[] sprites = new Sprite[resolvedLength];

        for (int i = 0; i < resolvedLength; i++)
        {
            Sprite currentSprite = i < currentLength ? currentSprites[i] : null;
            string fallbackPath = i < fallbackLength ? fallbackPaths[i] : string.Empty;
            sprites[i] = ResolveSpriteReference(currentSprite, fallbackPath, pixelsPerUnit);
        }

        return sprites;
    }

    private static Sprite ResolveSpriteReference(Sprite currentSprite, string fallbackPath, float pixelsPerUnit)
    {
        if (currentSprite != null)
        {
            ConfigureSpriteImport(AssetDatabase.GetAssetPath(currentSprite), pixelsPerUnit);
            return currentSprite;
        }

        return LoadSpriteAtPath(fallbackPath, pixelsPerUnit);
    }

    private static Sprite LoadSpriteAtPath(string assetPath, float pixelsPerUnit)
    {
        if (string.IsNullOrEmpty(assetPath))
            return null;

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

        Debug.LogWarning("HumanCityRuntimeBootstrap could not load sprite at " + assetPath);
        return null;
    }

    private static void ConfigureSpriteImport(string assetPath, float pixelsPerUnit)
    {
        if (string.IsNullOrEmpty(assetPath))
            return;

        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer == null)
            return;

        bool changed = false;
        changed |= SetImporterValue(importer.textureType, TextureImporterType.Sprite, value => importer.textureType = value);
        changed |= SetImporterValue(importer.spriteImportMode, SpriteImportMode.Single, value => importer.spriteImportMode = value);
        changed |= SetImporterValue(importer.spritePixelsPerUnit, pixelsPerUnit, value => importer.spritePixelsPerUnit = value);
        changed |= SetImporterValue(importer.spritePivot, new Vector2(0.5f, 0.5f), value => importer.spritePivot = value);
        changed |= SetImporterValue(importer.mipmapEnabled, false, value => importer.mipmapEnabled = value);
        changed |= SetImporterValue(importer.filterMode, FilterMode.Point, value => importer.filterMode = value);
        changed |= SetImporterValue(importer.textureCompression, TextureImporterCompression.Uncompressed, value => importer.textureCompression = value);
        changed |= SetImporterValue(importer.alphaIsTransparency, true, value => importer.alphaIsTransparency = value);

        if (changed)
            importer.SaveAndReimport();
    }

    private static bool SetImporterValue<T>(T currentValue, T targetValue, System.Action<T> setter)
    {
        if (EqualityComparer<T>.Default.Equals(currentValue, targetValue))
            return false;

        setter(targetValue);
        return true;
    }
#endif
}
