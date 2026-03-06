using UnityEngine;
using UnityEngine.InputSystem;

public class InputDeviceDetector : MonoBehaviour
{
    public static bool usingGamepad = false;

    void Update()
    {
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            usingGamepad = true;
        }

        if (Keyboard.current != null && Keyboard.current.wasUpdatedThisFrame)
        {
            usingGamepad = false;
        }
    }
}