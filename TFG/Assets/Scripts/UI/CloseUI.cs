using UnityEngine;

public class CloseUI : MonoBehaviour
{
    [SerializeField] private GameObject uiPanel;

    private InputEvents inputEvents;

    private void Start()
    {
        uiPanel = uiPanel != null ? uiPanel : gameObject;
        inputEvents = InputEvents.Instance;
        inputEvents.OnToggle += CloseUIPanel;
    }

    private void CloseUIPanel()
    {
        if (uiPanel != null && uiPanel.activeSelf)
        {
            uiPanel.SetActive(false);
        }
    }
}