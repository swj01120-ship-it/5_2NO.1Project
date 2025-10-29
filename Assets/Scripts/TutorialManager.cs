using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Header("Slideshow Settings")]
    public Image currentStepImage;              // ���� ������ �̹���
    public Sprite[] stepSprites;                // ��� �ܰ� �̹�����
    //public TextMeshProUGUI pageNumber;          // ������ ��ȣ �ؽ�Ʈ
    public GameObject prevButton;               // ���� ��ư
    public GameObject nextButton;               // ���� ��ư

    [Header("Animation")]
    public float fadeDuration = 0.3f;           // ���̵� ȿ�� �ð�

    private int currentStep = 0;                // ���� �ܰ� (0���� ����)
    private CanvasGroup imageCanvasGroup;       // ���̵� ȿ����

    void Start()
    {
        // CanvasGroup �߰� (���̵� ȿ����)
        if (currentStepImage != null)
        {
            imageCanvasGroup = currentStepImage.GetComponent<CanvasGroup>();
            if (imageCanvasGroup == null)
            {
                imageCanvasGroup = currentStepImage.gameObject.AddComponent<CanvasGroup>();
            }
        }

        // ù ��° �ܰ� ǥ��
        UpdateStep();
    }

    // ���� �ܰ��
    public void NextStep()
    {
        if (currentStep < stepSprites.Length - 1)
        {
            currentStep++;
            StartCoroutine(FadeAndUpdateStep());
        }
    }

    // ���� �ܰ��
    public void PrevStep()
    {
        if (currentStep > 0)
        {
            currentStep--;
            StartCoroutine(FadeAndUpdateStep());
        }
    }

    // �ܰ� ������Ʈ (���̵� ȿ�� ����)
    IEnumerator FadeAndUpdateStep()
    {
        // ���̵� �ƿ�
        yield return StartCoroutine(FadeOut());

        // �̹��� ����
        UpdateStepImage();

        // ���̵� ��
        yield return StartCoroutine(FadeIn());
    }

    // �ܰ� ������Ʈ (���)
    void UpdateStep()
    {
        UpdateStepImage();
    }

    // �̹����� UI ������Ʈ
    void UpdateStepImage()
    {
        // �̹��� ����
        if (currentStepImage != null && stepSprites != null && currentStep < stepSprites.Length)
        {
            currentStepImage.sprite = stepSprites[currentStep];
        }

        // ������ ��ȣ ������Ʈ
        //if (pageNumber != null && stepSprites != null)
        {
            //pageNumber.text = (currentStep + 1) + " / " + stepSprites.Length;
        }

        // ��ư Ȱ��ȭ/��Ȱ��ȭ
        UpdateButtons();
    }

    // ��ư ǥ��/����
    void UpdateButtons()
    {
        // ���� ��ư: ù �������� �ƴϸ� ���̱�
        if (prevButton != null)
        {
            prevButton.SetActive(currentStep > 0);
        }

        // ���� ��ư: ������ �������� �ƴϸ� ���̱�
        if (nextButton != null)
        {
            nextButton.SetActive(currentStep < stepSprites.Length - 1);
        }
    }


    // ���̵� �ƿ� ȿ��
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

    // ���̵� �� ȿ��
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

    // ���� �޴��� ���ư���
    public void BackToMainMenu()
    {
        Debug.Log("���� �޴��� ���ư���");
        LoadingSceneManager.nextScene = "MainMenu";
        SceneManager.LoadScene("LoadingScene");
    }

    // ���� ����
    public void StartGame()
    {
        Debug.Log("Ʃ�丮�󿡼� ���� ����");
        LoadingSceneManager.nextScene = "GameScene";
        SceneManager.LoadScene("LoadingScene");
    }


}
