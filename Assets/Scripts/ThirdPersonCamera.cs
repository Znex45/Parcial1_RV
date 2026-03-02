using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    public float distance = 4f;
    public float sensitivity = 100f;
    public float yMin = -30f;
    public float yMax = 70f;

    Transform target;
    float xRot, yRot;

    InputAction lookAction;
    InputActionMap viewMap;

    void Awake()
    {
        target = transform.root;

        var pi = GetComponentInParent<PlayerInput>();

        viewMap = pi.actions.FindActionMap("View", true);      // true = throw si no existe
        lookAction = viewMap.FindAction("Look", true);

        viewMap.Enable(); // <-- CLAVE: si el current map es PlayerMove, View no esta activo
    }

    void LateUpdate()
    {
        Vector2 look = lookAction.ReadValue<Vector2>();

        yRot += look.x * sensitivity * Time.deltaTime;
        xRot -= look.y * sensitivity * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, yMin, yMax);

        Quaternion rot = Quaternion.Euler(xRot, yRot, 0f);
        transform.position = target.position - rot * Vector3.forward * distance;
        transform.rotation = rot;
    }
}