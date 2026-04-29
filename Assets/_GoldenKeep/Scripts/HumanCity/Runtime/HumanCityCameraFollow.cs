using UnityEngine;

public class HumanCityCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector2 targetOffset = new Vector2(0f, 2f);
    public Vector2 minPosition = new Vector2(0f, -5f);
    public Vector2 maxPosition = new Vector2(96f, 6f);
    public float followSpeed = 14f;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desired = new Vector3(
            Mathf.Clamp(target.position.x + targetOffset.x, minPosition.x, maxPosition.x),
            Mathf.Clamp(target.position.y + targetOffset.y, minPosition.y, maxPosition.y),
            transform.position.z);

        float blend = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desired, blend);
    }

    public void SnapToTarget()
    {
        if (target == null)
            return;

        transform.position = new Vector3(
            Mathf.Clamp(target.position.x + targetOffset.x, minPosition.x, maxPosition.x),
            Mathf.Clamp(target.position.y + targetOffset.y, minPosition.y, maxPosition.y),
            transform.position.z);
    }
}
