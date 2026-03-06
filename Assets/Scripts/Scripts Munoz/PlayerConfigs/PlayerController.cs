using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 5f;
    public float jumpForce = 7f;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;

    public LayerMask groundLayer;

    [Header("Rol")]
    public PlayerRole role;

    [Header("Colores")]
    public Renderer playerRenderer;

    [Header("Ajustes por rol")]
    public float strongForce = 20f;
    public float lightJumpBoost = 14f;

    [Header("Interacción Strong")]
    public float pushForce = 15f;
    private Rigidbody objectToPush;

    private ButtonTrigger currentButton;

    [SerializeField] private bool overrideRole = false;
    [SerializeField] private PlayerRole debugRole;

    private UnstablePlatform currentPlatform;

    public Transform cameraTransform;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ApplyRole();
    }

        void ApplyRole()
    {
        if (playerRenderer == null)
        {
            playerRenderer = GetComponent<Renderer>();
        }

        Material mat = playerRenderer.material;

        switch (role)
        {
            case PlayerRole.Strong:
                mat.color = Color.red;
                break;

            case PlayerRole.Light:
                mat.color = Color.blue;
                jumpForce = lightJumpBoost;
                break;

            case PlayerRole.Activator:
                mat.color = Color.green;
                break;

            case PlayerRole.Stabilizer:
                mat.color = Color.yellow;
                break;
        }
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        if (role == PlayerRole.Activator && currentButton != null)
        {
            currentButton.Activate();
        }
    }
    public void SetRole(PlayerRole newRole)
    {
        role = newRole;
        ApplyRole();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
        }

        if (role == PlayerRole.Strong &&
            collision.gameObject.CompareTag("Pushable"))
        {
            objectToPush = collision.gameObject.GetComponent<Rigidbody>();
        }

        if (collision.gameObject.CompareTag("Button"))
        {
            currentButton = collision.gameObject.GetComponent<ButtonTrigger>();
        }
        if (role != PlayerRole.Stabilizer) return;

        UnstablePlatform platform = collision.gameObject.GetComponent<UnstablePlatform>();

        if (platform != null)
        {
            currentPlatform = platform;
            platform.SetStabilized(true);
        }
    }
    void HandlePush()
    {
        if (role != PlayerRole.Strong) return;
        if (objectToPush == null) return;

        Vector3 pushDirection = new Vector3(moveInput.x, 0, moveInput.y);

        if (pushDirection.magnitude > 0.1f)
        {
            objectToPush.AddForce(pushDirection * strongForce, ForceMode.Acceleration);
        }
    }
    private static int playerCount = 0;

    void Awake()
    {
        if (overrideRole)
        {
            role = debugRole;
        }
        else
        {
            role = (PlayerRole)(playerCount % 4);
            playerCount++;
        }
    }
    void FixedUpdate()
    {
        Move();
        HandlePush();
    }

    void Move()
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDirection = camForward * moveInput.y + camRight * moveInput.x;

        Vector3 velocity = moveDirection * speed;

        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    // INPUT SYSTEM (Send Messages)
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Pushable"))
        {
            objectToPush = null;
        }

        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
        }

        if (collision.gameObject.CompareTag("Button"))
        {
            currentButton = null;
        }
        if (role != PlayerRole.Stabilizer) return;

        UnstablePlatform platform = collision.gameObject.GetComponent<UnstablePlatform>();

        if (platform != null)
        {
            platform.SetStabilized(false);
            currentPlatform = null;
        }

    }
}