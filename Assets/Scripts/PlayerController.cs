using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Input (drag from your Input Actions asset)")]
    [SerializeField] InputActionReference moveAction;   // Player/Move  (Vector2)
    [SerializeField] InputActionReference jumpAction;   // Player/Jump  (Button)

    [Header("Run")]
    [SerializeField] float moveSpeed = 8f;
    [SerializeField] float acceleration = 90f;
    [SerializeField] float deceleration = 110f;
    [Range(0f, 1f)] [SerializeField] float airControl = 0.75f;

    [Header("Jump")]
    [SerializeField] float jumpVelocity = 15f;
    [SerializeField] float coyoteTime = 0.1f;
    [SerializeField] float jumpBufferTime = 0.1f;

    [Header("Gravity")]
    [SerializeField] float baseGravity = 4f;
    [SerializeField] float fallMultiplier = 1.5f;      // falls faster than it rises
    [SerializeField] float lowJumpMultiplier = 2.5f;   // releasing jump early cuts the jump short
    [SerializeField] float maxFallSpeed = 25f;

    [Header("Ground Check")]
    [Tooltip("Include the Ground layer AND the Clone layer so clones count as floor.")]
    [SerializeField] LayerMask groundMask;
    [SerializeField] float groundCheckDepth = 0.06f;

    Rigidbody2D rb;
    BoxCollider2D box;

    float moveInput;
    bool jumpHeld;
    bool grounded;
    float coyoteCounter;
    float bufferCounter;

    // +1 = normal gravity (falls down), -1 = inverted (falls up)
    float gravityDir = 1f;

    public bool GravityInverted => gravityDir < 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        rb.freezeRotation = true;
        rb.gravityScale = baseGravity;
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();
    }

    void Update()
    {
        // --- input ---
        moveInput = moveAction.action.ReadValue<Vector2>().x;
        bool jumpPressed = jumpAction.action.WasPressedThisFrame();
        jumpHeld = jumpAction.action.IsPressed();

        // --- ground + timers ---
        // "up" velocity is relative to gravity, so this works upside down too.
        // The velocity check stops the ground box from re-triggering right after we jump.
        float upVel = rb.linearVelocity.y * gravityDir;
        grounded = CheckGrounded() && upVel <= 0.1f;

        coyoteCounter = grounded ? coyoteTime : coyoteCounter - Time.deltaTime;
        bufferCounter = jumpPressed ? jumpBufferTime : bufferCounter - Time.deltaTime;

        // --- jump ---
        if (bufferCounter > 0f && coyoteCounter > 0f)
        {
            var v = rb.linearVelocity;
            v.y = jumpVelocity * gravityDir;
            rb.linearVelocity = v;
            bufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }

    void FixedUpdate()
    {
        var v = rb.linearVelocity;

        // horizontal
        float target = moveInput * moveSpeed;
        float rate = Mathf.Abs(moveInput) > 0.01f ? acceleration : deceleration;
        if (!grounded) rate *= airControl;
        v.x = Mathf.MoveTowards(v.x, target, rate * Time.fixedDeltaTime);

        // vertical (measured relative to gravity: positive = moving "up")
        float upVel = v.y * gravityDir;
        upVel = Mathf.Max(upVel, -maxFallSpeed);
        v.y = upVel * gravityDir;
        rb.linearVelocity = v;

        float g = baseGravity * gravityDir;   // a negative gravityScale pulls upward
        if (upVel < 0f)                      rb.gravityScale = g * fallMultiplier;
        else if (upVel > 0f && !jumpHeld)    rb.gravityScale = g * lowJumpMultiplier;
        else                                 rb.gravityScale = g;
    }

    bool CheckGrounded()
    {
        Bounds b = box.bounds;
        // feet are on the bottom normally, on the top when gravity is inverted
        float y = gravityDir > 0f ? b.min.y - groundCheckDepth * 0.5f
                                  : b.max.y + groundCheckDepth * 0.5f;
        Vector2 center = new Vector2(b.center.x, y);
        Vector2 size = new Vector2(b.size.x * 0.9f, groundCheckDepth);
        return Physics2D.OverlapBox(center, size, 0f, groundMask) != null;
    }

    /// <summary>1 = normal gravity, -1 = inverted.</summary>
    public void SetGravityDirection(float direction)
    {
        gravityDir = direction < 0f ? -1f : 1f;
    }

    public void FlipGravity()
    {
        SetGravityDirection(-gravityDir);
    }

    /// <summary>Instantly moves the player, clears motion/input buffers, and restores normal gravity.</summary>
    public void TeleportTo(Vector2 position)
    {
        SetGravityDirection(1f);
        rb.linearVelocity = Vector2.zero;
        rb.position = position;
        transform.position = position;
        coyoteCounter = 0f;
        bufferCounter = 0f;
    }

    void OnDrawGizmosSelected()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col == null) return;
        Bounds b = col.bounds;
        float y = gravityDir > 0f ? b.min.y - groundCheckDepth * 0.5f
                                  : b.max.y + groundCheckDepth * 0.5f;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(new Vector3(b.center.x, y, 0f),
                            new Vector3(b.size.x * 0.9f, groundCheckDepth, 0f));
    }
}