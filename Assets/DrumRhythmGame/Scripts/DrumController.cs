using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumController : MonoBehaviour
{
    [Header("드럼 설정")]
    public int drumIndex;
    public KeyCode drumKey;

    [Header("⭐ 게임 모드 선택")]
    public bool useNoteSystem = true;

    [Header("색상 설정")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.red;
    public Color hitColor = Color.yellow;

    [Header("색상 변경 속도")]
    public float colorChangeDuration = 0.3f;

    [Header("사운드 설정")]
    public AudioClip drumSound;
    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.5f, 2f)]
    public float pitch = 1f;

    [Header("컴포넌트")]
    private Renderer drumRenderer;
    private Material drumMaterial;
    private AudioSource audioSource;

    private bool isHighlighted = false;
    private Vector3 originalScale;

    private Coroutine colorChangeCoroutine;

    [Header("판정 윈도우")]
    public float perfectDistance = 0.3f;
    public float greatDistance = 0.8f;
    public float goodDistance = 1.5f;
    public float hitCheckRadius = 3f;

    [Header("효과")]
    public ParticleSystem hitParticle;
    public GameObject tutorialHitEffectPrefab;
    public float effectYOffset = 0.6f;
    public bool isTutorialMode = false;

    void Start()
    {
        drumRenderer = GetComponent<Renderer>();
        if (drumRenderer == null)
            drumRenderer = GetComponentInChildren<Renderer>();
        if (drumRenderer == null)
        {
            Debug.LogError($"❌ Drum {drumIndex}: Renderer가 없습니다!");
            return;
        }

        drumMaterial = new Material(drumRenderer.material);
        drumRenderer.material = drumMaterial;
        SetColor(normalColor);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;
        audioSource.pitch = pitch;

        originalScale = transform.localScale;

        // ⭐ 어떤 매니저가 있는지 확인
        if (RhythmGameManager.Instance != null)
            Debug.Log($"✅ Drum {drumIndex}: RhythmGameManager 연결됨");
        else if (TutorialRhythmManager.Instance != null)
            Debug.Log($"✅ Drum {drumIndex}: TutorialRhythmManager 연결됨");
        else
            Debug.LogWarning($"⚠️ Drum {drumIndex}: 매니저가 없습니다!");
    }

    void Update()
    {
        // ⭐ 일시정지 중에는 입력 무시
        if (Time.timeScale == 0f) return;

        if (Input.GetKeyDown(drumKey))
            HitDrum();
    }

    // ⭐⭐⭐ 수정: 비활성화 상태에서도 색상 변경 가능
    public void Highlight()
    {
        isHighlighted = true;
        
        // ⭐ 비활성화 상태면 즉시 색상 변경
        if (!gameObject.activeInHierarchy)
        {
            SetColor(highlightColor);
            Debug.Log($"🥁 Drum {drumIndex} 커버 색상 변경! (비활성화 상태)");
            return;
        }
        
        // 활성화 상태면 코루틴으로 부드러운 전환
        if (colorChangeCoroutine != null) StopCoroutine(colorChangeCoroutine);
        colorChangeCoroutine = StartCoroutine(ChangeColorCoroutine(highlightColor));
        ResetScale();
        Debug.Log($"🥁 Drum {drumIndex} 커버 색상 변경!");
    }

    // ⭐⭐⭐ 수정: 비활성화 상태에서도 색상 복구 가능
    public void UnHighlight()
    {
        isHighlighted = false;
        
        // ⭐ 비활성화 상태면 즉시 색상 변경
        if (!gameObject.activeInHierarchy)
        {
            SetColor(normalColor);
            Debug.Log($"🥁 Drum {drumIndex} 커버 색상 복구! (비활성화 상태)");
            return;
        }
        
        // 활성화 상태면 코루틴으로 부드러운 전환
        if (colorChangeCoroutine != null) StopCoroutine(colorChangeCoroutine);
        colorChangeCoroutine = StartCoroutine(ChangeColorCoroutine(normalColor));
        ResetScale();
    }

    private IEnumerator ChangeColorCoroutine(Color targetColor)
    {
        Color startColor = drumMaterial.color;
        float elapsed = 0f;

        while (elapsed < colorChangeDuration)
        {
            elapsed += Time.deltaTime;
            drumMaterial.color = Color.Lerp(startColor, targetColor, elapsed / colorChangeDuration);
            yield return null;
        }
        drumMaterial.color = targetColor;
        colorChangeCoroutine = null;
    }

    void HitDrum()
    {
        PlayDrumSound();

        if (useNoteSystem)
            HitDrum_NoteMode();
        else
            HitDrum_HighlightMode();
    }

    void HitDrum_HighlightMode()
    {
        // 사용 안 함
    }

    void HitDrum_NoteMode()
    {
        List<NoteObject> nearbyNotes = FindNotesInRange();

        // ⭐ 튜토리얼 모드: 노트가 없으면 Highlight 기반으로 판정
        if (nearbyNotes.Count == 0)
        {
            // 튜토리얼 매니저가 있고 플레이 중이면
            if (TutorialRhythmManager.Instance != null && TutorialRhythmManager.Instance.IsPlaying())
            {
                // Highlight된 드럼을 쳤는지 확인
                if (isHighlighted)
                {
                    Debug.Log($"✅ Tutorial Hit: Drum {drumIndex} - Highlighted!");
                    TutorialRhythmManager.Instance.OnTutorialDrumHit("Perfect", drumIndex);
                    ShowHitEffect(true);
                }
                else
                {
                    Debug.Log($"❌ Tutorial Miss: Drum {drumIndex} - Not Highlighted!");
                    TutorialRhythmManager.Instance.OnTutorialDrumHit("Miss", drumIndex);
                }
                return;
            }

            // 일반 게임 모드에서 노트 없음
            Debug.Log($"❌ Miss! (Drum {drumIndex}) - 노트 없음");
            if (RhythmGameManager.Instance != null)
            {
                RhythmGameManager.Instance.OnDrumHit("Miss", drumIndex);
            }

            return;
        }

        // ⭐ 노트가 있는 경우 (일반 게임 모드)
        NoteObject closestNote = GetClosestNote(nearbyNotes);
        float distance = closestNote.GetDistanceToTarget();
        string judgment = GetJudgmentFromDistance(distance);

        Debug.Log($"🥁 Hit: Drum {drumIndex}, 거리: {distance:F2}, 판정: {judgment}");

        closestNote.OnHit(judgment);

        // 두 매니저 모두 지원
        if (RhythmGameManager.Instance != null)
        {
            RhythmGameManager.Instance.OnDrumHit(judgment, drumIndex);
            RhythmGameManager.Instance.RemoveNote(closestNote);
        }
        else if (TutorialRhythmManager.Instance != null)
        {
            TutorialRhythmManager.Instance.OnTutorialDrumHit(judgment, drumIndex);
        }

        if (judgment != "Miss")
            ShowHitEffect(true);
    }

    NoteObject GetClosestNote(List<NoteObject> notes)
    {
        NoteObject closest = notes[0];
        float minDistance = closest.GetDistanceToTarget();

        for (int i = 1; i < notes.Count; i++)
        {
            float distance = notes[i].GetDistanceToTarget();
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = notes[i];
            }
        }
        return closest;
    }

    string GetJudgmentFromDistance(float distance)
    {
        if (distance <= perfectDistance)
            return "Perfect";
        else if (distance <= greatDistance)
            return "Great";
        else if (distance <= goodDistance)
            return "Good";
        else
            return "Miss";
    }

    List<NoteObject> FindNotesInRange()
    {
        List<NoteObject> notesInRange = new List<NoteObject>();
        NoteObject[] allNotes = FindObjectsOfType<NoteObject>();

        foreach (NoteObject note in allNotes)
        {
            if (note.drumIndex != drumIndex) continue;
            if (!note.CanBeHit()) continue;
            float distance = Vector3.Distance(note.transform.position, transform.position);
            if (distance <= hitCheckRadius)
                notesInRange.Add(note);
        }
        return notesInRange;
    }

    void PlayDrumSound()
    {
        if (audioSource != null && drumSound != null)
        {
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(drumSound);
            Debug.Log($"🔊 Drum {drumIndex} 사운드 재생");
        }
    }

    // ⭐⭐⭐ 수정된 부분: 튜토리얼 모드일 때 매니저에서 이펙트 처리
    void ShowHitEffect(bool playEffect)
    {
        if (!playEffect) return;

        // ⭐ 튜토리얼 모드일 때는 TutorialRhythmManager에서 이펙트 처리
        if (isTutorialMode && TutorialRhythmManager.Instance != null)
        {
            TutorialRhythmManager.Instance.PlayTutorialHitEffect(drumIndex);
            return;
        }

        // ⭐ 일반 모드: 드럼이 활성화되어 있을 때만 실행
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning($"⚠️ Drum {drumIndex}가 비활성화 상태여서 이펙트를 실행할 수 없습니다!");
            return;
        }

        // 일반 모드 이펙트
        if (hitParticle != null)
            hitParticle.Play();

        if (tutorialHitEffectPrefab != null)
        {
            Vector3 effectPos = transform.position + Vector3.up * effectYOffset;
            Instantiate(tutorialHitEffectPrefab, effectPos, Quaternion.identity);
        }

        StartCoroutine(HitFlash());
        StartCoroutine(DrumPunchAnimation());
    }

    // ⭐⭐⭐ 새로 추가: 외부(매니저)에서 호출 가능한 히트 플래시
    public void PlayHitFlash()
    {
        SetColor(hitColor);
        // 0.1초 후 하이라이트 색상으로 복구 (비활성화 상태에서도 작동)
        Invoke("RestoreHighlightColor", 0.1f);
    }

    void RestoreHighlightColor()
    {
        SetColor(highlightColor);
    }

    IEnumerator DrumPunchAnimation()
    {
        ResetScale();
        float duration = 0.12f;
        Vector3 punchScale = originalScale * 1.13f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(originalScale, punchScale, t);
            yield return null;
        }
        elapsed = 0;
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

    void OnDrawGizmosSelected()
    {
        if (!useNoteSystem) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, hitCheckRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, perfectDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, greatDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, goodDistance);
    }
}
