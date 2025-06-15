// ScreenCapturePanel.cs - CapturePanel-derived class that captures a specific monitor using DDCapture.dll
using Capture;
using System.Runtime.InteropServices;
using System;
using UnityEngine;
using UnityEngine.UI;

public class ScreenCapturePanel : CapturePanel
{
    [DllImport("DDCapture.dll")]
    private static extern bool StartCapture(int monitorIndex);

    [DllImport("DDCapture.dll")]
    private static extern void StopCapture();

    [DllImport("DDCapture.dll")]
    private static extern IntPtr GetSharedBuffer();

    [DllImport("DDCapture.dll")]
    private static extern int GetWidth();

    [DllImport("DDCapture.dll")]
    private static extern int GetHeight();

    [DllImport("DDCapture.dll")]
    private static extern float SetWide(float wide);

    private Texture2D tex;
    private int width;
    private int height;
    private int monitorIndex = 0;

    private bool captureStarted = false;

    private int[] _aspectRatio;

    new void Start()
    {
        base.Start();
    }

    protected override void StartCapture()
    {
        Stop();

        SetWideRatio(1.5f);

        if (!StartCapture(monitorIndex))
        {
            Debug.LogError("[ScreenCapturePanel] Failed to start capture");
            return;
        }
        width = GetWidth();
        height = GetHeight();

        tex = new Texture2D(width, height, TextureFormat.BGRA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        if (imageTarget != null)
        {
            imageTarget.rectTransform.localScale = new Vector3(1.0f, -1.0f, 1.0f);
            imageTarget.rectTransform.sizeDelta = new Vector2(width, height);
            imageTarget.texture = tex;
        }

        _aspectRatio = CalculateAspectRatio(width, height);
        ResizeToFitRawImage();

        captureStarted = true;
    }

    protected override void Stop()
    {
        StopCapture();
        captureStarted = false;
    }

    new void Update()
    {
        base.Update();

        if (!captureStarted || tex == null) return;
        IntPtr buffer = GetSharedBuffer();
        if (buffer == IntPtr.Zero) return;

        tex.LoadRawTextureData(buffer, width * height * 4 * _aspectRatio[0] / _aspectRatio[1]);
        tex.Apply(false);
    }

    void OnDestroy()
    {
        StopCapture();
    }
    public void StartMonitorCapture()
    {
        StartCapture();
    }

    public void SetMonitorIndex(int index)
    {
        monitorIndex = index;
    }

    public void SetWideRatio(float wide)
    {
        SetWide(wide);
    }
}
