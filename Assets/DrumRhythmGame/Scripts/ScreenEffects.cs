using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenEffects : MonoBehaviour
{
    public static ScreenEffects Instance;

    [Header("플래시 효과")]
    public Image flashImage;
    public float flashDuration = 0.1f;

    [Header("카메라 쉐이크")]
    public Camera mainCamera;
    private Vector3 originalCameraPosition;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (mainCamera != null)
        {
            originalCameraPosition = mainCamera.transform.localPosition;
        }
    }

    // 화면 플래시
    public void Flash(Color color, float intensity = 0.5f)
    {
        if (flashImage != null)
        {
            StopAllCoroutines();
            StartCoroutine(FlashCoroutine(color, intensity));
        }
    }

    IEnumerator FlashCoroutine(Color color, float intensity)
    {
        // 페이드 인
        color.a = intensity;
        flashImage.color = color;

        float elapsed = 0f;

        // 페이드 아웃
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(intensity, 0f, elapsed / flashDuration);
            color.a = alpha;
            flashImage.color = color;
            yield return null;
        }

        // 완전히 투명
        color.a = 0f;
        flashImage.color = color;
    }

    // 카메라 쉐이크
    public void CameraShake(float intensity = 0.1f, float duration = 0.2f)
    {
        if (mainCamera != null)
        {
            StartCoroutine(CameraShakeCoroutine(intensity, duration));
        }
    }

    IEnumerator CameraShakeCoroutine(float intensity, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            mainCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 원래 위치로 복귀
        mainCamera.transform.localPosition = originalCameraPosition;
    }
}
