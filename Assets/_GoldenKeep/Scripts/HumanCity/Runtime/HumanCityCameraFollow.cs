using UnityEngine;

public class HumanCityCameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector2 targetOffset = new Vector2(0f, 2f);
    public Vector2 minPosition = new Vector2(0f, -5f);
    public Vector2 maxPosition = new Vector2(96f, 6f);
    public bool clampToViewBounds;
    public Vector2 viewMinPosition = new Vector2(-6f, -8f);
    public Vector2 viewMaxPosition = new Vector2(106f, 7f);
    public float followSpeed = 14f;

    private Camera followCamera;

    private void Awake()
    {
        followCamera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector2 desiredPosition = GetDesiredPosition();
        Vector3 desired = new Vector3(desiredPosition.x, desiredPosition.y, transform.position.z);

        float blend = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desired, blend);
    }

    public void SnapToTarget()
    {
        if (target == null)
            return;

        Vector2 desiredPosition = GetDesiredPosition();
        transform.position = new Vector3(desiredPosition.x, desiredPosition.y, transform.position.z);
    }

    private Vector2 GetDesiredPosition()
    {
        Vector2 desired = new Vector2(target.position.x + targetOffset.x, target.position.y + targetOffset.y);
        if (clampToViewBounds)
            return ClampToViewBounds(desired);

        return new Vector2(
            Mathf.Clamp(desired.x, minPosition.x, maxPosition.x),
            Mathf.Clamp(desired.y, minPosition.y, maxPosition.y));
    }

    private Vector2 ClampToViewBounds(Vector2 desired)
    {
        if (followCamera == null)
            followCamera = GetComponent<Camera>();

        if (followCamera == null || !followCamera.orthographic)
            return new Vector2(
                Mathf.Clamp(desired.x, minPosition.x, maxPosition.x),
                Mathf.Clamp(desired.y, minPosition.y, maxPosition.y));

        float halfHeight = followCamera.orthographicSize;
        float halfWidth = halfHeight * followCamera.aspect;
        return new Vector2(
            ClampAxis(desired.x, viewMinPosition.x + halfWidth, viewMaxPosition.x - halfWidth),
            ClampAxis(desired.y, viewMinPosition.y + halfHeight, viewMaxPosition.y - halfHeight));
    }

    private static float ClampAxis(float value, float min, float max)
    {
        if (min > max)
            return (min + max) * 0.5f;

        return Mathf.Clamp(value, min, max);
    }
}
