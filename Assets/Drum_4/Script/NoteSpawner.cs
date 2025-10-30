using System.Collections;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoteData
{
    public int lane; // 0=A, 1=S, 2=D, 3=F
    public float time; // 음악 시작 후 몇 초에 생성되는지
}

public class NoteSpawner : MonoBehaviour
{
    [Header("Note Settings")]
    public GameObject notePrefab;
    public Transform[] spawnPoints; // 4개 레인의 생성 위치
    public List<NoteData> noteChart = new List<NoteData>(); // 노트 차트 데이터

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

        // 다음 노트 생성 체크
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

    // 인스펙터에서 노트 차트 자동 생성 (테스트용)
    [ContextMenu("Generate Test Chart")]
    void GenerateTestChart()
    {
        noteChart.Clear();

        // 4비트마다 랜덤 레인에 노트 생성 (테스트용)
        float beatInterval = 60f / songBPM; // 한 비트의 길이

        for (int i = 0; i < 50; i++) // 50개 노트 생성
        {
            NoteData data = new NoteData();
            data.lane = Random.Range(0, 4); // 랜덤 레인
            data.time = i * beatInterval; // 매 비트마다
            noteChart.Add(data);
        }

        Debug.Log("테스트 차트 생성 완료: " + noteChart.Count + "개 노트");
    }
}
