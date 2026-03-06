using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject instructionsPanel;
    public GameObject rulesPanel;

    [Header("First Selected Buttons")]
    public GameObject mainFirstButton;
    public GameObject instructionsFirstButton;
    public GameObject rulesFirstButton;

    [Header("Game Scene")]
    public int gameSceneIndex = 1;

    void Start()
    {
        EventSystem.current.SetSelectedGameObject(mainFirstButton);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneIndex);
    }

    public void OpenInstructions()
    {
        mainPanel.SetActive(false);
        instructionsPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(instructionsFirstButton);
    }

    public void OpenRules()
    {
        instructionsPanel.SetActive(false);
        rulesPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(rulesFirstButton);
    }

    public void BackToMain()
    {
        instructionsPanel.SetActive(false);
        rulesPanel.SetActive(false);
        mainPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(mainFirstButton);
    }
}