using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Transform player;
    public Transform cameraPivot;

    public float mouseSensitivity = 150f;
    public float controllerSensitivity = 200f;

    private PlayerControls controls;
    private Vector2 lookInput;

    float xRotation = 0f;
    float yRotation = 0f;

    void Awake()
    {
        controls = new PlayerControls();

        controls.View.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.View.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    void Start()
    {
        if (player == null)
            player = transform;

        if (cameraPivot == null)
            cameraPivot = transform.Find("CameraPivot");
    }

    void LateUpdate()
    {
        float sensitivity = Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero
            ? mouseSensitivity
            : controllerSensitivity;

        float mouseX = lookInput.x * sensitivity * Time.deltaTime;
        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        player.rotation = Quaternion.Euler(0f, yRotation, 0f);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}