using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumController : MonoBehaviour
{
    [Header("드럼 설정")]
    public int drumIndex; // 0, 1, 2, 3
    public KeyCode drumKey; // A, S, D, F

    [Header("색상 설정")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.red;
    public Color hitColor = Color.yellow;

    [Header("컴포넌트")]
    private Renderer drumRenderer;
    private Material drumMaterial;

    [Header("타이밍")]
    private bool isHighlighted = false;
    private float highlightStartTime;
    private float perfectWindow = 0.1f;  // Perfect 판정 범위 (±0.1초)
    private float greatWindow = 0.15f;    // Great 판정 범위 (±0.15초)
    private float goodWindow = 0.2f;      // Good 판정 범위 (±0.2초)

    void Start()
    {
        drumRenderer = GetComponent<Renderer>();

        if (drumRenderer == null)
        {
            Debug.LogError($"❌ Drum {drumIndex}: Renderer가 없습니다! 3D 오브젝트에 부착하세요.");
            return;
        }

        // Material 복사 (각 드럼이 독립적인 색상 가질 수 있도록)
        drumMaterial = new Material(drumRenderer.material);
        drumRenderer.material = drumMaterial;

        SetColor(normalColor);

        Debug.Log($"✅ Drum {drumIndex} 초기화 완료! (키: {drumKey})");
    }

    void Update()
    {
        // 키 입력 감지
        if (Input.GetKeyDown(drumKey))
        {
            HitDrum();
        }
    }

    // 북을 강조 표시 (리듬 타이밍에 호출됨)
    public void Highlight()
    {
        isHighlighted = true;
        highlightStartTime = Time.time;
        SetColor(highlightColor);

        Debug.Log($"🥁 Drum {drumIndex} 강조됨! (키: {drumKey})");
    }

    // 강조 해제
    public void UnHighlight()
    {
        isHighlighted = false;
        SetColor(normalColor);
    }

    // 북 타격 처리
    void HitDrum()
    {
        if (!isHighlighted)
        {
            // 강조되지 않았을 때 타격 = Miss
            Debug.Log($"❌ Miss! (Drum {drumIndex}) - 강조되지 않았을 때 침");
            RhythmGameManager.Instance.OnDrumHit("Miss", drumIndex);
            ShowHitEffect();
            return;
        }

        // 타이밍 계산
        float timeDifference = Mathf.Abs(Time.time - highlightStartTime);

        string judgment;
        if (timeDifference <= perfectWindow)
        {
            judgment = "Perfect";
            Debug.Log($"⭐ Perfect! (Drum {drumIndex}, {timeDifference:F3}초 차이)");
        }
        else if (timeDifference <= greatWindow)
        {
            judgment = "Great";
            Debug.Log($"✨ Great! (Drum {drumIndex}, {timeDifference:F3}초 차이)");
        }
        else if (timeDifference <= goodWindow)
        {
            judgment = "Good";
            Debug.Log($"👍 Good! (Drum {drumIndex}, {timeDifference:F3}초 차이)");
        }
        else
        {
            judgment = "Miss";
            Debug.Log($"❌ Miss! (Drum {drumIndex}, {timeDifference:F3}초 차이 - 너무 늦음)");
        }

        // 게임 매니저에 알림
        RhythmGameManager.Instance.OnDrumHit(judgment, drumIndex);

        // 시각 효과
        ShowHitEffect();
        UnHighlight();
    }

    void ShowHitEffect()
    {
        StartCoroutine(HitFlash());
    }

    System.Collections.IEnumerator HitFlash()
    {
        SetColor(hitColor);
        yield return new WaitForSeconds(0.1f);
        SetColor(isHighlighted ? highlightColor : normalColor);
    }

    void SetColor(Color color)
    {
        if (drumMaterial != null)
        {
            drumMaterial.color = color;
        }
    }
}
