using UnityEngine;
using UnityEngine.EventSystems;

public class MenuFirstButton : MonoBehaviour
{
    public GameObject firstButton;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstButton);
    }
}