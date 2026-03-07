using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    CharacterController controller;
    PlayerInput playerInput;

    InputAction walkAction;
    InputAction jumpAction;

    Vector2 moveInput;
    float verticalVelocity;

    Transform cameraTransform;
    Vector3 spawnPosition;
    Quaternion spawnRotation;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        // Acciones del Action Map PlayerMove (las que ya tienes configuradas)
        walkAction = playerInput.actions.FindActionMap("PlayerMove").FindAction("Walk");
        jumpAction = playerInput.actions.FindActionMap("PlayerMove").FindAction("Jump");

        // Cada jugador toma SU propia camara (hija)
        cameraTransform = GetComponentInChildren<Camera>().transform;
    }

    void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    void Respawn()
    {
        // Desactivar el controller para poder mover el transform directamente
        controller.enabled = false;
        transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        verticalVelocity = 0f;
        controller.enabled = true;
    }

    void OnEnable()
    {
        walkAction.performed += OnMove;
        walkAction.canceled += OnMove;
        jumpAction.performed += OnJump;
    }

    void OnDisable()
    {
        walkAction.performed -= OnMove;
        walkAction.canceled -= OnMove;
        jumpAction.performed -= OnJump;
    }

    void Update()
    {
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        camForward.Normalize();
        camRight.Normalize();

        // Movimiento horizontal separado del vertical para evitar wall-sticking
        Vector3 horizontalMove = camForward * moveInput.y + camRight * moveInput.x;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        // Si golpeamos algo por arriba (techo/cornisa), cancelar velocidad ascendente
        if ((controller.collisionFlags & CollisionFlags.Above) != 0 && verticalVelocity > 0f)
            verticalVelocity = 0f;

        verticalVelocity += gravity * Time.deltaTime;

        // Rotar personaje solo segun el horizontal
        if (horizontalMove.sqrMagnitude > 0.01f)
            transform.forward = horizontalMove;

        // Movimiento horizontal (sin Y) * speed  +  vertical independiente
        Vector3 finalMove = horizontalMove * speed + Vector3.up * verticalVelocity;
        controller.Move(finalMove * Time.deltaTime);
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        if (controller.isGrounded)
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
}
