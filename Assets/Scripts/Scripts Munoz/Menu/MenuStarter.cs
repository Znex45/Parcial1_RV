using UnityEngine;
using UnityEngine.EventSystems;

public class MenuStarter : MonoBehaviour
{
    public GameObject firstButton;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(firstButton);
    }
}