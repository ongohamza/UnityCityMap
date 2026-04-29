using System;
using System.Collections.Generic;

[Serializable]
public class CityRouteGraphDatabase
{
    public List<CityRouteNodeRecord> nodes = new List<CityRouteNodeRecord>();
    public List<CityRouteEdgeRecord> edges = new List<CityRouteEdgeRecord>();
}

[Serializable]
public class CityRouteNodeRecord
{
    public string id;
    public string label;
    public string type;
    public string choiceMeaning;
}

[Serializable]
public class CityRouteEdgeRecord
{
    public string from;
    public string to;
    public string choice;
    public string risk;
    public string requires;
    public string creates;
}
