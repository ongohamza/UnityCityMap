using UnityEngine;

public class CityDistractionTrigger : MonoBehaviour
{
    public string distractionId = "bakery_smoke";
    public string playerTag = "Player";
    public float durationSeconds = 18f;
    public int alarmChange = 0;
    public bool oneShot = true;

    private bool used;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used && oneShot) return;
        if (!other.CompareTag(playerTag)) return;

        used = true;

        if (CityAlarmDirector.Instance != null)
            CityAlarmDirector.Instance.TriggerDistraction(distractionId, durationSeconds, alarmChange);
    }
}
