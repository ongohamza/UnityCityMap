using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class HumanCityPlayerController : MonoBehaviour
{
    public float moveSpeed = 7f;
    public float climbSpeed = 5f;
    public float jumpImpulse = 11f;
    public LayerMask groundMask = ~0;

    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private int ladderContacts;
    private bool jumpQueued;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        body.freezeRotation = true;
    }

    private void Update()
    {
        if (IsJumpPressed())
            jumpQueued = true;
    }

    private void FixedUpdate()
    {
        float horizontal = ReadHorizontal();
        float vertical = ReadVertical();
        Vector2 velocity = body.linearVelocity;

        velocity.x = horizontal * moveSpeed;

        if (ladderContacts > 0 && Mathf.Abs(vertical) > 0.01f)
        {
            velocity.y = vertical * climbSpeed;
        }
        else if (jumpQueued && IsGrounded())
        {
            velocity.y = jumpImpulse;
        }

        body.linearVelocity = velocity;
        jumpQueued = false;
    }

    private bool IsGrounded()
    {
        return bodyCollider != null && bodyCollider.IsTouchingLayers(groundMask);
    }

    private float ReadHorizontal()
    {
        float value = 0f;
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) value -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) value += 1f;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) value -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) value += 1f;
#endif
        return Mathf.Clamp(value, -1f, 1f);
    }

    private float ReadVertical()
    {
        float value = 0f;
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) value -= 1f;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) value += 1f;
        }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) value -= 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) value += 1f;
#endif
        return Mathf.Clamp(value, -1f, 1f);
    }

    private bool IsJumpPressed()
    {
        bool pressed = false;
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        pressed |= keyboard != null &&
                   (keyboard.spaceKey.wasPressedThisFrame ||
                    keyboard.wKey.wasPressedThisFrame ||
                    keyboard.upArrowKey.wasPressedThisFrame);
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
        pressed |= Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
#endif
        return pressed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<HumanCityLadderZone>() != null)
            ladderContacts++;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<HumanCityLadderZone>() != null)
            ladderContacts = Mathf.Max(0, ladderContacts - 1);
    }
}
