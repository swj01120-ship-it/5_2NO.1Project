using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BeatNote
{
    public float time;        // ����� �ð� (��)
    public int drumIndex;     // � ���� ĥ�� (0~3)
    public float duration;    // ���� ���� �ð� (��)

    public BeatNote(float time, int drumIndex, float duration = 0.3f)
    {
        this.time = time;
        this.drumIndex = drumIndex;
        this.duration = duration;
    }
}
