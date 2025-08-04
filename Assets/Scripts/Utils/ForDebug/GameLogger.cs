using System.Collections.Generic;
using TMPro;
using UnityEngine;

public static class LogUtils
{ 
    public static string TimeStamp()
    {
        return System.DateTime.Now.ToString("HH-mm-ss");
    }
}

public static class GameLogger
{
    private static List<TextMeshProUGUI> logTargets = new();
    private static Queue<string> logQueue = new();
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
        string message = prefixMessage + $" : {value:F2}";
        Log(message);
    }

    public static void Log(float value1, float value2, string prefixMessage = "Floats")
    {
        string message = prefixMessage + $" : {value1:F2} , {value2:F2}";
        Log(message);
    }

    public static void Log(float value1, float value2, float value3, string prefixMessage = "Floats")
    {
        string message = prefixMessage + $" : {value1:F2} , {value2:F2} , {value3:F2}";
        Log(message);
    }

    public static void Log(bool b, string prefixMessage = "Bool")
    {
        string message = prefixMessage + $" : {(b ? "True" : "False")}";
        Log(message);
    }

    public static void Log(Vector2 logv, int digits = 2)
    {
        Vector2Log(logv, digits);
    }

    public static void Log(Vector3 logv, int digits = 2)
    {
        Vector3Log(logv, digits);
    }

    public static void Log(Vector3 logv1, Vector3 logv2)
    {
        TwoPointsLog(logv1, logv2);
    }

    public static void Log(Quaternion logq, int digits = 2, bool _displayAsQuaternion = false, string prefix = "")
    {
        if(_displayAsQuaternion)
        {
            string message = prefix;
            switch (digits)
            {
                case 1:
                    message += $"x : {logq.x:F1}, y : {logq.y:F1}, z : {logq.z:F1}, w : {logq.w:F1}";
                    break;
                case 2:
                    message += $"x : {logq.x:F2} , y :  {logq.y:F2}, z : {logq.z:F2}, w : {logq.w:F2}";
                    break;
                case 3:
                    message += $"x : {logq.x:F3} , y :  {logq.y:F3}, z : {logq.z:F3}, w : {logq.w:F3}";
                    break;
                default:
                    message += $"x : {logq.x:F2} , y :  {logq.y:F2}, z : {logq.z:F2}, w : {logq.w:F2}";
                    break;
            }
            Log(message);
        }
        else
        {
            Vector3Log(logq.eulerAngles, digits, prefix:prefix);
        }
    }

    public static void Log(Vector3 logv, float f, int digits = 2)
    {
        Vector3Log(logv, f, digits);
    }

    public static void Log(Quaternion logq, float f, int digits = 2)
    {
        Vector3Log(logq.eulerAngles, f, digits);
    }

    public static void Log(Vector4 logv, int digits = 2)
    {
        Vector4Log(logv, digits);
    }

    public static void Vector2Log(Vector2 logv, int digits = 2, string prefix = "")
    {
        string message = prefix;
        switch (digits)
        {
            case 1:
                message += $"x : {logv.x:F1}, y : {logv.y:F1}";
                break;
            case 2:
                message += $"x : {logv.x:F2}, y : {logv.y:F2}";
                break;
            case 3:
                message += $"x : {logv.x:F3}, y : {logv.y:F3}";
                break;
            default:
                message += $"x : {logv.x:F2}, y : {logv.y:F2}";
                break;
        }

        Log(message);
    }

    public static void Vector3Log(Vector3 logv, int digits = 2, string prefix = "")
    {
        string message = prefix;
        switch (digits)
        {
            case 1:
                message += $"x : {logv.x:F1}, y : {logv.y:F1}, z : {logv.z:F1}";
                break;
            case 2:
                message += $"x : {logv.x:F2}, y : {logv.y:F2}, z : {logv.z:F2}";
                break;
            case 3:
                message += $"x : {logv.x:F3}, y : {logv.y:F3}, z : {logv.z:F3}";
                break;
            default:
                message += $"x : {logv.x:F2}, y : {logv.y:F2}, z : {logv.z:F2}";
                break;
        }

        Log(message);
    }

    public static void Vector3Log(Vector3 logv, float f, int digits = 2)
    {
        string message;
        switch (digits)
        {
            case 1:
                message = $"x : {logv.x:F1}, y : {logv.y:F1}, z : {logv.z:F1} / F : {f:F1}";
                break;
            case 2:
                message = $"x : {logv.x:F2}, y : {logv.y:F2}, z : {logv.z:F2} / F : {f:F2}";
                break;
            case 3:
                message = $"x : {logv.x:F3}, y : {logv.y:F3}, z : {logv.z:F3} / F : {f:F3}";
                break;
            default:
                message = $"x : {logv.x:F2}, y : {logv.y:F2}, z : {logv.z:F2} / F : {f:F2}";
                break;
        }

        Log(message);
    }

    public static void Vector4Log(Vector4 logv, int digits = 2)
    {
        string message;
        switch (digits)
        {
            case 1:
                message = $"x1 : {logv.x:F1}, y1 : {logv.y:F1}, x2 : {logv.z:F1}, y2 : {logv.w:F1}";
                break;
            case 2:
                message = $"x1 : {logv.x:F2}, y1 : {logv.y:F2}, x2 : {logv.z:F2}, y2 : {logv.w:F2}";
                break;
            case 3:
                message = $"x1 : {logv.x:F3}, y1 : {logv.y:F3}, x2 : {logv.z:F3}, y2 : {logv.w:F3}";
                break;
            default:
                message = $"x1 : {logv.x:F2}, y1 : {logv.y:F2}, x2 : {logv.z:F2}, y2 : {logv.w:F2}";
                break;
        }

        Log(message);
    }

    public static void TwoPointsLog(Vector3 point1, Vector3 point2)
    {
        string message = $"x1 : {point1.x:F2}, y1 : {point1.y:F2}, z1 : {point1.z:F2}\n" +
                         $"x2 : {point2.x:F2}, y2 : {point2.y:F2}, z2 : {point2.z:F2}\n" +
                         $"dx : {(point2-point1).x:F2}, dy : {(point2 - point1).y:F2}, dz : {(point2 - point1).z:F2}\n" + 
                         $"Dot : {Vector3.Dot(point1.normalized, point2.normalized):F2}";
        Log(message);
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
