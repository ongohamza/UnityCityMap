using System;
using System.Collections.Generic;

[Serializable]
public class CitizenDatabase
{
    public List<CitizenRecord> citizens = new List<CitizenRecord>();
}

[Serializable]
public class CitizenRecord
{
    public string id;
    public string name;
    public string role;
    public string personality;
    public string prefabKey;
    public CombatRecord combat;
    public List<ScheduleBlock> schedule = new List<ScheduleBlock>();
    public string playerOpportunity;
}

[Serializable]
public class CombatRecord
{
    public string attackType;
    public string movement;
    public string threatLevel;
    public string alarmBehavior;
}

[Serializable]
public class ScheduleBlock
{
    public string start;
    public string end;
    public string locationId;
    public string activity;
}
