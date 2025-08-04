using NUnit.Framework;
using TMPro;
using Unity.Collections;
using UnityEngine;
using VRMController;

[RequireComponent(typeof(TextMeshProUGUI))]
public class VisibilityLogger : MonoBehaviour
{
    [SerializeField] VRMAllocator _vrmAllocator;
    [SerializeField] TextMeshProUGUI _visibilityText;

    void Update()
    {
        if (_vrmAllocator == null)
        {
            _visibilityText.text = "Allocator does not exist!";
            return;
        }

        var landmarks = _vrmAllocator.Landmarks();

        if (landmarks.Count == 0)
        {
            _visibilityText.text = "No model loaded!";
            return;
        }

        if(landmarks[14].visibility == null)
        {
            _visibilityText.text = "No model loaded!";
            return;
        }

        float leftWristVisibility = landmarks[15].visibility.Value;
        float rightWristVisibility = landmarks[14].visibility.Value;
        float leftAncleVisibility = landmarks[27].visibility.Value;
        float rightAncleVisibility = landmarks[28].visibility.Value;

        string colorHex = GetColorHexByValue(leftWristVisibility);
        string leftHandText  = $"<color={colorHex}>L Hand : {leftWristVisibility:F2}</color>\n";

        colorHex = GetColorHexByValue(rightWristVisibility);
        string rightHandText = $"<color={colorHex}>R Hand : {rightWristVisibility:F2}</color>\n";

        colorHex = GetColorHexByValue(leftAncleVisibility);
        string leftFootText  = $"<color={colorHex}>L Foot  : {leftAncleVisibility:F2}</color>\n";

        colorHex = GetColorHexByValue(rightAncleVisibility);
        string rightFootText = $"<color={colorHex}>R Foot  : {rightAncleVisibility:F2}</color>";

        _visibilityText.text = leftHandText + rightHandText + leftFootText + rightFootText;
    }

    private static string GetColorHexByValue(float value)
    {
        if (value >= 0.9f)
            return "#00FF00"; // Green
        else if (value >= 0.5f)
            return "#FFA500"; // Yellow
        else
            return "#FF0000"; // Red
    }
}
