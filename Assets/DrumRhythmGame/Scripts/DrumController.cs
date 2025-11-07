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

    private bool isHighlighted = false;
    private float highlightStartTime;

    [Header("판정 윈도우 (초 단위)")]
    public float perfectWindow = 0.07f;
    public float greatWindow = 0.5f;
    public float goodWindow = 1.2f;

    [Header("효과")]
    public ParticleSystem hitParticle;

    [Header("튜토리얼 전용 프리팹 이펙트 (Instantiate용, 드럼 위 생성)")]
    public GameObject tutorialHitEffectPrefab;

    [Header("이펙트 Y 위치 보정값")]
    public float effectYOffset = 0.6f;


    private Vector3 originalScale;

    void Start()
    {
        drumRenderer = GetComponent<Renderer>();
        if (drumRenderer == null)
        {
            Debug.LogError($"❌ Drum {drumIndex}: Renderer가 없습니다! 3D 오브젝트에 부착하세요.");
            return;
        }

        drumRenderer = GetComponentInChildren<Renderer>();

        if (drumRenderer == null)
        {
            Debug.LogError($"❌ Drum {drumIndex}: Renderer가 없습니다!");
            return;
        }

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

        originalScale = transform.localScale;

        //난이도 설정
        if (DifficultySettings.Instance != null)
        {
            DifficultySettings.Instance.GetJudgmentWindows(out perfectWindow, out greatWindow, out goodWindow);
            Debug.Log($"✅ Drum {drumIndex} 초기화! (Perfect: {perfectWindow}s, Great: {greatWindow}s, Good: {goodWindow}s)");
        }
    }

    void Update()
    {
        // GameObject가 비활성화 상태면 아무것도 안 함
        if (!gameObject.activeInHierarchy)
        {
            return;
        }
        //키 입력 감지
        if (Input.GetKeyDown(drumKey))
        {
            HitDrum();
        }
    }

    //북을 강조 표시(리듬 타이밍에 호출)
    public void Highlight()
    {
        isHighlighted = true;
        highlightStartTime = Time.time;
        SetColor(highlightColor);

        Debug.Log($"🥁 Drum {drumIndex} 강조됨! (키: {drumKey})");
        ResetScale();
    }

    //강조 해제
    public void UnHighlight()
    {
        isHighlighted = false;
        SetColor(normalColor);
        ResetScale();
    }

    // 북 타격 처리
    void HitDrum()
    {
        // 🔊 사운드 먼저 재생
        PlayDrumSound();

        if (!isHighlighted)
        {
            Debug.Log($"❌ Miss! (Drum {drumIndex}) - 강조되지 않았을 때 침");

            // ✅ null 체크 추가!
            if (RhythmGameManager.Instance != null)
            {
                RhythmGameManager.Instance.OnDrumHit("Miss", drumIndex);
            }
            else if (TutorialRhythmManager.Instance != null)
            {
                TutorialRhythmManager.Instance.OnTutorialDrumHit("Miss", drumIndex);
            }
            else
            {
                Debug.LogWarning("⚠️ 게임 매니저를 찾을 수 없습니다!");
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
        }
        else if (timeDifference <= greatWindow)
        {
            judgment = "Great";
        }
        else if (timeDifference <= goodWindow)
        {
            judgment = "Good";
        }
        else
        {
            judgment = "Miss";
        }

        // ✅ 여기도 null 체크!
        if (RhythmGameManager.Instance != null)
        {
            RhythmGameManager.Instance.OnDrumHit(judgment, drumIndex);
        }
        else if (TutorialRhythmManager.Instance != null)
        {
            TutorialRhythmManager.Instance.OnTutorialDrumHit(judgment, drumIndex);
        }
        else
        {
            Debug.LogWarning("⚠️ 게임 매니저를 찾을 수 없습니다!");
        }

        // GOOD 이상 판정일 때만 이펙트 출력함
        if (judgment == "Perfect" || judgment == "Great" || judgment == "Good")
            ShowHitEffect(true);

        else
            ShowHitEffect(false);

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

    void ShowHitEffect(bool isGoodHit = true)
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (hitParticle != null && isGoodHit)
        {
            hitParticle.Play();
        }
        if (tutorialHitEffectPrefab != null && isGoodHit)
        {
            Vector3 effectPos = transform.position + Vector3.up * effectYOffset;
            Instantiate(tutorialHitEffectPrefab, effectPos, Quaternion.identity);
            Debug.Log($"튜토리얼 이펙트 생성 위치: {effectPos}");
        }
        StartCoroutine(HitFlash());
        StartCoroutine(DrumPunchAnimation());
    }

    IEnumerator DrumPunchAnimation()
    {
        ResetScale();
        float duration = 0.12f;
        Vector3 punchScale = originalScale * 1.13f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(originalScale, punchScale, t);
            yield return null;
        }
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(punchScale, originalScale, t);
            yield return null;
        }
        ResetScale();
    }

    IEnumerator HitFlash()
    {
        SetColor(hitColor);
        yield return new WaitForSeconds(0.1f);
        SetColor(isHighlighted ? highlightColor : normalColor);
    }

    void SetColor(Color color)
    {
        if (drumMaterial != null)
            drumMaterial.color = color;
    }

    void ResetScale()
    {
        transform.localScale = originalScale;
    }
    void OnDisable()
    {
        // 이 GameObject의 모든 Coroutine 중지
        StopAllCoroutines();

        // 색상 초기화
        if (drumMaterial != null)
        {
            drumMaterial.color = normalColor;
        }
    }
}
