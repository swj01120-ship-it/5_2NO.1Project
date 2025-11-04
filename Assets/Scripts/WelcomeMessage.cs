using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WelcomeMessage : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject welcomePanel;
    [SerializeField] private Text welcomeText;

    [Header("설정")]
    [SerializeField] private float displayDuration = 8f; // 메시지 표시 시간
    [SerializeField] private float fadeOutDuration = 1f; // 페이드 아웃 시간
    [SerializeField] private bool autoHide = true; // 자동으로 숨김

    [Header("메시지")]
    [TextArea(3, 10)]
    [SerializeField] private string welcomeMessage = "어서오세요!\n\n마우스로 시야를 변경하고, W, A, S, D 키를 활용하여 움직일 수 있습니다.\n\n이제, 주변을 돌아보고 귀여운 호랑이 선생님을 찾아\n가까이 다가가서 \"E\" 키를 눌러보세요!";

    private CanvasGroup canvasGroup;
    private bool isShowing = true;

    void Start()
    {
        // CanvasGroup 가져오기 (페이드 효과용)
        if (welcomePanel != null)
        {
            canvasGroup = welcomePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = welcomePanel.AddComponent<CanvasGroup>();
            }
        }

        // 메시지 설정
        if (welcomeText != null)
        {
            welcomeText.text = welcomeMessage;
        }

        // 패널 표시
        if (welcomePanel != null)
        {
            welcomePanel.SetActive(true);
            canvasGroup.alpha = 1f;
        }

        // 자동 숨김
        if (autoHide)
        {
            StartCoroutine(AutoHideMessage());
        }
    }

    void Update()
    {
        // E 키를 누르면 즉시 숨김
        if (Input.GetKeyDown(KeyCode.E) && isShowing)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }

        // ESC나 Space로도 숨길 수 있음
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Space)) && isShowing)
        {
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }
    }

    IEnumerator AutoHideMessage()
    {
        // 지정된 시간 대기
        yield return new WaitForSeconds(displayDuration);

        // 페이드 아웃
        yield return StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        if (!isShowing) yield break;

        isShowing = false;

        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);

            if (canvasGroup != null)
            {
                canvasGroup.alpha = alpha;
            }

            yield return null;
        }

        // 완전히 숨김
        if (welcomePanel != null)
        {
            welcomePanel.SetActive(false);
        }
    }

    // 외부에서 수동으로 숨기기
    public void HideMessage()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }
}
