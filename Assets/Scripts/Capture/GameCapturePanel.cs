using Capture;
using UnityEngine;

public class GameCapturePanel : CapturePanel
{
    private WebCamTexture _webCamTexture;
    private WebCamDevice[] _videoDevices;

    private int _deviceIndex = 0;

    new void Start()
    {
        base.Start();
    }

    public void SetDeviceIndex(int index)
    {
        _deviceIndex = index;
    }

    private void LoadDevices()
    {
        _videoDevices = WebCamTexture.devices;
    }

    protected override void StartCapture()
    {
        LoadDevices();

        if (_videoDevices.Length <= _deviceIndex) return;

        _webCamTexture = new WebCamTexture(_videoDevices[_deviceIndex].name);
        imageTarget.texture = _webCamTexture;

        _webCamTexture.Play();
        ResizeToFitRawImage();
    }

    public void StartGameCapture()
    {
        StartCapture();
    }

    public override void Stop()
    {
        if (_webCamTexture != null)
        {
            _webCamTexture.Stop();
        }
    }

    void OnDestroy()
    {
        Stop();
    }
}
