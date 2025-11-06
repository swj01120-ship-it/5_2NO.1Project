using UnityEngine;

public class ScrollBackground : MonoBehaviour
{
    public float scrollSpeed = 5.0f; // 배경이 지나가는 속도
    private Renderer rend;
    private Vector2 offset = Vector2.zero;

    void Start() => rend = GetComponent<Renderer>();

    void Update()
    {
        offset.x += scrollSpeed * Time.deltaTime;
        rend.material.mainTextureOffset = offset;
    }
}
