using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    /// <summary>
    /// Carga una escena usando su nombre.
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogWarning("SceneLoader: El nombre de la escena está vacío.");
        }
    }

    public void errorButtonSound()
    {
        AudioManager.Instance.Play("error");
    }
}
