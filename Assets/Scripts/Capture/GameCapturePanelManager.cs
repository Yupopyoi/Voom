using UnityEngine;

public class GameCapturePanelManager : CapturePanelManagerBase
{
    protected override System.Type PanelType => typeof(GameCapturePanel);

    protected override void AwakeCapture()
    {
        VideoDropdownHandler videoDropdownHandler = dropdownHandler as VideoDropdownHandler;
        GameCapturePanel gameCapturePanel = capturePanel as GameCapturePanel;

        gameCapturePanel.SetDeviceIndex(videoDropdownHandler.GetMonitorIndex());
        gameCapturePanel.StartGameCapture();
    }
}
