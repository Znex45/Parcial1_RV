using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCameraChild : MonoBehaviour
{
    public Transform playerBody;

    [Header("Configuración de Input (¡Revisa tu Inspector!)")]
    [Tooltip("El nombre del Action Map. Ej: 'View' o 'Player'")]
    public string actionMapName = "View";
    [Tooltip("El nombre de la Acción. Ej: 'Look' o 'View'")]
    public string actionName = "Look";

    [Header("Sensibilidad")]
    public float sensitivity = 120f;
    public float minPitch = -30f;
    public float maxPitch = 60f;
    public Vector3 cameraOffset = new Vector3(0f, 1.6f, -3.5f);

    private float yaw;
    private float pitch;

    private InputActionMap viewMap;
    private InputAction lookAction;

    void Awake()
    {
        // 1. Buscamos el componente PlayerInput en la raíz del jugador
        PlayerInput pi = GetComponentInParent<PlayerInput>();

        if (pi != null)
        {
            // 2. Buscamos el Mapa y la Acción EXACTAMENTE como lo hacen tus compañeros.
            // El 'true' al final hace que Unity te muestre un error rojo en la consola si escribiste mal el nombre.
            viewMap = pi.actions.FindActionMap(actionMapName, true);
            lookAction = viewMap.FindAction(actionName, true);

            // 3. Encendemos el mapa entero a la fuerza
            viewMap.Enable();
        }
        else
        {
            Debug.LogError("No se encontró el PlayerInput en el objeto padre.");
        }

        if (playerBody == null) playerBody = transform.parent;
    }

    void Start()
    {
        if (playerBody != null) yaw = playerBody.eulerAngles.y;
        pitch = 10f;
    }

    void LateUpdate()
    {
        if (playerBody == null || lookAction == null) return;

        // Leemos el valor del joystick derecho o mouse
        Vector2 input = lookAction.ReadValue<Vector2>();

        yaw += input.x * sensitivity * Time.deltaTime;
        pitch -= input.y * sensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Rotar al jugador horizontalmente
        playerBody.rotation = Quaternion.Euler(0f, yaw, 0f);

        // Posicionar la cámara
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.position = playerBody.position + (rotation * cameraOffset);

        transform.LookAt(playerBody.position + Vector3.up * 1.4f);
    }

    void OnDestroy()
    {
        // Limpiamos la memoria al destruir al jugador
        if (viewMap != null) viewMap.Disable();
    }
}
