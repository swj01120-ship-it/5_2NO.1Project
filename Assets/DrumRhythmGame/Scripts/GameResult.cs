using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameResult
{
    public int finalScore;
    public int maxCombo;
    public int perfectCount;
    public int greatCount;
    public int goodCount;
    public int missCount;

    public int TotalNotes
    {
        get { return perfectCount + greatCount + goodCount + missCount; }
    }

    public float Accuracy
    {
        get
        {
            if (TotalNotes == 0) return 0f;
            float totalPoints = (perfectCount * 100f) + (greatCount * 70f) + (goodCount * 40f);
            float maxPoints = TotalNotes * 100f;
            return (totalPoints / maxPoints) * 100f;
        }
    }

    public string Grade
    {
        get
        {
            float accuracy = Accuracy;
            if (accuracy >= 95f) return "S";
            if (accuracy >= 90f) return "A";
            if (accuracy >= 80f) return "B";
            if (accuracy >= 70f) return "C";
            if (accuracy >= 60f) return "D";
            return "F";
        }
    }

    public Color GradeColor
    {
        get
        {
            switch (Grade)
            {
                case "S": return new Color(1f, 0.84f, 0f); // �ݻ�
                case "A": return new Color(0f, 1f, 0f);     // �ʷ�
                case "B": return new Color(0f, 0.5f, 1f);   // �Ķ�
                case "C": return new Color(1f, 0.65f, 0f);  // ��Ȳ
                case "D": return new Color(0.8f, 0.8f, 0.8f); // ȸ��
                case "F": return new Color(1f, 0f, 0f);     // ����
                default: return Color.white;
            }
        }
    }
}
