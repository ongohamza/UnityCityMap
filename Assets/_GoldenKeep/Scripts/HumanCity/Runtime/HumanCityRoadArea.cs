using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class HumanCityRoadArea : MonoBehaviour
{
    public string roadAreaId;
    public string fromRouteNodeId;
    public string toRouteNodeId;

    private BoxCollider2D roadCollider;

    private void Awake()
    {
        roadCollider = GetComponent<BoxCollider2D>();
    }

    public bool Contains(Vector2 worldPosition, float padding)
    {
        if (roadCollider == null)
            roadCollider = GetComponent<BoxCollider2D>();

        Bounds bounds = roadCollider.bounds;
        bounds.Expand(padding * 2f);
        return bounds.Contains(worldPosition);
    }
}
