using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerText;

    private float elapsed;
    private bool running;

    public float Elapsed => elapsed;
    public string FormattedTime => FormatTime(elapsed);

    private void Start()
    {
        elapsed = 0f;
        running = true;
        UpdateLabel();

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnProgramFinished += StopTimer;
            GameManager.Instance.OnActionFailed += AddTime;
            
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnProgramFinished -= StopTimer;
    }

    private void Update()
    {
        if (!running) return;
        elapsed += Time.deltaTime;
        UpdateLabel();
    }

    public void StopTimer() => running = false;
    public void AddTime()
    {
        elapsed += 3f;
        UpdateLabel();
    }

    public void ResetTimer()
    {
        elapsed = 0f;
        running = true;
        UpdateLabel();
    }

    private void UpdateLabel()
    {
        if (timerText == null) return;
        timerText.text = FormatTime(elapsed);
    }

    private string FormatTime(float t)
    {
        int minutes = Mathf.FloorToInt(t / 60f);
        int seconds = Mathf.FloorToInt(t % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}
