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

            case PlayerRole.Connector:
                mat.color = Color.yellow;
                break;
        }
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }

        if (role == PlayerRole.Strong &&
            collision.gameObject.CompareTag("Pushable"))
        {
            objectToPush = collision.gameObject.GetComponent<Rigidbody>();
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
        role = (PlayerRole)(playerCount % 4);
        playerCount++;
    }
    void FixedUpdate()
    {
        Move();
        HandlePush();
    }

    void Move()
    {
        Vector3 movement = new Vector3(moveInput.x, 0, moveInput.y);
        Vector3 velocity = movement * speed;

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
        if (collision.gameObject.CompareTag("Ground"))
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

        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}