using UnityEngine;
using System.Collections.Generic;

using Mediapipe.UnityRunner.PoseLandmarkDetection;
using Mediapipe.UnityRunner.FaceLandmarkDetection;

public class CameraDropdownHandler : TMPDropdownHandlerBase
{
    [Header("Avoke")]
    [SerializeField] private PoseLandmarkerRunner poseRunner;
    [SerializeField] private FaceLandmarkerRunner faceRunner;

    protected override void Awake()
    {
        base.Awake();

        var devices = WebCamTexture.devices;
        var labels = new List<string>();
        foreach (var d in devices)
        {
            labels.Add(d.name);
        }

        SetOptions(labels);
        SetInitialIndex();
    }

    protected override void OnChangedValue(int index)
    {
#pragma warning disable UNT0008 // Null propagation on Unity objects
        poseRunner?.OnChangedCameraDevice(index);
        faceRunner?.OnChangedCameraDevice(index);
#pragma warning restore UNT0008 // Null propagation on Unity objects
        Debug.Log("Changed Camera : " + index);
    }
}
