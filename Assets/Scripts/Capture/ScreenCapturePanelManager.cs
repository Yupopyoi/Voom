using UnityEngine;

public class ScreenCapturePanelManager : CapturePanelManagerBase
{
    // In the inspector,these can be used to set the initial value of the "Wide",
    // but it is not reflected in the initial value of the slider.
    // "Wide" represent a multiplier for the size of the area to be captured.
    // The screen size is automatically obtained in DDCapture.dll,
    // but it may differ from the actual screen size. This can be corrected by these.
    [SerializeField, Range(0.25f,2.5f)] private float _widthWide = 1.0f;
    [SerializeField, Range(0.25f,2.5f)] private float _heightWide = 1.0f;

    protected override System.Type PanelType => typeof(ScreenCapturePanel);

    public void SetWidthWideRatio(float widthWide)
    {
        _widthWide = widthWide;
    }

    public void SetHeightWideRatio(float heightWide)
    {
        _heightWide = heightWide;
    }

    protected override void AwakeCapture()
    {
        MonitorDropdownHandler monitorDropdownHandler = dropdownHandler as MonitorDropdownHandler;
        ScreenCapturePanel screenCapturePanel = capturePanel as ScreenCapturePanel;

        screenCapturePanel.Stop();
        screenCapturePanel.SetMonitorIndex(monitorDropdownHandler.GetMonitorIndex());
        screenCapturePanel.SetWideRatio(_widthWide, _heightWide);
        screenCapturePanel.StartMonitorCapture();
    }
}
