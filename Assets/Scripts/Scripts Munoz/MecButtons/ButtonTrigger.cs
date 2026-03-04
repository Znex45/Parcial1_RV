using UnityEngine;
using UnityEngine.Events;


public class ButtonTrigger : MonoBehaviour
{
    public UnityEvent onActivated;

    private bool isPressed = false;

    public void Activate()
    {
        if (isPressed) return;

        isPressed = true;
        Debug.Log("Botón activado");

        onActivated?.Invoke();
    }
}