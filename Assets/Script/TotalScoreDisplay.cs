using UnityEngine;
using UnityEngine.UI;

public class TotalScoreDisplay : MonoBehaviour
{
    public static TotalScoreDisplay Instance;

    public Text totalScoreText;  // UnityEngine.UI.Text
    private int totalScore = 0;
    private int totalCombo = 0;  // ������ �޺� �� ���� ����

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �� ��ȯ �� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ���� �߰� �Լ�
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

    // �޺� �� ������Ʈ �Լ�
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

    // ������ �޺��� �Բ� UI ������Ʈ
    private void UpdateScoreText()
    {
        if (totalScoreText != null)
        {
            string comboDisplay = totalCombo > 0 ? $"Combo: {totalCombo}" : "";
            totalScoreText.text = $"Total Score: {totalScore}\n{comboDisplay}";

            totalScoreText.color = Color.white;           // �۾� ������ �������
            totalScoreText.fontStyle = FontStyle.Bold;    // �۾��� ����

            Debug.Log($"UpdateScoreText updated UI: {totalScoreText.text}");
        }
        else
        {
            Debug.LogWarning("TotalScoreText UI component is not assigned.");
        }
    }
}
