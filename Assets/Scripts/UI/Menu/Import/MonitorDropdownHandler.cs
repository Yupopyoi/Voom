using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonitorDropdownHandler : TMPDropdownHandlerBase
{
    [SerializeField] private UnityEvent onChanged;

    const int MAX_MONITOR_NUM = 3;
    int _monitorIndex = 1;

    protected override void Awake()
    {
        base.Awake();

        var labels = new List<string>();
        for(int monitorIndex = 1; monitorIndex <= MAX_MONITOR_NUM; monitorIndex++)
        {
            labels.Add(monitorIndex.ToString());
        }

        SetOptions(labels);
        SetInitialIndex();
    }

    protected override void OnChangedValue(int index)
    {
        onChanged?.Invoke();
    }

    public int GetMonitorIndex()
    {
        return _monitorIndex;
    }
}
