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
            if (accuracy >= 75f) return "S";
            if (accuracy >= 60f) return "A";
            if (accuracy >= 50f) return "B";
            if (accuracy >= 40f) return "C";
            if (accuracy >= 30f) return "D";
            return "F";
        }
    }

    public Color GradeColor
    {
        get
        {
            switch (Grade)
            {
                case "S": return new Color(1f, 0.84f, 0f); // 금색
                case "A": return new Color(0f, 1f, 0f);     // 초록
                case "B": return new Color(0f, 0.5f, 1f);   // 파랑
                case "C": return new Color(1f, 0.65f, 0f);  // 주황
                case "D": return new Color(0.8f, 0.8f, 0.8f); // 회색
                case "F": return new Color(1f, 0f, 0f);     // 빨강
                default: return Color.white;
            }
        }
    }
}
