using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrumController : MonoBehaviour
{
    [Header("드럼 설정")]
    public int drumIndex;
    public KeyCode drumKey;

    [Header("색상 설정")]
    public Color normalColor = Color.white;
    public Color highlightColor = Color.red;
    public Color hitColor = Color.yellow;

    private Renderer drumRenderer;
    private Material drumMaterial;

    private bool isHighlighted = false;
    private float highlightStartTime;

    [Header("판정 윈도우 (초 단위)")]
    public float perfectWindow = 0.03f;
    public float greatWindow = 0.12f;
    public float goodWindow = 1.2f;

    [Header("효과")]
    public ParticleSystem hitParticle;

    private Vector3 originalScale;

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

        originalScale = transform.localScale;

        if (DifficultySettings.Instance != null)
        {
            DifficultySettings.Instance.GetJudgmentWindows(out perfectWindow, out greatWindow, out goodWindow);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(drumKey))
        {
            HitDrum();
        }
    }

    public void Highlight()
    {
        isHighlighted = true;
        highlightStartTime = Time.time;
        SetColor(highlightColor);
        ResetScale();
    }

    public void UnHighlight()
    {
        isHighlighted = false;
        SetColor(normalColor);
        ResetScale();
    }

    void HitDrum()
    {
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
