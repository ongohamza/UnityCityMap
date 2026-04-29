using UnityEngine;
using UnityEngine.Events;

public class CityRaidObjective : MonoBehaviour
{
    public static CityRaidObjective Instance { get; private set; }

    public int requiredLootCount = 1;
    public int collectedLootCount;
    public bool raidComplete;
    public UnityEvent lootChanged;
    public UnityEvent raidCompleted;

    private void Awake()
    {
        Instance = this;
    }

    public void CollectLoot(int amount)
    {
        collectedLootCount += amount;
        lootChanged?.Invoke();
    }

    public bool HasRequiredLoot()
    {
        return collectedLootCount >= requiredLootCount;
    }

    public void CompleteRaid()
    {
        if (raidComplete || !HasRequiredLoot()) return;

        raidComplete = true;
        raidCompleted?.Invoke();
    }
}
