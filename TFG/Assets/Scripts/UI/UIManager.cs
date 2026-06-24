using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public Transform codePanel;
    public GameObject textPrefab;
    public TMP_Text taskText;
    public TMP_Text consoleText;

    private List<TMP_Text> uiLines = new List<TMP_Text>();
    private GameManager gameManager;

    private void Awake() => gameManager = GameManager.Instance;

    private void OnEnable() {
        gameManager.OnCodeLoaded += SetupCodeUI;
        gameManager.OnLineChanged += HighlightLine;
        gameManager.OnTaskUpdated += UpdateTaskPrompt;
        gameManager.OnOutputGenerated += UpdateConsole;
    }

    private void OnDisable() {
        gameManager.OnCodeLoaded -= SetupCodeUI;
        gameManager.OnLineChanged -= HighlightLine;
        gameManager.OnTaskUpdated -= UpdateTaskPrompt;
        gameManager.OnOutputGenerated -= UpdateConsole;
    }

    private void SetupCodeUI(List<string> lines) {
        foreach (Transform child in codePanel) Destroy(child.gameObject);
        uiLines.Clear();
        foreach (string line in lines) {
            TMP_Text t = Instantiate(textPrefab, codePanel).GetComponent<TMP_Text>();
            t.text = line;
            uiLines.Add(t);
        }
    }

    private void HighlightLine(int index) {
        for (int i = 0; i < uiLines.Count; i++)
            uiLines[i].color = (i == index) ? Color.red : Color.black;
    }

    private void UpdateTaskPrompt(string msg) => taskText.text = msg;
    private void UpdateConsole(string msg) => consoleText.text += "\n> " + msg;
}
