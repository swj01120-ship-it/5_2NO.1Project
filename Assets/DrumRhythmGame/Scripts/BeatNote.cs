using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BeatNote
{
    public float time;        // 곡에서의 시간 (초)
    public int drumIndex;     // 어떤 북을 칠지 (0~3)
    public float duration;    // 강조 지속 시간 (초)

    public BeatNote(float time, int drumIndex, float duration = 0.3f)
    {
        this.time = time;
        this.drumIndex = drumIndex;
        this.duration = duration;
    }
}
