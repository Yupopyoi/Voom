using UnityEngine;
using System.Collections;

using Capture;

public abstract class CapturePanelManagerBase : MonoBehaviour
{
    [SerializeField] protected GameObject capturePanelPrefab;

    // Set to your main Canvas transform in inspector
    [SerializeField] protected Transform canvasTransform;

    [SerializeField] protected TMPDropdownHandlerBase dropdownHandler;

    protected GameObject activePanel;
    protected CapturePanel capturePanel;

    protected abstract System.Type PanelType { get; }

    public void OnAddScreenCapture()
    {
        if (activePanel != null) return;

        activePanel = Instantiate(capturePanelPrefab, canvasTransform);

        var component = activePanel.GetComponent(PanelType);
        if (component is CapturePanel panel)
        {
            capturePanel = panel;
            StartCoroutine(StartPanelAfterInitialized());
        }
        else
        {
            Debug.LogError("PanelType is not a subclass of CapturePanel.");
        }
    }

    public void OnDeleteScreenCapture()
    {
        DeletePanel();
    }

    public void OnFixPanel()
    {
        capturePanel.IsFixed = true;
    }

    public void OnUnfixPanel()
    {
        capturePanel.IsFixed = false;
    }

    private IEnumerator StartPanelAfterInitialized()
    {
        yield return null; // Ensure that Start is called first.

        if (capturePanel != null)
        {
            AwakeCapture();
        }
    }

    protected abstract void AwakeCapture();

    // Update is called once per frame
    void Update()
    {
        if (capturePanel == null) return;

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            DeletePanel();
        }
    }

    protected void DeletePanel()
    {
        if (capturePanel == null) return;
        capturePanel.Stop();
        Destroy(activePanel);
        activePanel = null;
        capturePanel = null;
    }
}
