using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ScreenCaptureButton : MonoBehaviour
{
    bool _isScreenDisplaying = false;

    TextMeshProUGUI _buttonText;

    [SerializeField] private UnityEvent onDisplay;
    [SerializeField] private UnityEvent onDelete;

    private void Start()
    {
        _buttonText = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void OnClick()
    {
        _isScreenDisplaying = !_isScreenDisplaying;

        if(_isScreenDisplaying)
        {
            _buttonText.text = "Delete";
            onDisplay?.Invoke();
        }
        else /* is Screen Deleted */
        {
            _buttonText.text = "Display";
            onDelete?.Invoke();
        }
    }
}
