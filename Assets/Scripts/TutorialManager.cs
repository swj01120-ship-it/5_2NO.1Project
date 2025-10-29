using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("Slideshow Settings")]
    public Image currentStepImage;              // 현재 보여줄 이미지
    public Sprite[] stepSprites;                // 모든 단계 이미지들
    //public TextMeshProUGUI pageNumber;          // 페이지 번호 텍스트
    public GameObject prevButton;               // 이전 버튼
    public GameObject nextButton;               // 다음 버튼

    [Header("Animation")]
    public float fadeDuration = 0.3f;           // 페이드 효과 시간

    private int currentStep = 0;                // 현재 단계 (0부터 시작)
    private CanvasGroup imageCanvasGroup;       // 페이드 효과용

    void Start()
    {
        // CanvasGroup 추가 (페이드 효과용)
        if (currentStepImage != null)
        {
            imageCanvasGroup = currentStepImage.GetComponent<CanvasGroup>();
            if (imageCanvasGroup == null)
            {
                imageCanvasGroup = currentStepImage.gameObject.AddComponent<CanvasGroup>();
            }
        }

        // 첫 번째 단계 표시
        UpdateStep();
    }

    // 다음 단계로
    public void NextStep()
    {
        if (currentStep < stepSprites.Length - 1)
        {
            currentStep++;
            StartCoroutine(FadeAndUpdateStep());
        }
    }

    // 이전 단계로
    public void PrevStep()
    {
        if (currentStep > 0)
        {
            currentStep--;
            StartCoroutine(FadeAndUpdateStep());
        }
    }

    // 단계 업데이트 (페이드 효과 포함)
    IEnumerator FadeAndUpdateStep()
    {
        // 페이드 아웃
        yield return StartCoroutine(FadeOut());

        // 이미지 변경
        UpdateStepImage();

        // 페이드 인
        yield return StartCoroutine(FadeIn());
    }

    // 단계 업데이트 (즉시)
    void UpdateStep()
    {
        UpdateStepImage();
    }

    // 이미지와 UI 업데이트
    void UpdateStepImage()
    {
        // 이미지 변경
        if (currentStepImage != null && stepSprites != null && currentStep < stepSprites.Length)
        {
            currentStepImage.sprite = stepSprites[currentStep];
        }

        // 페이지 번호 업데이트
        //if (pageNumber != null && stepSprites != null)
        {
            //pageNumber.text = (currentStep + 1) + " / " + stepSprites.Length;
        }

        // 버튼 활성화/비활성화
        UpdateButtons();
    }

    // 버튼 표시/숨김
    void UpdateButtons()
    {
        // 이전 버튼: 첫 페이지가 아니면 보이기
        if (prevButton != null)
        {
            prevButton.SetActive(currentStep > 0);
        }

        // 다음 버튼: 마지막 페이지가 아니면 보이기
        if (nextButton != null)
        {
            nextButton.SetActive(currentStep < stepSprites.Length - 1);
        }
    }


    // 페이드 아웃 효과
    IEnumerator FadeOut()
    {
        if (imageCanvasGroup == null) yield break;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            imageCanvasGroup.alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            yield return null;
        }

        imageCanvasGroup.alpha = 0;
    }

    // 페이드 인 효과
    IEnumerator FadeIn()
    {
        if (imageCanvasGroup == null) yield break;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            imageCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            yield return null;
        }

        imageCanvasGroup.alpha = 1;
    }

    // 메인 메뉴로 돌아가기
    public void BackToMainMenu()
    {
        Debug.Log("메인 메뉴로 돌아가기");
        LoadingSceneManager.nextScene = "MainMenu";
        SceneManager.LoadScene("LoadingScene");
    }

    // 게임 시작
    public void StartGame()
    {
        Debug.Log("튜토리얼에서 게임 시작");
        LoadingSceneManager.nextScene = "GameScene";
        SceneManager.LoadScene("LoadingScene");
    }


}
