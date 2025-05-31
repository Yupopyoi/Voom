using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public abstract class TMPDropdownHandlerBase : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] protected TMP_Dropdown dropdown;
    [SerializeField] protected HoverHandler hoverHandler;
    private RectTransform dropdownRectTransform;
    private TextMeshProUGUI label;

    [Header("Config")]
    [SerializeField] bool _setInitialIndex = true;
    [SerializeField] protected int _initialIndex = -1;

    [Header("Visual")]
    [SerializeField] int _dropdownWidth = 700;
    [SerializeField] int _dropdownHeight = 100;
    [SerializeField] int _itemHeight = 80;
    [SerializeField] int _labelFontSize = 70;
    [SerializeField] int _itemFontSize = 60;

    protected virtual void Awake()
    {
        if (dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }

        if (dropdown != null)
        {
            dropdown.onValueChanged.AddListener(HandleValueChanged);

            CustomizeDropdownVisual();
        }
    }

    protected virtual void CustomizeDropdownVisual()
    {
        if (dropdown.template == null) return;

        // Label
        dropdownRectTransform = dropdown.GetComponent<RectTransform>();
        dropdownRectTransform.sizeDelta = new Vector2(_dropdownWidth, _dropdownHeight);

        GameObject labelObject = transform.Find("Label").GameObject();
        label = labelObject.GetComponent<TextMeshProUGUI>();
        label.fontSize = _labelFontSize;

        // Item
        var itemButton = dropdown.template.GetComponentInChildren<Toggle>(true);
        if (itemButton != null)
        {
            var rt = itemButton.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(0.0f, _itemHeight);
        }

        var itemText = dropdown.template.GetComponentInChildren<TextMeshProUGUI>(true);
        if (itemText != null)
        {
            itemText.fontSize = _itemFontSize;
            itemText.alignment = TextAlignmentOptions.Left;
            itemText.enableAutoSizing = false;
        }

        var contentFitter = dropdown.template.GetComponentInChildren<ContentSizeFitter>(true);
        if (contentFitter != null)
        {
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
    }


    private void HandleValueChanged(int index)
    {
        OnChangedValue(index);
    }

    /// <summary>
    /// Implement processing when the value of a drop-down is changed in a derived class
    /// </summary>
    protected abstract void OnChangedValue(int index);

    /// <summary>
    /// Replace drop-down choices with a list of labels and set initial selection values as needed.
    /// </summary>
    /// <param name="labels">String list of choices</param>
    /// <param name="defaultIndex">Index to be initially selected (default is 0)</param>
    /// <param name="notify">Whether to call OnChangedValue on selection explicitly (default is falseÅj</param>
    protected void SetOptions(List<string> labels, int defaultIndex = 0, bool notify = false)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(labels);

        if (defaultIndex < 0) return;

        if (notify)
        {
            dropdown.value = defaultIndex;
        }
        else
        {
            dropdown.SetValueWithoutNotify(defaultIndex);
            OnChangedValue(defaultIndex);
        }
    }

    protected void SetInitialIndex()
    {
        if (!_setInitialIndex) return;

        dropdown.value = _initialIndex;
        OnChangedValue(_initialIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (hoverHandler != null)
        {
            hoverHandler.PreventAutoClose = true;
            StartCoroutine(WatchDropdownClose());
        }
    }

    private IEnumerator WatchDropdownClose()
    {
        while (dropdown.template != null && dropdown.template.gameObject.activeSelf)
        {
            yield return null;
        }
        yield return null; // 1ÉtÉåÅ[ÉÄóPó\

        if (hoverHandler != null)
        {
            hoverHandler.PreventAutoClose = false;
        }
    }
}
