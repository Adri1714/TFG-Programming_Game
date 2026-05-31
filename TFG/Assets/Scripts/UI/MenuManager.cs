using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Tooltip("Nom de l'escena de joc (ha d'estar a Build Settings).")]
    [SerializeField] private string gameplaySceneName = "Version3";

    public void PlayLevel(int levelNumber)
    {
        GameSession.SetLevel(levelNumber);
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
