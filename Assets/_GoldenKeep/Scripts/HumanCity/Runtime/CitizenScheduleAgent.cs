using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class CitizenScheduleAgent : MonoBehaviour
{
    public string citizenId;
    public string citizenName;
    public string role;
    public float moveSpeed = 2f;
    public bool fleeOnAlarm = true;
    public bool attackOnAlarm = false;
    public float fullAlarmSpeedMultiplier = 1.5f;
    public float fleeDistance = 6f;
    public string playerTag = "Player";

    private List<ScheduleBlock> schedule = new List<ScheduleBlock>();
    private Dictionary<string, Transform> waypointLookup;
    private Transform target;
    private Transform player;
    private Vector3 alarmTarget;
    private bool fullAlarmActive;
    private bool subscribedToAlarm;
    private float nextAlarmRetargetTime;

    public void Configure(CitizenRecord record, Dictionary<string, Transform> waypoints)
    {
        citizenId = record.id;
        citizenName = record.name;
        role = record.role;
        schedule = record.schedule;
        waypointLookup = waypoints;
        attackOnAlarm = record.combat != null && (record.combat.threatLevel == "high" || record.combat.threatLevel == "very_high" || record.combat.threatLevel == "medium");
        fleeOnAlarm = !attackOnAlarm;
        SubscribeToAlarm();
        PickTarget();
    }

    private void OnEnable()
    {
        SubscribeToAlarm();
    }

    private void OnDisable()
    {
        if (subscribedToAlarm && CityAlarmDirector.Instance != null)
        {
            CityAlarmDirector.Instance.AlarmChanged -= HandleAlarmChanged;
            subscribedToAlarm = false;
        }
    }

    private void Update()
    {
        SubscribeToAlarm();

        if (fullAlarmActive)
        {
            ReactDuringFullAlarm();
            return;
        }

        PickTarget();
        MoveToTarget(moveSpeed);
    }

    private void PickTarget()
    {
        if (schedule == null || schedule.Count == 0 || waypointLookup == null) return;
        float hour = CityClock.Instance != null ? CityClock.Instance.CurrentHour : 12f;
        foreach (var block in schedule)
        {
            if (IsHourInBlock(hour, block.start, block.end) && waypointLookup.TryGetValue(block.locationId, out var t))
            {
                target = t;
                return;
            }
        }
    }

    private void MoveToTarget()
    {
        MoveToTarget(moveSpeed);
    }

    private void MoveToTarget(float speed)
    {
        if (target == null) return;
        Vector3 next = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        transform.position = next;
    }

    private void ReactDuringFullAlarm()
    {
        float alarmSpeed = moveSpeed * fullAlarmSpeedMultiplier;

        if (attackOnAlarm)
        {
            Transform playerTarget = GetPlayer();
            if (playerTarget != null)
            {
                target = playerTarget;
                MoveToTarget(alarmSpeed);
            }
            return;
        }

        if (fleeOnAlarm)
        {
            if (Time.time >= nextAlarmRetargetTime)
                PickFleeTarget();

            transform.position = Vector3.MoveTowards(transform.position, alarmTarget, alarmSpeed * Time.deltaTime);
        }
    }

    private void PickFleeTarget()
    {
        nextAlarmRetargetTime = Time.time + 1.5f;
        Transform playerTarget = GetPlayer();

        if (playerTarget == null)
        {
            alarmTarget = transform.position + new Vector3(Random.Range(-fleeDistance, fleeDistance), 0f, 0f);
            return;
        }

        Vector3 away = transform.position - playerTarget.position;
        if (away.sqrMagnitude < 0.01f)
            away = Vector3.left;

        alarmTarget = transform.position + away.normalized * fleeDistance;
    }

    private Transform GetPlayer()
    {
        if (player != null) return player;
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        player = playerObject != null ? playerObject.transform : null;
        return player;
    }

    private void SubscribeToAlarm()
    {
        if (subscribedToAlarm || CityAlarmDirector.Instance == null) return;
        CityAlarmDirector.Instance.AlarmChanged += HandleAlarmChanged;
        subscribedToAlarm = true;
        fullAlarmActive = CityAlarmDirector.Instance.IsFullAlarm();
    }

    private void HandleAlarmChanged(int level, bool isFullAlarm)
    {
        fullAlarmActive = isFullAlarm;
        if (isFullAlarm)
            PickFleeTarget();
    }

    private bool IsHourInBlock(float hour, string start, string end)
    {
        float s = ParseHour(start);
        float e = ParseHour(end);
        if (e < s)
            return hour >= s || hour < e;
        return hour >= s && hour < e;
    }

    private float ParseHour(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0f;
        string[] parts = text.Split(':');
        float h = float.Parse(parts[0], CultureInfo.InvariantCulture);
        float m = parts.Length > 1 ? float.Parse(parts[1], CultureInfo.InvariantCulture) : 0f;
        return h + m / 60f;
    }
}
