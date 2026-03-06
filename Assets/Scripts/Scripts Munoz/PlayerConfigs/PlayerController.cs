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
    public Renderer playerRenderer;
    public float strongForce = 20f;
    public float lightJumpBoost = 14f;

    [Header("Configuración")]
    public Transform cameraTransform;
    private static int playerCount = 0;
    [SerializeField] private bool overrideRole = false;
    [SerializeField] private PlayerRole debugRole;
    private Rigidbody objectToPush;
    private ButtonTrigger currentButton;

    void Awake()
    {
        if (overrideRole) role = debugRole;
        else { role = (PlayerRole)(playerCount % 4); playerCount++; }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        ApplyRole();
    }

    void FixedUpdate()
    {
        Move();
        HandlePush();
    }

    // Recibe el input de movimiento de Unity
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void Move()
    {
        if (cameraTransform == null) return;
        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
        Vector3 velocity = (camForward * moveInput.y + camRight * moveInput.x) * speed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);
    }

    void HandlePush()
    {
        if (role == PlayerRole.Strong && objectToPush != null)
        {
            Vector3 pushDir = new Vector3(moveInput.x, 0, moveInput.y);
            if (pushDir.magnitude > 0.1f) objectToPush.AddForce(pushDir * strongForce, ForceMode.Acceleration);
        }
    }

    // Fundamental para que PlayerManager.cs no arroje el error CS1061
    public void SetRole(PlayerRole newRole)
    {
        role = newRole;
        ApplyRole();
    }

    void ApplyRole()
    {
        if (playerRenderer == null) playerRenderer = GetComponent<Renderer>();
        if (playerRenderer == null) return;
        switch (role)
        {
            case PlayerRole.Strong: playerRenderer.material.color = Color.red; break;
            case PlayerRole.Light: playerRenderer.material.color = Color.blue; jumpForce = lightJumpBoost; break;
            case PlayerRole.Activator: playerRenderer.material.color = Color.green; break;
            case PlayerRole.Stabilizer: playerRenderer.material.color = Color.yellow; break;
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (((1 << col.gameObject.layer) & groundLayer) != 0) isGrounded = true;
        if (role == PlayerRole.Strong && col.gameObject.CompareTag("Pushable")) objectToPush = col.gameObject.GetComponent<Rigidbody>();
    }
    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Pushable")) objectToPush = null;
        if (((1 << col.gameObject.layer) & groundLayer) != 0) isGrounded = false;
    }
}