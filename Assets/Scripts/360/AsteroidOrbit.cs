using UnityEngine;

/// <summary>
/// Hace orbitar el objeto alrededor del centro de la sphere 360.
/// Asigna el centro (SphereCenter) y configura velocidad/inclinacion en el Inspector.
/// </summary>
public class AsteroidOrbit : MonoBehaviour
{
    [Tooltip("Transform del centro de la sphere (ObservationSphere).")]
    public Transform sphereCenter;

    [Tooltip("Velocidad orbital en grados/segundo.")]
    [Range(5f, 120f)] public float orbitSpeed = 20f;

    [Tooltip("Eje de orbita en espacio mundo. Varía por asteroide para distintas alturas/inclinaciones.")]
    public Vector3 orbitAxis = Vector3.up;

    void Update()
    {
        if (sphereCenter == null) return;

        transform.RotateAround(
            sphereCenter.position,
            orbitAxis.normalized,
            orbitSpeed * Time.deltaTime
        );
    }
}
