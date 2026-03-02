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

        Vector3 move = camForward * moveInput.y + camRight * moveInput.x;

        if (controller.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        if (move.sqrMagnitude > 0.01f)
            transform.forward = new Vector3(move.x, 0f, move.z);

        controller.Move(move * speed * Time.deltaTime);
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
