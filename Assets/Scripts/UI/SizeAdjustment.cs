using UnityEngine;

public class SizeAdjustment : MonoBehaviour
{
    void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        float contractionRatio = UnityEngine.Screen.width / 3840.0f;  /* 3840 = Horizontal pixels in 4K quality */

        rectTransform.localScale = new Vector3(contractionRatio, contractionRatio, contractionRatio);
    }
}
