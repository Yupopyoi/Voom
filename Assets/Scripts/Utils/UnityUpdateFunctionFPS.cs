using System.Collections.Generic;
using UnityEngine;

public static class FPSHolder
{
    static private float _unityUpdateFPS = 1.0f;
    static private float _mediaPipeFPS = 1.0f;

    static public float UnityUpdateFPS
    {
        get { return _unityUpdateFPS; }
        set
        {
            if (value < 0) return;
            _unityUpdateFPS = value;
        }
    }

    static public float MediaPipeFPS
    {
        get { return _mediaPipeFPS; }
        set
        {
            if (value < 0) return;
            _mediaPipeFPS = value;
        }
    }

    static public float FPSRatio
    {
        get { return _unityUpdateFPS / _mediaPipeFPS; } // This should be a value greater than 1.
    }
}

public class UnityUpdateFunctionFPS : MonoBehaviour
{
    static private readonly Queue<float> _deltaTimeQueue = new();
    private const int Interval = 10;

    static private int _frameCounter = 0;

    private void Update()
    {
        _deltaTimeQueue.Enqueue(Time.deltaTime);

        if (_deltaTimeQueue.Count > Interval)
        {
            _deltaTimeQueue.Dequeue();
        }

        if (_frameCounter++ < Interval) return;

        float sum = 0.0f;
        foreach (var dt in _deltaTimeQueue)
        {
            sum += dt;
        }

        float averageDeltaTime = sum / _deltaTimeQueue.Count;
        FPSHolder.UnityUpdateFPS = 1f / averageDeltaTime;

        _frameCounter = 0;
    }
}
