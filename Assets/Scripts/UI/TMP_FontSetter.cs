using UnityEngine;
using TMPro;

public class TMP_FontSetter : MonoBehaviour
{
    [SerializeField] TMP_FontAsset font;

    void Start()
    {
        // �q�I�u�W�F�N�g�ȉ��ɂ���S�Ă�TextMeshPro�iTMP_Text�j���擾
        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true); // ��A�N�e�B�u���܂�

        foreach (TMP_Text text in texts)
        {
            text.font = font;
        }
    }
}
