using UnityEngine;

public class ShineUI : MonoBehaviour
{
    public RectTransform shineRect;       // ShineImage RectTransform
    public RectTransform maskTargetRect;  // LogoImage RectTransform (RectMask2D의 범위)
    public float speed = 1500f;           // 픽셀/초 단위 이동 속도
    public float startOffsetMul = -1.2f;  // 시작 X 오프셋(타겟 폭 기준)
    public float endOffsetMul = 1.2f;     // 끝 X 오프셋

    Vector2 startPos, endPos;

    void Start()
    {
        if (shineRect == null || maskTargetRect == null)
        {
            Debug.LogError("ShineUI: shineRect 또는 maskTargetRect가 할당되지 않음.");
            enabled = false;
            return;
        }

        float w = maskTargetRect.rect.width;

        startPos = new Vector2(w * startOffsetMul, 0f);
        endPos = new Vector2(w * endOffsetMul, 0f);

        shineRect.anchoredPosition = startPos;
    }

    void Update()
    {
        Vector2 p = shineRect.anchoredPosition;
        p.x += speed * Time.deltaTime;
        shineRect.anchoredPosition = p;

        if (p.x >= endPos.x)
            shineRect.anchoredPosition = startPos;
    }
}
