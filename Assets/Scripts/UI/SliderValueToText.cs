using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SliderUtils
{
    [RequireComponent(typeof(Slider))]
    public class SliderValueToText : MonoBehaviour
    {
        public enum Precision 
        {
            OneDecimal = 1,    // e.g.) 2.3 
            TwoDecimals = 2,   // e.g.) 2.31 
            ThreeDecimals = 3  // e.g.) 2.313
        }

        private Slider slider;
        [SerializeField] TextMeshProUGUI valueText;

        [Header("Display Format")]
        [SerializeField] private bool isValueInt = false;
        [SerializeField] private Precision precision = Precision.TwoDecimals;

        void Start()
        {
            slider = GetComponent<Slider>();

            if (valueText == null)
            {
                var valueTextObject = transform.Find("ValueText")?.gameObject;
                valueText = valueTextObject?.GetComponent<TextMeshProUGUI>();
            }

            UpdateText(slider.value);
        }

        // Attach as listeners of OnValueChanged or onSteppedValueChanged event.
        public void UpdateText(float value)
        {
            if (isValueInt)
            {
                valueText.text = value.ToString("0");
            }
            else /* Value is Float */
            {
                switch (precision)
                {
                    case Precision.OneDecimal:
                        valueText.text = value.ToString("0.0");
                        break;
                    case Precision.TwoDecimals:
                        valueText.text = value.ToString("0.00");
                        break;
                    case Precision.ThreeDecimals:
                        valueText.text = value.ToString("0.000");
                        break;
                }
            }
        }
    }
} // namespace SliderUtils