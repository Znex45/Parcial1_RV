using UnityEngine;

public class ControlUIManager : MonoBehaviour
{
    public GameObject keyboardUI;
    public GameObject gamepadUI;

    void Update()
    {
        if (InputDeviceDetector.usingGamepad)
        {
            keyboardUI.SetActive(false);
            gamepadUI.SetActive(true);
        }
        else
        {
            keyboardUI.SetActive(true);
            gamepadUI.SetActive(false);
        }
    }
}