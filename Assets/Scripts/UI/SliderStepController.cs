using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SliderUtils
{
    [RequireComponent(typeof(Slider))]
    public class SliderStepController : MonoBehaviour
    {
        // Round the slider value to the specified step size (e.g., 0.25).
        // Notify other scripts of the rounded value via an event.

        // Avoid attaching listeners directly to the Slider's built-in OnValueChanged event,
        // as it provides raw values before rounding.
        // Instead, use the onSteppedValueChanged event in this script,
        // and set the UnityEvent to "Dynamic float" to receive the rounded value properly.

        [Header("Config")]
        [SerializeField] private bool _useStepController = true;
        [Tooltip("The increment size used when rounding the slider value")]
        [SerializeField] private float _step = 0.25f;

        [Header("Callback after stepped value")]
        [Tooltip("This event is invoked with the snapped (rounded) value after value change")]
        [SerializeField] private UnityEvent<float> _onSteppedValueChanged;

        private Slider _slider;
        private bool _internallySetting = false; // Prevent recursive calls when setting value internally

        void Start()
        {
            _slider = GetComponent<Slider>();
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        void OnSliderValueChanged(float value)
        {
            if (!_useStepController || _internallySetting) return;

            if (_step <= 0f) return;

            float stepped = Mathf.Round(value / _step) * _step;

            if (Mathf.Abs(value - stepped) > 0.0001f)
            {
                _internallySetting = true;
                _slider.value = stepped;
                _internallySetting = false;
            }
            _onSteppedValueChanged.Invoke(stepped);
        }
    }
} // namespace SliderUtils
