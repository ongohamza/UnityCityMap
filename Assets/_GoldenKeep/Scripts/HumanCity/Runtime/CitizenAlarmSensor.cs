using UnityEngine;

public class CitizenAlarmSensor : MonoBehaviour
{
    public int alarmAmount = 1;
    public string playerTag = "Player";
    public float repeatSeconds = 2f;

    private float nextAlarmTime;

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryRaiseAlarm(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryRaiseAlarm(other);
    }

    private void TryRaiseAlarm(Collider2D other)
    {
        if (Time.time < nextAlarmTime) return;

        if (other.CompareTag(playerTag) && CityAlarmDirector.Instance != null)
        {
            nextAlarmTime = Time.time + repeatSeconds;
            CityAlarmDirector.Instance.RaiseAlarm(alarmAmount, name);
        }
    }
}
