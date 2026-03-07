﻿using UnityEngine;
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
    [Header("Ground Check")]
    public float groundCheckRadius = 0.3f;
    public float groundCheckDistance = 0.15f;
    [Header("Air Control")]
    [Range(0f, 1f)] public float airControlFactor = 0.15f;
    public float wallCheckDistance = 0.15f;  // distancia del cast lateral para detectar pared

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

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip walkSound;
    public AudioClip jumpSound;

    private UnstablePlatform currentPlatform;

    private bool isWalking;
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
        CheckGrounded();
        Move();
        HandlePush();
    }

    void CheckGrounded()
    {
        // SphereCast desde el centro del collider hacia abajo — no le afecta rozar paredes laterales
        Vector3 origin = transform.position + Vector3.up * groundCheckRadius;
        isGrounded = Physics.SphereCast(origin, groundCheckRadius, Vector3.down,
                                        out _, groundCheckRadius + groundCheckDistance, groundLayer);
    }

    // Recibe el input de movimiento de Unity
    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && isGrounded)
        {
            audioSource.Stop();

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            audioSource.loop = false;
            audioSource.PlayOneShot(jumpSound);
        }
    }

    void Move()
    {
        if (cameraTransform == null) return;
        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight   = Vector3.ProjectOnPlane(cameraTransform.right,   Vector3.up).normalized;
        Vector3 desiredVelocity = (camForward * moveInput.y + camRight * moveInput.x) * speed;

        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(desiredVelocity.x, rb.linearVelocity.y, desiredVelocity.z);
        }
        else
        {
            // Si el desired apunta hacia una pared, eliminar esa componente
            Vector3 safeDesired = ClipVelocityAgainstWalls(desiredVelocity);

            float newX = Mathf.Lerp(rb.linearVelocity.x, safeDesired.x, airControlFactor);
            float newZ = Mathf.Lerp(rb.linearVelocity.z, safeDesired.z, airControlFactor);
            rb.linearVelocity = new Vector3(newX, rb.linearVelocity.y, newZ);
        }

        // SONIDO DE PASOS
        if (desiredVelocity.magnitude > 0.1f && isGrounded)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = walkSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.clip == walkSound)
                audioSource.Stop();
        }
    }

    /// <summary>
    /// Devuelve el vector de velocidad deseado sin la componente que choca contra paredes laterales.
    /// </summary>
    Vector3 ClipVelocityAgainstWalls(Vector3 velocity)
    {
        if (velocity.sqrMagnitude < 0.001f) return velocity;

        Vector3 dir = velocity.normalized;
        float radius = groundCheckRadius * 0.9f;  // usa un radio ligeramente menor al del cuerpo

        // Cast en la dirección del movimiento horizontal
        if (Physics.CapsuleCast(
                transform.position + Vector3.up * radius,
                transform.position + Vector3.up * (GetComponent<Collider>().bounds.size.y - radius),
                radius,
                dir,
                out RaycastHit hit,
                wallCheckDistance,
                groundLayer))
        {
            // Normal de la pared hit
            Vector3 wallNormal = hit.normal;
            wallNormal.y = 0f;
            wallNormal.Normalize();

            // Si el input apunta hacia la pared (dot negativo), proyectar sobre el plano de la pared
            if (Vector3.Dot(dir, -wallNormal) > 0f)
            {
                velocity = Vector3.ProjectOnPlane(velocity, wallNormal);
            }
        }

        return velocity;
    }

    void HandlePush()
    {
        if (role != PlayerRole.Strong) return;
        if (objectToPush == null) return;
        if (cameraTransform == null) return;

        Vector3 camForward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;

        Vector3 pushDir = camForward * moveInput.y + camRight * moveInput.x;

        if (pushDir.magnitude > 0.1f)
        {
            objectToPush.AddForce(pushDir * strongForce, ForceMode.Impulse);
        }
    }

    // Fundamental para que PlayerManager.cs no arroje el error CS1061
    public void SetRole(PlayerRole newRole)
    {
        role = newRole;
        ApplyRole();
    }
    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        if (role == PlayerRole.Activator && currentButton != null)
        {
            currentButton.Activate();
        }
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
        if (role == PlayerRole.Strong && col.gameObject.CompareTag("Pushable")) objectToPush = col.gameObject.GetComponent<Rigidbody>();
        if (col.gameObject.CompareTag("Button"))
            currentButton = col.gameObject.GetComponent<ButtonTrigger>();
        if (role == PlayerRole.Stabilizer)
        {
            UnstablePlatform platform = col.gameObject.GetComponent<UnstablePlatform>();
            if (platform != null)
            {
                currentPlatform = platform;
                platform.SetStabilized(true);
            }
        }
    }
    void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Pushable")) objectToPush = null;
        if (col.gameObject.CompareTag("Button"))
            currentButton = null;
        if (role == PlayerRole.Stabilizer)
        {
            UnstablePlatform platform = col.gameObject.GetComponent<UnstablePlatform>();

            if (platform != null)
            {
                platform.SetStabilized(false);
                currentPlatform = null;
            }
        }
    }
}