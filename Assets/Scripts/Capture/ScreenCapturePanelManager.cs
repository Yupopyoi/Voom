// CapturePanelManager.cs - Manages singleton instance of ScreenCapturePanel from UI
using UnityEngine;
using System.Collections;

public class ScreenCapturePanelManager : MonoBehaviour
{
    [SerializeField] GameObject screenCapturePanelPrefab;
    [SerializeField] Transform canvasTransform; // Set to your main Canvas transform in inspector

    [SerializeField] MonitorDropdownHandler _monitorDropdownHandler;

    private GameObject activePanel;

    public void OnAddScreenCapture()
    {
        if (activePanel != null) return;

        activePanel = Instantiate(screenCapturePanelPrefab, canvasTransform);
        StartCoroutine(StartPanelAfterInitialized(activePanel));
    }

    public void OnDeleteScreenCapture()
    {
        if (activePanel == null) return;
        Destroy(activePanel);
    }

    void Update()
    {
        if (activePanel == null) return;

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            Destroy(activePanel);
            activePanel = null;
        }
    }

    private IEnumerator StartPanelAfterInitialized(GameObject panel)
    {
        yield return null; // StartÇ™êÊÇ…åƒÇŒÇÍÇÈÇÊÇ§Ç…Ç∑ÇÈ

        var script = panel.GetComponent<ScreenCapturePanel>();
        if (script != null)
        {
            script.SetMonitorIndex(_monitorDropdownHandler.GetMonitorIndex());
            script.StartMonitorCapture();
        }
    }
}
