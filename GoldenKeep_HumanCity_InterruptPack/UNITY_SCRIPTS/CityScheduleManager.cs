using System.Collections.Generic;
using UnityEngine;

public class CityScheduleManager : MonoBehaviour
{
    public TextAsset citizensJson;
    public CitizenScheduleAgent civilianPrefab;
    public CitizenScheduleAgent guardPrefab;
    public Transform citizensParent;

    private Dictionary<string, Transform> waypoints = new Dictionary<string, Transform>();

    private void Start()
    {
        RegisterWaypoints();
        SpawnCitizens();
    }

    private void RegisterWaypoints()
    {
        foreach (CityWaypoint wp in FindObjectsOfType<CityWaypoint>())
        {
            if (!string.IsNullOrWhiteSpace(wp.waypointId) && !waypoints.ContainsKey(wp.waypointId))
                waypoints.Add(wp.waypointId, wp.transform);
        }
    }

    private void SpawnCitizens()
    {
        if (citizensJson == null || civilianPrefab == null) return;
        CitizenDatabase db = JsonUtility.FromJson<CitizenDatabase>(citizensJson.text);
        if (db == null || db.citizens == null) return;

        foreach (CitizenRecord record in db.citizens)
        {
            CitizenScheduleAgent prefab = IsGuard(record) && guardPrefab != null ? guardPrefab : civilianPrefab;
            Transform start = GetStartWaypoint(record);
            CitizenScheduleAgent agent = Instantiate(prefab, start.position, Quaternion.identity, citizensParent);
            agent.name = record.id + "_" + record.name.Replace(" ", "_");
            agent.Configure(record, waypoints);
        }
    }

    private bool IsGuard(CitizenRecord record)
    {
        if (record.combat == null) return false;
        return record.combat.threatLevel == "medium" || record.combat.threatLevel == "high" || record.combat.threatLevel == "very_high";
    }

    private Transform GetStartWaypoint(CitizenRecord record)
    {
        if (record.schedule != null && record.schedule.Count > 0 && waypoints.TryGetValue(record.schedule[0].locationId, out var t))
            return t;
        return transform;
    }
}
