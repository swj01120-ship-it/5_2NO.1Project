using System.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoteData
{
    public int lane; // 0=A, 1=S, 2=D, 3=F
    public float time; // ���� ���� �� �� �ʿ� �����Ǵ���
}

public class NoteSpawner : MonoBehaviour
{
    [Header("Note Settings")]
    public GameObject notePrefab;
    public Transform[] spawnPoints; // 4�� ������ ���� ��ġ
    public List<NoteData> noteChart = new List<NoteData>(); // ��Ʈ ��Ʈ ������

    [Header("Timing")]
    public float songBPM = 120f;
    public float noteSpeed = 5f;

    private float songPosition = 0f;
    private int noteIndex = 0;
    private bool isPlaying = false;

    void Update()
    {
        if (!isPlaying) return;

        songPosition += Time.deltaTime;

        // ���� ��Ʈ ���� üũ
        while (noteIndex < noteChart.Count && noteChart[noteIndex].time <= songPosition)
        {
            SpawnNote(noteChart[noteIndex].lane);
            noteIndex++;
        }
    }

    public void StartSpawning()
    {
        isPlaying = true;
        songPosition = 0f;
        noteIndex = 0;
    }

    void SpawnNote(int lane)
    {
        if (notePrefab == null || spawnPoints == null || lane >= spawnPoints.Length)
            return;

        GameObject noteObj = Instantiate(notePrefab, spawnPoints[lane].position, Quaternion.identity);
        Note note = noteObj.GetComponent<Note>();

        if (note != null)
        {
            note.lane = lane;
            note.speed = noteSpeed;
        }
    }

    // �ν����Ϳ��� ��Ʈ ��Ʈ �ڵ� ���� (�׽�Ʈ��)
    [ContextMenu("Generate Test Chart")]
    void GenerateTestChart()
    {
        noteChart.Clear();

        // 4��Ʈ���� ���� ���ο� ��Ʈ ���� (�׽�Ʈ��)
        float beatInterval = 60f / songBPM; // �� ��Ʈ�� ����

        for (int i = 0; i < 50; i++) // 50�� ��Ʈ ����
        {
            NoteData data = new NoteData();
            data.lane = Random.Range(0, 4); // ���� ����
            data.time = i * beatInterval; // �� ��Ʈ����
            noteChart.Add(data);
        }

        Debug.Log("�׽�Ʈ ��Ʈ ���� �Ϸ�: " + noteChart.Count + "�� ��Ʈ");
    }
}
