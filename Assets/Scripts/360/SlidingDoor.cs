using UnityEngine;

/// <summary>
/// Desliza la puerta aplicando un offset local al Transform cuando se desbloquea.
/// Arrastra este componente sobre el GameObject de la puerta.
/// </summary>
public class SlidingDoor : MonoBehaviour
{
    [Header("Slide Settings")]
    [Tooltip("Offset en espacio local que define la posicion abierta de la puerta.")]
    public Vector3 openOffset = new Vector3(0f, 3f, 0f);

    [Tooltip("Velocidad de desplazamiento (unidades/seg).")]
    [Min(0.01f)] public float slideSpeed = 2f;

    // ----- estado -----
    Vector3 _closedLocalPos;
    Vector3 _openLocalPos;
    bool    _isOpen;

    void Awake()
    {
        _closedLocalPos = transform.localPosition;
        _openLocalPos   = _closedLocalPos + openOffset;
    }

    void Update()
    {
        Vector3 target = _isOpen ? _openLocalPos : _closedLocalPos;
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition, target, slideSpeed * Time.deltaTime);
    }

    /// <summary>Abre la puerta deslizandola hacia el openOffset.</summary>
    public void Open()  => _isOpen = true;

    /// <summary>Cierra la puerta (opcional).</summary>
    public void Close() => _isOpen = false;

    public bool IsOpen => _isOpen;
}

