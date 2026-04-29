using UnityEngine;

public class CityLootPickup : MonoBehaviour
{
    public string playerTag = "Player";
    public int lootAmount = 1;
    public int alarmOnPickup = 1;
    public bool hideAfterPickup = true;

    private bool collected;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag(playerTag)) return;

        collected = true;

        if (CityRaidObjective.Instance != null)
            CityRaidObjective.Instance.CollectLoot(lootAmount);

        if (CityAlarmDirector.Instance != null && alarmOnPickup > 0)
            CityAlarmDirector.Instance.RaiseAlarm(alarmOnPickup, name);

        if (hideAfterPickup)
            gameObject.SetActive(false);
    }
}
