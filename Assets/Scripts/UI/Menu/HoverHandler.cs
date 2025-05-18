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
        StartCoroutine(DelayedHide());
    }

    private IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(0.1f); // è≠Çµë“Ç¡ÇƒÇ©ÇÁèÛë‘ämîF
        if (!_isPointerOver && !_isPointerOverSubMenu)
        {
            _selectedImage.gameObject.SetActive(false);
            if (_subMenuPanel != null) _subMenuPanel.SetActive(false);
        }
    }

    public void SetSubMenuHover(bool hover)
    {
        _isPointerOverSubMenu = hover;
        if (!hover && !_isPointerOver)
        {
            _selectedImage.gameObject.SetActive(false);
            if (_subMenuPanel != null) _subMenuPanel.SetActive(false);
        }
    }
}
