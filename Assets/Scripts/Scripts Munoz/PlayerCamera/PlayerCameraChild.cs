using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraChild : MonoBehaviour
{
    public Transform playerBody;

    public float mouseSensitivity = 0.12f;
    public float controllerSensitivity = 120f;

    public float minPitch = -30f;
    public float maxPitch = 60f;

    public Vector3 cameraOffset = new Vector3(0f, 1.6f, -3.5f);

    private PlayerControls controls;
    private Vector2 lookInput;

    private float yaw;
    private float pitch;

    void Awake()
    {
        controls = new PlayerControls();

        controls.View.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.View.Look.canceled += ctx => lookInput = Vector2.zero;

        if (playerBody == null)
            playerBody = transform.parent;
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
        yaw = playerBody.eulerAngles.y;
        pitch = 10f;
    }

    void LateUpdate()
    {
        if (playerBody == null) return;

        bool usingMouse = Mouse.current != null && Mouse.current.delta.ReadValue() != Vector2.zero;

        float sensX = usingMouse ? mouseSensitivity : controllerSensitivity * Time.deltaTime;
        float sensY = usingMouse ? mouseSensitivity : controllerSensitivity * Time.deltaTime;

        yaw += lookInput.x * sensX;
        pitch -= lookInput.y * sensY;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        playerBody.rotation = Quaternion.Euler(0f, yaw, 0f);

        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 desiredPosition = playerBody.position + rotation * cameraOffset;

        transform.position = desiredPosition;

        transform.LookAt(playerBody.position + Vector3.up * 1.4f);
    }
}
