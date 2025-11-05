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
        //사운드 먼저 재생
        PlayDrumSound();

        string judgment = "Miss";
        float timeDifference = Mathf.Abs(Time.time - highlightStartTime);

        if (isHighlighted)
        {
            if (timeDifference <= perfectWindow)
                judgment = "Perfect";
            else if (timeDifference <= greatWindow)
                judgment = "Great";
            else if (timeDifference <= goodWindow)
                judgment = "Good";
        }

        RhythmGameManager.Instance.OnDrumHit(judgment, drumIndex);
        ShowHitEffect();
        UnHighlight();
        return;
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
}
