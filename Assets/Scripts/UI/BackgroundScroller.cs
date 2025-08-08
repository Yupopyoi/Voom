using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public Material backgroundMaterial;
    public float scrollSpeed = 0.04f;

    private Vector2 offset;

    void Update()
    {
        offset.x += scrollSpeed * Time.deltaTime;
        offset.y -= scrollSpeed * Time.deltaTime;
        backgroundMaterial.SetTextureOffset("_BaseMap", offset);
    }
}