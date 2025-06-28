// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

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
