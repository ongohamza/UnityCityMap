using System;
using System.Collections.Generic;

[Serializable]
public class CityBuildingDatabase
{
    public List<CityBuildingRecord> buildings = new List<CityBuildingRecord>();
}

[Serializable]
public class CityBuildingRecord
{
    public string id;
    public string name;
    public string type;
    public string zone;
    public float x;
    public float y;
    public string function;
    public List<string> routes = new List<string>();
}
