using UnityEngine;

public class HumanCityMapManifest : MonoBehaviour
{
    [Header("Primitive data")]
    public TextAsset buildingsJson;
    public TextAsset citizensJson;
    public TextAsset routeGraphJson;

    [Header("Source art")]
    public string sourceBackdropPath;
    public string visualReferencePath;

    [TextArea(3, 8)]
    public string architectureNote =
        "Primitive: city IDs. Black boxes: data, map builder, schedule simulation, alarm/objective gameplay. " +
        "The scene can be rebuilt from JSON and art without changing gameplay code.";
}
