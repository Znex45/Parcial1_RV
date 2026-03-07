using UnityEngine;
using UnityEngine.SceneManagement; // Necesario para cambiar de escena

public class SceneChanger : MonoBehaviour
{
    public string nextSceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CambiarEscena();
        }
    }

    public void CambiarEscena()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("No has puesto el nombre de la escena en el Inspector.");
        }
    }
}