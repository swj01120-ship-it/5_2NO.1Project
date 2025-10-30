using UnityEngine;
using UnityEngine.UI;

public class TotalScoreDisplay : MonoBehaviour
{
    public static TotalScoreDisplay Instance;

    public Text totalScoreText;  // UnityEngine.UI.Text
    private int totalScore = 0;
    private int totalCombo = 0;  // 누적된 콤보 수 저장 변수

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 전환 시 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 점수 추가 함수
    public void AddScore(int score)
    {
        if (score < 0)
        {
            Debug.LogWarning("AddScore called with negative value: " + score);
            return;
        }

        totalScore += score;
        Debug.Log($"AddScore called. New totalScore: {totalScore}");
        UpdateScoreText();
    }

    // 콤보 수 업데이트 함수
    public void UpdateCombo(int combo)
    {
        if (combo < 0)
        {
            Debug.LogWarning("UpdateCombo called with negative value: " + combo);
            return;
        }

        totalCombo = combo;
        Debug.Log($"UpdateCombo called. New totalCombo: {totalCombo}");
        UpdateScoreText();
    }

    // 점수와 콤보를 함께 UI 업데이트
    private void UpdateScoreText()
    {
        if (totalScoreText != null)
        {
            string comboDisplay = totalCombo > 0 ? $"Combo: {totalCombo}" : "";
            totalScoreText.text = $"Total Score: {totalScore}\n{comboDisplay}";

            totalScoreText.color = Color.white;           // 글씨 색깔을 흰색으로
            totalScoreText.fontStyle = FontStyle.Bold;    // 글씨를 굵게

            Debug.Log($"UpdateScoreText updated UI: {totalScoreText.text}");
        }
        else
        {
            Debug.LogWarning("TotalScoreText UI component is not assigned.");
        }
    }
}
