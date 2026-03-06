using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Observation Station — nucleo del sistema de observacion 360.
///
/// SETUP MANUAL EN EDITOR:
///  1. Crea un GameObject "ObservationStation" con este componente.
///  2. Crea una Sphere Trigger alrededor de la estacion (IsTrigger = true) y asignala a "interactTrigger".
///  3. Crea una camara hija, desactivada, con ObservationView360 -> asignala a "view360".
///  4. Asigna la camara principal del jugador a "playerCamera".
///  5. Asigna la puerta (SlidingDoor) a "door".
///  6. Asigna los objetos destruibles (con tag "Target360") al array "targets".
///  7. El jugador debe tener PlayerInput con el asset IActs.
/// </summary>
public class ObservationStation : MonoBehaviour
{
    // ------------------------------------------------------------------ inspector
    [Header("References")]
    [Tooltip("Camara principal del jugador (se desactiva al entrar en modo 360).")]
    public Camera playerCamera;

    [Tooltip("Camara 360 con componente ObservationView360 (desactivada en escena).")]
    public ObservationView360 view360;

    [Tooltip("Puerta que se abre al completar el desafio.")]
    public SlidingDoor door;

    [Header("Challenge")]
    [Tooltip("Objetos destruibles dentro de la esfera (tag Target360).")]
    public GameObject[] targets;

    [Tooltip("Tiempo limite en segundos para destruir todos los objetivos.")]
    [Min(1f)] public float timeLimit = 30f;

    [Tooltip("Cuantos objetos hay que destruir para ganar (0 = todos).")]
    [Min(0)]  public int   requiredHits = 0;

    [Header("Proximity")]
    [Tooltip("Distancia maxima al jugador para poder interactuar.")]
    public float interactRadius = 3f;

    // ------------------------------------------------------------------ estado
    bool  _sessionActive;      // dentro de la vista 360
    bool  _doorUnlocked;       // reto superado permanentemente
    float _timeRemaining;
    int   _hitsLeft;

    // cache del jugador
    Transform   _playerTransform;
    PlayerInput _playerInput;

    // refs a los action maps del jugador para pausarlos/reanudarlos
    InputActionMap _playerMap;
    InputActionMap _viewMap;

    // acciones de movimiento que se bloquean durante la sesion 360
    InputAction _moveAction;
    InputAction _jumpAction;
    InputAction _grabAction;

    // input de interact
    InputAction _interactAction;

    // ================================================================ propiedades publicas
    public bool IsRunning      => _sessionActive && !_doorUnlocked;
    public float TimeRemaining => _timeRemaining;
    public int   HitsLeft      => _hitsLeft;

    // ================================================================ Unity lifecycle
    void Awake()
    {
        if (requiredHits <= 0 || requiredHits > targets.Length)
            requiredHits = targets.Length;
    }

    void Start()
    {
        // Busca al jugador por tag (evita dependencia de referencia manual)
        GameObject player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[ObservationStation] No se encontro GameObject con tag 'Player'.");
            enabled = false;
            return;
        }

        _playerTransform = player.transform;
        _playerInput     = player.GetComponentInChildren<PlayerInput>();
        if (_playerInput == null)
            _playerInput = player.GetComponent<PlayerInput>();

        if (_playerInput == null)
        {
            Debug.LogError("[ObservationStation] PlayerInput no encontrado en el jugador.");
            enabled = false;
            return;
        }

        _playerMap = _playerInput.actions.FindActionMap("Player", true);
        _viewMap   = _playerInput.actions.FindActionMap("View",   true);

        _moveAction     = _playerMap.FindAction("Move",     true);
        _jumpAction     = _playerMap.FindAction("Jump",     true);
        _grabAction     = _playerMap.FindAction("Grab",     false); // opcional
        _interactAction = _playerMap.FindAction("Interact", true);

        _interactAction.performed += OnInteract;

        // Inicializa la camara 360 con las dependencias necesarias
        view360.Initialize(this, _playerInput);

        // Asegura que la camara 360 empiece desactivada
        view360.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (_interactAction != null)
            _interactAction.performed -= OnInteract;
    }

    void Update()
    {
        if (!_sessionActive || _doorUnlocked) return;

        _timeRemaining -= Time.deltaTime;

        if (_timeRemaining <= 0f)
            EndSession(success: false);
    }

    // ================================================================ Input
    void OnInteract(InputAction.CallbackContext ctx)
    {
        if (_doorUnlocked) return;      // puerta ya abierta, no hace falta volver
        if (_sessionActive) return;     // ya dentro

        float dist = Vector3.Distance(_playerTransform.position, transform.position);
        if (dist > interactRadius) return;

        StartSession();
    }

    // ================================================================ Session control
    void StartSession()
    {
        _sessionActive = true;
        _timeRemaining = timeLimit;
        _hitsLeft      = requiredHits;

        // Reactiva todos los objetivos (permite reintentar)
        foreach (var t in targets)
        {
            if (t != null) t.SetActive(true);
        }

        // Bloquea solo las acciones de movimiento; Shoot/Exit360/Interact siguen activos
        _moveAction.Disable();
        _jumpAction.Disable();
        _grabAction?.Disable();
        _viewMap.Disable();

        // Activa camara 360
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
        view360.gameObject.SetActive(true);
    }

    void EndSession(bool success)
    {
        _sessionActive = false;

        // Restaura controles del jugador
        _moveAction.Enable();
        _jumpAction.Enable();
        _grabAction?.Enable();
        _viewMap.Enable();

        view360.gameObject.SetActive(false);
        if (playerCamera != null) playerCamera.gameObject.SetActive(true);

        if (success)
        {
            _doorUnlocked = true;
            door?.Open();
            Debug.Log("[ObservationStation] Reto superado! Puerta abierta.");
        }
        else
        {
            Debug.Log("[ObservationStation] Tiempo agotado. Puedes volver a intentarlo.");
        }
    }

    // ================================================================ Llamado por ObservationView360
    /// <summary>Registra un impacto sobre un objetivo destruible.</summary>
    public void RegisterHit(GameObject target)
    {
        if (!_sessionActive || _doorUnlocked) return;
        if (target == null || !target.activeInHierarchy) return;

        target.SetActive(false);    // "destruye" visualmente sin Destroy, permite reintentar
        _hitsLeft--;

        if (_hitsLeft <= 0)
            EndSession(success: true);
    }

    /// <summary>El jugador pide salir manualmente (Escape).</summary>
    public void RequestExit()
    {
        if (!_sessionActive) return;
        EndSession(success: false);
    }

    // ================================================================ Gizmos de ayuda
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
