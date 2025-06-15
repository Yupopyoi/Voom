// CapturePanel.cs - Base class for movable, scalable, and auto-destroyable UI panels
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Capture
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public abstract class CapturePanel : MonoBehaviour, IDragHandler, IPointerDownHandler, IScrollHandler
    {
        [SerializeField] protected RawImage imageTarget;

        protected RectTransform rectTransform;
        private Image panelImage;
        private Vector2 lastMousePosition;
        private RectTransform targetRectTransform;

        [SerializeField, Range(0.01f, 10.0f)] float initialSize = 5.0f;

        [Header("Sensitivity")]
        [SerializeField, Range(0.01f,20.0f)] float sizeChangeSensitivity = 5.0f;

        protected virtual void Start()
        {
            rectTransform = GetComponent<RectTransform>();
            panelImage = GetComponent<Image>();

            if (imageTarget == null)
            {
                Debug.LogError("[CapturePanel] NullReferenceException: imageTarget is null. Please specify it from the inspector.");
                return;
            }

            targetRectTransform = imageTarget.GetComponent<RectTransform>();
        }

        protected virtual void Update()
        {
            // Destroy panel if Delete key is pressed
            if (Input.GetKeyDown(KeyCode.Delete))
            {
                Destroy(gameObject);
            }

            // Destroy panel if completely outside screen bounds
            Vector3[] corners = new Vector3[4];
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, eventData.position, eventData.pressEventCamera, out lastMousePosition);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform, eventData.position, eventData.pressEventCamera, out Vector2 currentMousePosition))
            {
                Vector2 offset = currentMousePosition - lastMousePosition;
                rectTransform.anchoredPosition += offset;
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            float scaleFactor = 1 + eventData.scrollDelta.y * 0.001f * sizeChangeSensitivity;
            rectTransform.localScale *= scaleFactor;
        }

        protected int[] CalculateAspectRatio(int width, int height)
        {
            int w = width;
            int h = height;
            int[] divisors = new int[4] { 2, 3, 5, 7 };
            foreach (int d in divisors)
            {
                while ((w % d == 0) && (h % d == 0))
                {
                    w /= d;
                    h /= d;
                }
            }
            return new int[] { w, h };
        }

        protected void ResizeToFitRawImage()
        {
            rectTransform.localScale = new Vector3(initialSize, initialSize, 1.0f);
            rectTransform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

            rectTransform.sizeDelta = new Vector2(targetRectTransform.sizeDelta.x, targetRectTransform.sizeDelta.y);

            float diff  = targetRectTransform.sizeDelta.x - targetRectTransform.sizeDelta.y;
            float ratio = targetRectTransform.sizeDelta.y / targetRectTransform.sizeDelta.x;

            panelImage.raycastPadding = new Vector4(0.0f, diff * ratio, 0.0f, 0.0f);
        }

        protected abstract void StartCapture();
        protected abstract void Stop();
    }
}// namespace Capture