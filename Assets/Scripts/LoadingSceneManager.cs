using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingSceneManager : MonoBehaviour
{
    public static string nextScene; // 로드할 씬 이름

    public TextMeshProUGUI loadingText;
    public Slider loadingBar; // 선택사항

    void Start()
    {
        // 로딩 시작
        StartCoroutine(LoadSceneAsync());
    }

    
    IEnumerator LoadSceneAsync()
    {
        if (string.IsNullOrEmpty(nextScene))
        {
            nextScene = "GameScene";
        }

        // 점 애니메이션 시작
        StartCoroutine(AnimateDots());

        yield return new WaitForSeconds(2.5f);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextScene);

        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            if (loadingBar != null)
            {
                loadingBar.value = progress;
            }

            yield return null;
        }
    }

    // 점 애니메이션
    IEnumerator AnimateDots()
    {
        string baseText = "Loading";
        int dotCount = 0;

        while (true)
        {
            if (loadingText != null)
            {
                string dots = new string('.', dotCount);
                loadingText.text = baseText + dots;
            }

            dotCount = (dotCount + 1) % 4; // 0, 1, 2, 3 반복
            yield return new WaitForSeconds(0.5f);
        }
    }
}
