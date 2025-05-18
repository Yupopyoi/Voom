using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuDisplayer : MonoBehaviour
{
    private Vector2 _dragStartPos;
    private bool _bothClickActive = false;
    private RectTransform _menuTransform;

    private float _contractionRatio; // Reduction rate from 4K image quality

    private List<RectTransform> _menuItems = new List<RectTransform>();

    [Header("General")]
    [SerializeField] private Canvas _menuCanvas;
    [SerializeField, Range(1f, 1000f)] private float _dragJudgmentThreshold = 100f; // [pixels]

    [Header("Deployment")]
    [SerializeField] float _verticalOffset = 150f; // No Use now
    [SerializeField] float _spacing = 250f;
    [SerializeField] float _animationTime = 0.07f;
    [SerializeField] float _delayBetweenItems = 0.01f;

    private void Start()
    {
        _menuCanvas.gameObject.SetActive(false);
        _menuTransform = _menuCanvas.GetComponent<RectTransform>();
        _dragJudgmentThreshold = Mathf.Clamp(_dragJudgmentThreshold, 0.0f, UnityEngine.Screen.height * 0.5f);

        _contractionRatio = UnityEngine.Screen.width / 3840.0f; /* 3840 = Horizontal pixels in 4K quality */

        // Find all CircleMenuItem
        foreach (Transform child in _menuCanvas.GetComponentsInChildren<Transform>(true))
        {
            if (child.CompareTag("CircleMenu"))
            {
                if (child.TryGetComponent<RectTransform>(out var rt))
                {
                    _menuItems.Add(rt);
                }
            }
        }

        foreach (RectTransform item in _menuItems)
        {
            item.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // Pressing both mouse buttons is the trigger to open the menu.
        // It is a conditional judgment that if either button is pressed while the other button is pressed.
        if ((Input.GetMouseButtonDown(0) && Input.GetMouseButton(1)) || (Input.GetMouseButton(0) && Input.GetMouseButtonDown(1)))
        {
            _dragStartPos = Input.mousePosition;
            _bothClickActive = true;
        }

        if (_bothClickActive)
        {
            Vector2 dragEndPos = Input.mousePosition;
            float dragDistance = dragEndPos.y - _dragStartPos.y;

            if (dragDistance < -_dragJudgmentThreshold /* Downward */)
            {
                ShowMainMenu();
                _bothClickActive = false;
            }
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            _bothClickActive = false;
        }

        if (Input.GetMouseButtonDown(2))
        {
            HideMainMenu();
        }
    }

    void ShowMainMenu()
    {
        _menuCanvas.gameObject.SetActive(true);

        StartCoroutine(AnimateMenuItems());
    }

    void HideMainMenu()
    {
        _menuCanvas.gameObject.SetActive(false);
    }

    private IEnumerator AnimateMenuItems()
    {
        // Move all circle items to start position 
        for (int i = 0; i < _menuItems.Count; i++)
        {
            RectTransform item = _menuItems[i];
            item.gameObject.SetActive(false);

            Vector2 startPos = _dragStartPos;
            item.localPosition = (Vector3)startPos;
        }

        for (int i = _menuItems.Count - 1; i >= 0; i--)
        {
            RectTransform item = _menuItems[i];
            item.gameObject.SetActive(true);;

            Vector2 endPos = _dragStartPos - new Vector2(0.0f, _spacing * (_menuItems.Count - i) * _contractionRatio);
            Vector2 startPos = endPos + new Vector2(0.0f, _spacing * _contractionRatio);

            yield return StartCoroutine(AnimateItemMove(item, startPos, endPos, _animationTime));
            yield return new WaitForSeconds(_delayBetweenItems);
        }
    }

    private IEnumerator AnimateItemMove(RectTransform rt, Vector2 from, Vector2 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float easedT = 1 - Mathf.Pow(1 - t, 3); // easeOutCubic
            rt.anchoredPosition = Vector2.Lerp(from, to, easedT);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rt.anchoredPosition = to;
    }
}
