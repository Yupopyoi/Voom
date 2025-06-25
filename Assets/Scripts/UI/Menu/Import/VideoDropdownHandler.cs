using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VideoDropdownHandler : TMPDropdownHandlerBase
{
    [SerializeField] private UnityEvent onChanged;

    private WebCamDevice[] _videoDevices;

    int _monitorIndex = 1;

    protected override void Awake()
    {
        base.Awake();
        
        LoadDevices();
        var labels = new List<string>();
        foreach (var device in _videoDevices)
        {
            labels.Add(device.name);
        }

        SetOptions(labels);
        SetInitialIndex();
    }

    public void LoadDevices()
    {
        _videoDevices = WebCamTexture.devices;
    }

    protected override void OnChangedValue(int index)
    {
        _monitorIndex = index;
        onChanged?.Invoke();
    }

    public int GetMonitorIndex()
    {
        return _monitorIndex;
    }
}
