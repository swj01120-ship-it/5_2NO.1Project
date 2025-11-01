using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultScreenManager : MonoBehaviour
{
    public static ResultScreenManager Instance;

    [Header("결과 화면 UI")]
    public GameObject resultPanel;

    [Header("점수 텍스트")]
    public Text scoreText;
    public Text comboText;
    public Text accuracyText;
    public Text gradeText;

    [Header("판정 통계 텍스트")]
    public Text perfectText;
    public Text greatText;
    public Text goodText;
    public Text missText;

    [Header("버튼")]
    public Button retryButton;
    public Button mainMenuButton;

    private GameResult currentResult;

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
        // 결과 화면 초기에는 숨기기
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }

        // 버튼 이벤트 연결
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    public void ShowResult(GameResult result)
    {
        currentResult = result;

        // 결과 화면 표시
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }

        // 점수 표시
        if (scoreText != null)
            scoreText.text = $"점수: {result.finalScore}";

        if (comboText != null)
            comboText.text = $"최대 콤보: {result.maxCombo}";

        if (accuracyText != null)
            accuracyText.text = $"정확도: {result.Accuracy:F2}%";

        if (gradeText != null)
        {
            gradeText.text = result.Grade;
            gradeText.color = result.GradeColor;
        }

        // 판정 통계 표시
        if (perfectText != null)
            perfectText.text = $"Perfect: {result.perfectCount}";

        if (greatText != null)
            greatText.text = $"Great: {result.greatCount}";

        if (goodText != null)
            goodText.text = $"Good: {result.goodCount}";

        if (missText != null)
            missText.text = $"Miss: {result.missCount}";

        Debug.Log($"🏆 게임 결과:");
        Debug.Log($"   점수: {result.finalScore}");
        Debug.Log($"   최대 콤보: {result.maxCombo}");
        Debug.Log($"   정확도: {result.Accuracy:F2}%");
        Debug.Log($"   등급: {result.Grade}");
        Debug.Log($"   Perfect: {result.perfectCount}, Great: {result.greatCount}, Good: {result.goodCount}, Miss: {result.missCount}");
    }

    public void RetryGame()
    {
        Debug.Log("🔄 게임 다시하기");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Debug.Log("🏠 메인 메뉴로");
        SceneManager.LoadScene("MainMenu");
    }
}
