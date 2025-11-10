using System.Collections;
using UnityEngine;

public class NoteObject : MonoBehaviour
{
    [Header("노트 정보")]
    public int drumIndex;
    public float targetTime;

    [Header("이동 설정")]
    public float fallSpeed = 5f;
    private Vector3 targetPosition;

    [Header("판정 상태")]
    private bool isHit = false;
    private bool canBeHit = false;
    private bool highlightCalled = false;
    private bool isMissedByTimeout = false;

    [Header("시각 효과")]
    private Renderer noteRenderer;
    private Material noteMaterial;

    void Start()
    {
        noteRenderer = GetComponent<Renderer>();
        if (noteRenderer != null)
        {
            noteMaterial = new Material(noteRenderer.material);
            noteRenderer.material = noteMaterial;
        }
    }

    void Update()
    {
        if (isHit) return;

        transform.position += Vector3.down * fallSpeed * Time.deltaTime;

        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        if (distanceToTarget < 0.05f && !highlightCalled)
        {
            highlightCalled = true;
            if (RhythmGameManager.Instance != null &&
                drumIndex >= 0 &&
                drumIndex < RhythmGameManager.Instance.drums.Length)
            {
                RhythmGameManager.Instance.drums[drumIndex].Highlight();
            }
        }

        canBeHit = distanceToTarget < 3f;

        if (transform.position.y < targetPosition.y - 2f && !isHit)
        {
            isMissedByTimeout = true;
            OnMiss();
        }
    }

    public void SetTargetPosition(Vector3 target)
    {
        targetPosition = target;
    }

    public void SetColor(Color color)
    {
        if (noteMaterial != null)
            noteMaterial.color = color;
    }

    public bool CanBeHit()
    {
        return canBeHit && !isHit;
    }

    public void OnHit(string judgment)
    {
        if (isHit) return;
        isHit = true;

        if (RhythmGameManager.Instance != null &&
            drumIndex >= 0 &&
            drumIndex < RhythmGameManager.Instance.drums.Length)
        {
            RhythmGameManager.Instance.drums[drumIndex].UnHighlight();
        }

        StartCoroutine(HitEffect());
    }

    private void OnMiss()
    {
        if (isHit) return;
        isHit = true;

        // 커버 색상 복구는 무조건 호출해서 자연 소멸 시에도 복구되도록 조치
        if (RhythmGameManager.Instance != null &&
            drumIndex >= 0 &&
            drumIndex < RhythmGameManager.Instance.drums.Length)
        {
            RhythmGameManager.Instance.drums[drumIndex].UnHighlight();
        }

        if (!isMissedByTimeout)
        {
            if (RhythmGameManager.Instance != null)
            {
                RhythmGameManager.Instance.OnDrumHit("Miss", drumIndex);
            }
            StartCoroutine(HitEffect());
        }
        else
        {
            // ⭐ 타임아웃 Miss도 리스트에서 제거
            if (RhythmGameManager.Instance != null)
            {
                RhythmGameManager.Instance.RemoveNote(this);
            }
            Destroy(gameObject);
        }
    }

    IEnumerator HitEffect()
    {
        float duration = 0.2f;
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, t);

            if (noteMaterial != null)
            {
                Color color = noteMaterial.color;
                color.a = 1 - t;
                noteMaterial.color = color;
            }

            yield return null;
        }

        // ⭐ 파괴 전에 리스트에서 제거
        if (RhythmGameManager.Instance != null)
        {
            RhythmGameManager.Instance.RemoveNote(this);
        }

        Destroy(gameObject);
    }

    public float GetDistanceToTarget()
    {
        return Mathf.Abs(transform.position.y - targetPosition.y);
    }

    // ⭐ OnDestroy에서도 안전하게 제거 (이중 안전장치)
    void OnDestroy()
    {
        if (RhythmGameManager.Instance != null)
        {
            RhythmGameManager.Instance.RemoveNote(this);
        }
    }
}
