using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador de la vista 360 dentro de la Observation Station.
///
/// COMPORTAMIENTO:
///  - Cursor LIBRE y VISIBLE en todo momento (Confined dentro de la ventana).
///  - Click IZQUIERDO           → dispara raycast desde la posicion del cursor.
///  - Click DERECHO MANTENIDO   → arrastra la camara (look 360).
///  - Exit360 (Escape)          → sale del modo 360.
/// </summary>
[RequireComponent(typeof(Camera))]
public class ObservationView360 : MonoBehaviour
{
    [Header("Look (drag con click derecho)")]
    [Tooltip("Sensibilidad del arrastre en grados por pixel.")]
    public float sensitivity   = 0.2f;
    [Range(10f, 89f)]
    public float verticalClamp = 80f;

    [Header("Shoot (click izquierdo)")]
    [Tooltip("Tag de los objetos destruibles dentro de la esfera.")]
    public string    destroyableTag = "Target360";
    public float     rayDistance    = 200f;
    public LayerMask rayMask        = ~0;

    // -- refs inyectadas por ObservationStation --
    ObservationStation _station;
    InputActionMap     _map;
    InputAction        _shootAction;   // click izquierdo → dispara
    InputAction        _exitAction;    // Escape          → salir
    Camera             _cam;

    float   _yaw;
    float   _pitch;
    Vector2 _lastMousePos;
    bool    _wasDragging;

    // ================================================================ API publica
    public void Initialize(ObservationStation station, PlayerInput playerInput)
    {
        _station = station;
        _cam     = GetComponent<Camera>();

        _map         = playerInput.actions.FindActionMap("Player", true);
        _shootAction = _map.FindAction("Shoot",   true);
        _exitAction  = _map.FindAction("Exit360", true);
    }

    // ================================================================ Unity callbacks
    void OnEnable()
    {
        if (_map == null) return;

        Vector3 e = transform.eulerAngles;
        _yaw   = e.y;
        _pitch = e.x > 180f ? e.x - 360f : e.x;

        _shootAction.performed += OnShoot;
        _exitAction.performed  += OnExit;

        // Cursor libre y visible para apuntar
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible   = true;
    }

    void OnDisable()
    {
        if (_map == null) return;

        _shootAction.performed -= OnShoot;
        _exitAction.performed  -= OnExit;

        _wasDragging = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    void Update()
    {
        HandleCameraLook();
    }

    // ================================================================ Look con drag
    void HandleCameraLook()
    {
        if (Mouse.current == null) return;

        bool isDragging = Mouse.current.rightButton.isPressed;
        Vector2 currentMousePos = Mouse.current.position.ReadValue();

        if (isDragging)
        {
            if (_wasDragging)
            {
                Vector2 delta = currentMousePos - _lastMousePos;
                _yaw   += delta.x * sensitivity;
                _pitch -= delta.y * sensitivity;
                _pitch  = Mathf.Clamp(_pitch, -verticalClamp, verticalClamp);
                transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
            }
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }

        _lastMousePos = currentMousePos;
        // _wasDragging se actualiza AL FINAL para que OnShoot (que llega via evento)
        // lea el valor correcto del frame actual, no el del frame anterior.
        _wasDragging = isDragging;
    }

    // ================================================================ Input handlers
    void OnShoot(InputAction.CallbackContext ctx)
    {
        if (_station == null || !_station.IsRunning) return;
        if (Mouse.current == null) return;

        // Solo bloquea si el click derecho esta ACTUALMENTE presionado (drag activo ahora mismo)
        if (Mouse.current.rightButton.isPressed) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Ray ray = _cam.ScreenPointToRay(new Vector3(screenPos.x, screenPos.y, 0f));

        Debug.Log($"[360] Shoot ray desde {screenPos}");

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, rayMask))
        {
            Debug.Log($"[360] Hit: {hit.collider.name} | tag: {hit.collider.tag}");
            if (hit.collider.CompareTag(destroyableTag))
                _station.RegisterHit(hit.collider.gameObject);
        }
        else
        {
            Debug.Log("[360] Raycast no impacto nada");
        }
    }

    void OnExit(InputAction.CallbackContext ctx) => _station?.RequestExit();
}
