using UnityEngine;

/// <summary>
/// Singleton. Guarda el punto de spawn del nivel.
/// Todos los jugadores lo consultan al morir.
/// </summary>
public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("Punto de spawn del nivel")]
    public Transform spawnPoint;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public Vector3 GetSpawnPosition()
    {
        if (spawnPoint != null) return spawnPoint.position;
        Debug.LogWarning("[RespawnManager] No hay spawnPoint asignado, usando Vector3.zero.");
        return Vector3.zero;
    }
}

