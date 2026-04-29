using System;
using UnityEngine;

public class CityAlarmDirector : MonoBehaviour
{
    public static CityAlarmDirector Instance { get; private set; }

    [Range(0, 3)]
    public int alarmLevel;
    public int fullAlarmLevel = 3;
    public float alarmDecayDelay = 20f;
    public string activeDistractionId;

    public event Action<int, bool> AlarmChanged;
    public event Action<string> RouteChoiceRecorded;

    private float lastAlarmTime;
    private float activeDistractionUntil;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (alarmLevel > 0 && Time.time - lastAlarmTime >= alarmDecayDelay)
        {
            alarmLevel--;
            lastAlarmTime = Time.time;
            NotifyAlarmChanged();
        }

        if (!string.IsNullOrEmpty(activeDistractionId) && Time.time >= activeDistractionUntil)
            activeDistractionId = string.Empty;
    }

    public void RaiseAlarm(int amount)
    {
        RaiseAlarm(amount, string.Empty);
    }

    public void RaiseAlarm(int amount, string source)
    {
        alarmLevel = Mathf.Clamp(alarmLevel + amount, 0, 3);
        lastAlarmTime = Time.time;
        NotifyAlarmChanged();
    }

    public void ReduceAlarm(int amount)
    {
        alarmLevel = Mathf.Clamp(alarmLevel - amount, 0, 3);
        NotifyAlarmChanged();
    }

    public void ResetAlarm()
    {
        alarmLevel = 0;
        activeDistractionId = string.Empty;
        NotifyAlarmChanged();
    }

    public bool IsFullAlarm()
    {
        return alarmLevel >= fullAlarmLevel;
    }

    public bool IsDistractionActive()
    {
        return !string.IsNullOrEmpty(activeDistractionId) && Time.time < activeDistractionUntil;
    }

    public bool IsDistractionActive(string distractionId)
    {
        if (string.IsNullOrEmpty(distractionId))
            return IsDistractionActive();
        return IsDistractionActive() && activeDistractionId == distractionId;
    }

    public void TriggerDistraction(string distractionId, float durationSeconds, int alarmChange)
    {
        activeDistractionId = distractionId;
        activeDistractionUntil = Time.time + durationSeconds;

        if (alarmChange > 0)
            RaiseAlarm(alarmChange, distractionId);
        else if (alarmChange < 0)
            ReduceAlarm(-alarmChange);
    }

    public void RecordRouteChoice(string routeNodeId, string choiceLabel, int alarmCost)
    {
        RouteChoiceRecorded?.Invoke(routeNodeId);
        if (alarmCost > 0)
            RaiseAlarm(alarmCost, choiceLabel);
    }

    private void NotifyAlarmChanged()
    {
        AlarmChanged?.Invoke(alarmLevel, IsFullAlarm());
    }
}
