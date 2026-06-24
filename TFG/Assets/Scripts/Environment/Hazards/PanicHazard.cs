using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class PanicHazard : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Si actiu, el pànic s'activa sol periòdicament. Si no, només via TriggerPanic() (p. ex. rates).")]
    [SerializeField, Min(1f)] private float timeLimit = 5f;

    [Header("Tecles")]
    [SerializeField] private KeyCode[] possibleKeys =
    {
        KeyCode.Q, KeyCode.E, KeyCode.R, KeyCode.F,
        KeyCode.G, KeyCode.Z, KeyCode.X, KeyCode.C
    };

    [Header("Penalització per Fallo")]
    [SerializeField] private PenaltyType penaltyType = PenaltyType.SlowDown;
    [SerializeField, Range(0f, 1f)] private float slowMultiplier = 0.3f;
    [SerializeField, Min(0f)] private float penaltyDuration = 4f;

    [Header("UI")]
    [SerializeField] private GameObject panicPanel;
    [SerializeField] private Image redFlashOverlay;
    [SerializeField] private TextMeshProUGUI keyPromptText;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private TextMeshProUGUI resultText;

    [Header("Parpelleig")]
    [SerializeField, Min(0.5f)] private float flashFrequency = 2.5f;
    [SerializeField, Range(0f, 1f)] private float minFlashAlpha = 0.08f;
    [SerializeField, Range(0f, 1f)] private float maxFlashAlpha = 0.45f;

    private enum PenaltyType { SlowDown, ReverseControls, Both }

    private bool isPanicking;
    private KeyCode currentKey;
    private Coroutine penaltyRoutine;
    private PlayerController player;

    private const string PenaltySource = nameof(PanicHazard);

    private void Start()
    {
        gameObject.SetActive(true);
        player = FindAnyObjectByType<PlayerController>();
    }

    public void TriggerPanic()
    {
        if (player != null && !isPanicking)
            StartCoroutine(PanicSequence());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        CloseUI();

        if (player != null)
            SetPenalty(player, apply: false);

        isPanicking = false;
    }

    private IEnumerator TriggerLoop()
    {
        while (true)
        {
            if (player != null && !isPanicking)
                yield return StartCoroutine(PanicSequence());
        }
    }

    private IEnumerator PanicSequence()
    {
        isPanicking = true;
        currentKey = possibleKeys[Random.Range(0, possibleKeys.Length)];
        float elapsed = 0f;

        OpenUI();

        while (elapsed < timeLimit)
        {
            if (Input.GetKeyDown(currentKey))
            {
                yield return StartCoroutine(ShowResult(success: true));
                isPanicking = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            RefreshCountdown(timeLimit - elapsed);
            RefreshFlash(elapsed);
            yield return null;
        }

        yield return StartCoroutine(ShowResult(success: false));
        BeginPenalty(player);
        isPanicking = false;
    }

    private IEnumerator ShowResult(bool success)
    {
        if (resultText != null)
        {
            resultText.text = success ? "SUCCESS" : "FAILURE";
            resultText.color = success ? Color.green : Color.red;
        }

        yield return new WaitForSeconds(0.75f);
        CloseUI();
    }

    private void BeginPenalty(PlayerController p)
    {
        if (penaltyRoutine != null) StopCoroutine(penaltyRoutine);
        penaltyRoutine = StartCoroutine(PenaltyRoutine(p));
    }

    private IEnumerator PenaltyRoutine(PlayerController p)
    {
        SetPenalty(p, apply: true);
        yield return new WaitForSeconds(penaltyDuration);
        SetPenalty(p, apply: false);
        penaltyRoutine = null;
    }

    private void SetPenalty(PlayerController p, bool apply)
    {
        if (p == null) return;
        switch (penaltyType)
        {
            case PenaltyType.SlowDown:
                if (apply) p.SetMoveSpeedMultiplier(PenaltySource, slowMultiplier);
                else       p.ClearMoveSpeedMultiplier(PenaltySource);
                break;
            case PenaltyType.ReverseControls:
                if (apply) p.SetControlsReversed(PenaltySource, true);
                else       p.ClearControlsReversal(PenaltySource);
                break;
            case PenaltyType.Both:
                if (apply) { p.SetMoveSpeedMultiplier(PenaltySource, slowMultiplier); p.SetControlsReversed(PenaltySource, true); }
                else       { p.ClearMoveSpeedMultiplier(PenaltySource); p.ClearControlsReversal(PenaltySource); }
                break;
        }
    }

    private void OpenUI()
    {
        if (panicPanel != null) panicPanel.SetActive(true);
        AudioManager.Play(l => l.panic);
        if (resultText != null) resultText.text = string.Empty;
        if (keyPromptText != null) keyPromptText.text = "PRESS " + currentKey.ToString().ToUpper();
        RefreshCountdown(timeLimit);
        SetOverlayAlpha(minFlashAlpha);
    }

    private void CloseUI()
    {
        if (panicPanel != null) panicPanel.SetActive(false);
        AudioManager.Stop(l => l.panic);
        SetOverlayAlpha(0f);
    }

    private void RefreshCountdown(float remaining)
    {
        if (countdownText != null)
            countdownText.text = Mathf.Max(0f, remaining).ToString("F1");
    }

    private void RefreshFlash(float elapsed)
    {
        float t = (Mathf.Sin(elapsed * flashFrequency * Mathf.PI * 2f) + 1f) * 0.5f;
        SetOverlayAlpha(Mathf.Lerp(minFlashAlpha, maxFlashAlpha, t));
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (redFlashOverlay == null) return;
        Color c = redFlashOverlay.color;
        c.a = alpha;
        redFlashOverlay.color = c;
    }
}
