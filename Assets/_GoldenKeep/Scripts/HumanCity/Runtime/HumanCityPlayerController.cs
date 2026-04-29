using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class HumanCityPlayerController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float roadProbeRadius = 0.35f;
    public bool constrainToRoads = true;

    private Rigidbody2D body;
    private HumanCityRoadArea[] roadAreas;
    private bool roadAreasAvailable;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;
        body.freezeRotation = true;

        Collider2D bodyCollider = GetComponent<Collider2D>();
        bodyCollider.isTrigger = true;
    }

    private void Start()
    {
        roadAreas = FindObjectsByType<HumanCityRoadArea>(FindObjectsSortMode.None);
        roadAreasAvailable = roadAreas.Length > 0;
    }

    private void FixedUpdate()
    {
        Vector2 moveInput = ReadMoveInput();
        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        Vector2 current = body.position;
        Vector2 delta = moveInput * moveSpeed * Time.fixedDeltaTime;
        Vector2 next = current + delta;

        if (CanMoveTo(next))
        {
            body.MovePosition(next);
            return;
        }

        TryMoveOnSingleAxis(current, delta);
    }

    private void TryMoveOnSingleAxis(Vector2 current, Vector2 delta)
    {
        Vector2 firstAxis = Mathf.Abs(delta.x) >= Mathf.Abs(delta.y)
            ? new Vector2(delta.x, 0f)
            : new Vector2(0f, delta.y);
        Vector2 secondAxis = firstAxis.x != 0f
            ? new Vector2(0f, delta.y)
            : new Vector2(delta.x, 0f);

        Vector2 next = current + firstAxis;
        if (CanMoveTo(next))
        {
            body.MovePosition(next);
            return;
        }

        next = current + secondAxis;
        if (CanMoveTo(next))
            body.MovePosition(next);
    }

    private bool CanMoveTo(Vector2 position)
    {
        if (!constrainToRoads || !roadAreasAvailable)
            return true;

        for (int i = 0; i < roadAreas.Length; i++)
        {
            if (roadAreas[i] != null && roadAreas[i].Contains(position, roadProbeRadius))
                return true;
        }

        return false;
    }

    private Vector2 ReadMoveInput()
    {
        Vector2 value = Vector2.zero;
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) value.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) value.x += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) value.y -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) value.y += 1f;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) value.x -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) value.x += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) value.y -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) value.y += 1f;
#endif
        return value;
    }
}
