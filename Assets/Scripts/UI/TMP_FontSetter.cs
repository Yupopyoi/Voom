using UnityEngine;
using TMPro;

public class TMP_FontSetter : MonoBehaviour
{
    [SerializeField] TMP_FontAsset font;

    void Start()
    {
        // 子オブジェクト以下にある全てのTextMeshPro（TMP_Text）を取得
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true); // 非アクティブも含む

        foreach (TMP_Text text in texts)
        {
            text.font = font;
        }
    }
}
