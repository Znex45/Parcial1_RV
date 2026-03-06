using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    public Transform player;
    public Transform cameraPivot;

    public float mouseSensitivity = 150f;
    public float controllerSensitivity = 200f;

    private Vector2 lookInput;

    float xRotation = 0f;
    float yRotation = 0f;

    void Start()
    {
        if (player == null)
            player = transform;

        if (cameraPivot == null)
            cameraPivot = transform.Find("CameraPivot");
    }

    void LateUpdate()
    {
        float sensitivity = mouseSensitivity;

        float mouseX = lookInput.x * sensitivity;
        float mouseY = lookInput.y * sensitivity;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -60f, 60f);

        player.rotation = Quaternion.Euler(0f, yRotation, 0f);
        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    public void OnView(InputValue value)
    {
        lookInput = value.Get<Vector2>();
        Debug.Log(lookInput);
    }
}