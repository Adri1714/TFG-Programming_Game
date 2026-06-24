using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LevelCompleteMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject completePanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text timeText;

    [Header("Referencies")]
    [SerializeField] private GameTimer gameTimer;
    [Tooltip("Boto de seguent nivell. S'amaga automaticament a l'ultim nivell.")]
    [SerializeField] private GameObject nextLevelButton;

    [Header("Escenes")]
    [SerializeField] private string gameplaySceneName = "Version3";
    [SerializeField] private string menuSceneName = "Menu";

    [Tooltip("Segons d'espera abans de mostrar el panell, perquè es vegi l'últim print.")]
    [SerializeField] private float endDelay = 1.5f;

    private bool shown;

    private void Start()
    {
        if (completePanel != null) completePanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.OnProgramFinished += ShowComplete;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnProgramFinished -= ShowComplete;
    }

    private void ShowComplete()
    {
        if (shown) return;
        shown = true;

        if (gameTimer != null) gameTimer.StopTimer();

        StartCoroutine(ShowAfterDelay());
    }

    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSeconds(endDelay);
        Time.timeScale = 0f;
        if (titleText != null) titleText.text = "LEVEL COMPLETED!";
        if (timeText != null)
            timeText.text = "Time: " + (gameTimer != null ? gameTimer.FormattedTime : "--:--");

        if (nextLevelButton != null)
            nextLevelButton.SetActive(GameSession.HasNextLevel);

        if (completePanel != null) completePanel.SetActive(true);
    }

    public void NextLevel()
    {
        if (!GameSession.HasNextLevel) return;
        GameSession.GoToNextLevel();
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

}