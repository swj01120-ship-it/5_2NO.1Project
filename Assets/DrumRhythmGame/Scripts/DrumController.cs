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

    [Header("사운드 설정")]
    public AudioClip drumSound; // 드럼 타격 소리
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.5f, 2f)]
    public float pitch = 1f;

    [Header("컴포넌트")]
    private Renderer drumRenderer;
    private Material drumMaterial;
    private AudioSource audioSource;

    [Header("타이밍")]
    private bool isHighlighted = false;
    private float highlightStartTime;
    private float perfectWindow;   // 초기값 제거
    private float greatWindow;  // 초기값 제거
    private float goodWindow;     // 초기값 제거

    [Header("효과")]
    public ParticleSystem hitParticle;

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

        // AudioSource 설정
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // AudioSource 초기 설정
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D 사운드
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        //난이도 설정
        if (DifficultySettings.Instance != null)
        {
            DifficultySettings.Instance.GetJudgmentWindows(out perfectWindow, out greatWindow, out goodWindow);
            Debug.Log($"✅ Drum {drumIndex} 초기화! (Perfect: {perfectWindow}s, Great: {greatWindow}s, Good: {goodWindow}s)");
        }
        else
        {
            // DifficultySettings가 없으면 기본값 사용
            perfectWindow = 0.15f;
            greatWindow = 0.25f;
            goodWindow = 0.35f;
            Debug.LogWarning($"⚠️ DifficultySettings가 없습니다. 기본 난이도 사용");
        }

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
        // 🔊 사운드 먼저 재생 (타이밍 정확도를 위해)
        PlayDrumSound();

        if (!isHighlighted)
        {
            // 강조되지 않았을 때 타격 = Miss
            Debug.Log($"❌ Miss! (Drum {drumIndex}) - 강조되지 않았을 때 침");

            // ✅ 게임 매니저 체크 추가
            if (RhythmGameManager.Instance != null)
            {
                RhythmGameManager.Instance.OnDrumHit("Miss", drumIndex);
            }
            else if (TutorialRhythmManager.Instance != null)
            {
                TutorialRhythmManager.Instance.OnTutorialDrumHit("Miss", drumIndex);
            }


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

        // ✅ 게임 매니저 체크 추가
        if (RhythmGameManager.Instance != null)
        {
            RhythmGameManager.Instance.OnDrumHit(judgment, drumIndex);
        }
        else if (TutorialRhythmManager.Instance != null)
        {
            TutorialRhythmManager.Instance.OnTutorialDrumHit(judgment, drumIndex);
        }

        // 시각 효과
        ShowHitEffect();
        UnHighlight();
    }

    void PlayDrumSound()
    {
        if (audioSource != null && drumSound != null)
        {
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(drumSound);

            Debug.Log($"🔊 Drum {drumIndex} 사운드 재생!");
        }
        else
        {
            if (drumSound == null)
            {
                Debug.LogWarning($"⚠️ Drum {drumIndex}: 사운드가 연결되지 않았습니다!");
            }
        }
    }
    void ShowHitEffect()
    {
        if (hitParticle != null)
        {
            hitParticle.Play();
        }

        StartCoroutine(HitFlash());

        // ✅ 드럼 펀치 애니메이션
        StartCoroutine(DrumPunchAnimation());
    }
    System.Collections.IEnumerator DrumPunchAnimation()
    {
        Vector3 originalScale = transform.localScale;
        Vector3 punchScale = originalScale * 1.2f;

        // 커지기
        float elapsed = 0f;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(originalScale, punchScale, t);
            yield return null;
        }

        // 작아지기
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(punchScale, originalScale, t);
            yield return null;
        }

        transform.localScale = originalScale;
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
