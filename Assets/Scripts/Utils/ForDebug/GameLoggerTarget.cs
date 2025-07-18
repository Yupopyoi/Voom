using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class GameLoggerTarget : MonoBehaviour
{
    private TextMeshProUGUI textComponent;

    void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        textComponent.text = "";
        GameLogger.RegisterTarget(textComponent);
    }

    void OnDestroy()
    {
        GameLogger.UnregisterTarget(textComponent);
    }
}
