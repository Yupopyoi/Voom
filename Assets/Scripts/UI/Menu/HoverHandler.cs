using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class HoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image _selectedImage;
    [SerializeField] GameObject _subMenuPanel;

    private bool _isPointerOver = false;
    private bool _isPointerOverSubMenu = false;

    private bool _preventAutoClose = false;

    public bool PreventAutoClose { get { return _preventAutoClose; } set { _preventAutoClose = value; } }

    void Start()
    {
        if (_subMenuPanel != null) _subMenuPanel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isPointerOver = true;
        _selectedImage.gameObject.SetActive(true);
        if (_subMenuPanel != null) _subMenuPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isPointerOver = false;

        if (_preventAutoClose)
        {
            return;
        }

        StartCoroutine(DelayedHideWrapper());
    }

    private IEnumerator DelayedHideWrapper()
    {
        yield return null; // 1ÉtÉåÅ[ÉÄë“Ç¬
        yield return StartCoroutine(DelayedHide());
    }

    private IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(0.1f);

        // If it was closed on its own, this is where it ends.
        if (_subMenuPanel != null && !_subMenuPanel.activeInHierarchy)
        {
            yield break;
        }

        if (!_isPointerOver && !_isPointerOverSubMenu && !_preventAutoClose)
        {
            _selectedImage.gameObject.SetActive(false);
            if (_subMenuPanel != null) _subMenuPanel.SetActive(false);
        }
    }

    public void SetSubMenuHover(bool hover)
    {
        _isPointerOverSubMenu = hover;
        if (!hover && !_isPointerOver && !_preventAutoClose)
        {
            _selectedImage.gameObject.SetActive(false);
            if (_subMenuPanel != null) _subMenuPanel.SetActive(false);
        }
    }
}
