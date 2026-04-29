using UnityEngine;

public class CityEscapeTrigger : MonoBehaviour
{
    public string playerTag = "Player";
    public int alarmIfNoLoot = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (CityRaidObjective.Instance != null && CityRaidObjective.Instance.HasRequiredLoot())
        {
            CityRaidObjective.Instance.CompleteRaid();
            return;
        }

        if (CityAlarmDirector.Instance != null && alarmIfNoLoot > 0)
            CityAlarmDirector.Instance.RaiseAlarm(alarmIfNoLoot, name);
    }
}
