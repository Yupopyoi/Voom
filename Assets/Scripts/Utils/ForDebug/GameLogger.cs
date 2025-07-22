using Mediapipe.Allocator;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class GameLogger
{
    private static List<TextMeshProUGUI> logTargets = new List<TextMeshProUGUI>();
    private static Queue<string> logQueue = new Queue<string>();
    private const int maxLines = 1;

    public static void RegisterTarget(TextMeshProUGUI target)
    {
        if (target != null && !logTargets.Contains(target))
        {
            logTargets.Add(target);
            RefreshTargets();
        }
    }

    public static void UnregisterTarget(TextMeshProUGUI target)
    {
        if (target != null && logTargets.Contains(target))
        {
            logTargets.Remove(target);
        }
    }

    public static void Log(string message)
    {
        logQueue.Enqueue(message);
        if (logQueue.Count > maxLines)
            logQueue.Dequeue();

        RefreshTargets();
    }

    public static void Log(float value, string prefixMessage = "Float")
    {
        logQueue.Enqueue(prefixMessage + $" : {value:F2}");
        if (logQueue.Count > maxLines)
            logQueue.Dequeue();

        RefreshTargets();
    }

    public static void Log(float value1, float value2, string prefixMessage = "Float")
    {
        logQueue.Enqueue(prefixMessage + $" : {value1:F2} , {value2:F2}");
        if (logQueue.Count > maxLines)
            logQueue.Dequeue();

        RefreshTargets();
    }

    public static void Log(Vector3 logv, int digits = 2)
    {
        Vector3Log(logv, digits);
    }

    public static void Vector3Log(Vector3 logv, int digits = 2)
    {
        string message;
        switch (digits)
        {
            case 1:
                message = $"x : {logv.x:F1}, y : {logv.y:F1}, z : {logv.z:F1}";
                break;
            case 2:
                message = $"x : {logv.x:F2}, y : {logv.y:F2}, z : {logv.z:F2}";
                break;
            case 3:
                message = $"x : {logv.x:F3}, y : {logv.y:F3}, z : {logv.z:F3}";
                break;
            default:
                message = $"x : {logv.x:F2}, y : {logv.y:F2}, z : {logv.z:F2}";
                break;
        }

        logQueue.Enqueue(message);
        if (logQueue.Count > maxLines)
            logQueue.Dequeue();

        RefreshTargets();
    }

    public static void RotationLog(Rotation rot)
    {
        Vector3Log(rot.ToVector3);
    }

    public static void RotationLog(Rotation? rot)
    {
        Vector3Log(rot.GetValueOrDefault().ToVector3);
    }

    public static void TwoPointsLog(Vector3 point1, Vector3 point2)
    {
        string message = $"x1 : {point1.x:F2}, y1 : {point1.y:F2}, z1 : {point1.z:F2}\n" +
                         $"x2 : {point2.x:F2}, y2 : {point2.y:F2}, z2 : {point2.z:F2}\n" +
                         $"dx : {(point2-point1).x:F2}, dy : {(point2 - point1).y:F2}, dz : {(point2 - point1).z:F2}";
        logQueue.Enqueue(message);
        if (logQueue.Count > maxLines)
            logQueue.Dequeue();

        RefreshTargets();
    }

    private static void RefreshTargets()
    {
        string combined = string.Join("\n", logQueue);
        foreach (var target in logTargets)
        {
            if (target != null)
                target.text = combined;
        }
    }
}
