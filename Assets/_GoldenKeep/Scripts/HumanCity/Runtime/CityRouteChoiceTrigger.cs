using UnityEngine;

public class CityRouteChoiceTrigger : MonoBehaviour
{
    public string routeNodeId;
    public string choiceLabel;
    public string playerTag = "Player";
    public CityRouteRisk risk = CityRouteRisk.Low;
    public bool oneShot = true;
    public bool requiresLoot;
    public string requiredDistractionId;
    public int missingRequirementAlarmPenalty = 1;

    private bool used;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (used && oneShot) return;
        if (!other.CompareTag(playerTag)) return;

        used = true;

        int alarmCost = GetAlarmCost(risk);
        CityAlarmDirector director = CityAlarmDirector.Instance;

        if (requiresLoot && (CityRaidObjective.Instance == null || !CityRaidObjective.Instance.HasRequiredLoot()))
            alarmCost += missingRequirementAlarmPenalty;

        if (!string.IsNullOrEmpty(requiredDistractionId) && (director == null || !director.IsDistractionActive(requiredDistractionId)))
            alarmCost += missingRequirementAlarmPenalty;

        if (director != null)
            director.RecordRouteChoice(routeNodeId, choiceLabel, alarmCost);
    }

    private int GetAlarmCost(CityRouteRisk routeRisk)
    {
        switch (routeRisk)
        {
            case CityRouteRisk.None:
                return 0;
            case CityRouteRisk.Low:
                return 0;
            case CityRouteRisk.Medium:
                return 1;
            case CityRouteRisk.High:
                return 2;
            case CityRouteRisk.VeryHigh:
                return 3;
            default:
                return 0;
        }
    }
}

public enum CityRouteRisk
{
    None,
    Low,
    Medium,
    High,
    VeryHigh
}
