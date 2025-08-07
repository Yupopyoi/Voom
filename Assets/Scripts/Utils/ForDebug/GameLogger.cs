// Copyright (c) 2025 Yupopyoi
//
// Use of this source code is governed by an MIT-style
// license that can be found in the LICENSE file or at
// https://opensource.org/licenses/MIT.

using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public static class LogUtils
{ 
    public static string TimeStamp(string format = "HH-mm-ss") /* "yyyy-MM-dd_HH-mm-ss.fff" */
    {
        return System.DateTime.Now.ToString(format);
    }

    public static string ClassName(int skipFrames = 2)
    {
        var stackTrace = new StackTrace();
        var frame = stackTrace.GetFrame(skipFrames);
        var method = frame?.GetMethod();
        var className = method?.DeclaringType?.Name;
        return className ?? "Unknown";
    }

}

// Instead of UnityEngine.Debug.Log(), display the log in the Game view.
// Basically, using Log() is fine.
// Numerous overloads are available, allowing you to display values of many types.
// No need for ToString(), which is a hassle to write every time.
// For those who are exhausted, you can show a cat. Please change _appendCat to true.

// [Overloads]
// Log(string message)
// Log(float value)
// Log(float value, float value)
// Log(float value, float value, float value)
// Log(bool b)
// Log(Vector2 logv)
// Log(Vector3 logv)
// Log(Vector4 logv)
// Log(Quaternion logq, [bool _displayAsQuaternion = false])
// Log(Vector3 logv1, Vector3 logv2) : The difference and inner product of two vectors are also displayed.

public static class GameLogger
{
    private readonly static List<TextMeshProUGUI> logTargets = new();
    private readonly static Queue<string> logQueue = new();
    private const int maxLines = 1;

    public static bool _appendCat = false;
    private readonly static string _catAA = "( =^-É÷-^=) < ";

    public static void Log(string message)
    {
        if (_appendCat) message = _catAA + message;

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

    public static void Log(Vector3 logv, int digits = 2, string prefix = "")
    {
        Vector3Log(logv, digits, prefix);
    }

    public static void Log(Vector3 logv1, Vector3 logv2)
    {
        TwoPointsLog(logv1, logv2);
    }

    public static void Log(Quaternion logq, bool _displayAsQuaternion = false, int digits = 2, string prefix = "")
    {
        if(_displayAsQuaternion)
        {
            string message = prefix;
            message += digits switch
            {
                1 => $"x : {logq.x:F1}, y : {logq.y:F1}, z : {logq.z:F1}, w : {logq.w:F1}",
                2 => $"x : {logq.x:F2} , y :  {logq.y:F2}, z : {logq.z:F2}, w : {logq.w:F2}",
                3 => $"x : {logq.x:F3} , y :  {logq.y:F3}, z : {logq.z:F3}, w : {logq.w:F3}",
                _ => $"x : {logq.x:F2} , y :  {logq.y:F2}, z : {logq.z:F2}, w : {logq.w:F2}",
            };
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
        message += digits switch
        {
            1 => $"x : {logv.x:F1}, y : {logv.y:F1}",
            2 => $"x : {logv.x:F2}, y : {logv.y:F2}",
            3 => $"x : {logv.x:F3}, y : {logv.y:F3}",
            _ => $"x : {logv.x:F2}, y : {logv.y:F2}",
        };
        Log(message);
    }

    public static void Vector3Log(Vector3 logv, int digits = 2, string prefix = "")
    {
        string message = prefix;
        message += digits switch
        {
            1 => $"x : {logv.x:F1}, y : {logv.y:F1}, z : {logv.z:F1}",
            2 => $"x : {logv.x:F2}, y : {logv.y:F2}, z : {logv.z:F2}",
            3 => $"x : {logv.x:F3}, y : {logv.y:F3}, z : {logv.z:F3}",
            _ => $"x : {logv.x:F2}, y : {logv.y:F2}, z : {logv.z:F2}",
        };
        Log(message);
    }

    public static void Vector3Log(Vector3 logv, float f, int digits = 2)
    {
        string message = digits switch
        {
            1 => $"x : {logv.x:F1}, y : {logv.y:F1}, z : {logv.z:F1} / F : {f:F1}",
            2 => $"x : {logv.x:F2}, y : {logv.y:F2}, z : {logv.z:F2} / F : {f:F2}",
            3 => $"x : {logv.x:F3}, y : {logv.y:F3}, z : {logv.z:F3} / F : {f:F3}",
            _ => $"x : {logv.x:F2}, y : {logv.y:F2}, z : {logv.z:F2} / F : {f:F2}",
        };
        Log(message);
    }

    public static void Vector4Log(Vector4 logv, int digits = 2)
    {
        string message = digits switch
        {
            1 => $"x1 : {logv.x:F1}, y1 : {logv.y:F1}, x2 : {logv.z:F1}, y2 : {logv.w:F1}",
            2 => $"x1 : {logv.x:F2}, y1 : {logv.y:F2}, x2 : {logv.z:F2}, y2 : {logv.w:F2}",
            3 => $"x1 : {logv.x:F3}, y1 : {logv.y:F3}, x2 : {logv.z:F3}, y2 : {logv.w:F3}",
            _ => $"x1 : {logv.x:F2}, y1 : {logv.y:F2}, x2 : {logv.z:F2}, y2 : {logv.w:F2}",
        };
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

    public static void Time(string format = "HH-mm-ss")
    {
        Log(LogUtils.TimeStamp(format));
    }

    // Extremely useful function
    public static void Cat(string whatToTalkAbout = "Nyaan")
    {
        Log(_catAA + whatToTalkAbout); // ( =^-É÷-^=) < Nyaan
    }

    #region Not Log Function

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


    private static void RefreshTargets()
    {
        string combined = string.Join("\n", logQueue);
        foreach (var target in logTargets)
        {
            if (target != null)
                target.text = combined;
        }
    }

    #endregion
}
