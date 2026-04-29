using UnityEngine;

public class HumanCityObjectiveHud : MonoBehaviour
{
    public bool showHud = true;

    private void OnGUI()
    {
        if (!showHud) return;

        int alarm = CityAlarmDirector.Instance != null ? CityAlarmDirector.Instance.alarmLevel : 0;
        bool fullAlarm = CityAlarmDirector.Instance != null && CityAlarmDirector.Instance.IsFullAlarm();
        int loot = CityRaidObjective.Instance != null ? CityRaidObjective.Instance.collectedLootCount : 0;
        int required = CityRaidObjective.Instance != null ? CityRaidObjective.Instance.requiredLootCount : 1;
        bool complete = CityRaidObjective.Instance != null && CityRaidObjective.Instance.raidComplete;

        GUI.Box(new Rect(16f, 16f, 330f, 112f), string.Empty);
        GUI.Label(new Rect(32f, 28f, 300f, 24f), "Human City Raid");
        GUI.Label(new Rect(32f, 52f, 300f, 24f), "Loot: " + loot + " / " + required);
        GUI.Label(new Rect(32f, 76f, 300f, 24f), "Alarm: " + alarm + (fullAlarm ? " FULL" : string.Empty));
        GUI.Label(new Rect(32f, 100f, 300f, 24f), complete ? "CITY RAID COMPLETE" : "Steal storehouse loot and escape.");
    }
}
