using UnityEngine;

public class CityWaypoint : MonoBehaviour
{
    public string waypointId;
    public string displayName;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
}
