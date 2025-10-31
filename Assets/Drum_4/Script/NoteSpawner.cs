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

    [Header("Auto Start (테스트용)")]
    public bool autoStart = true; // 자동 시작 여부

    private float songPosition = 0f;
    private int noteIndex = 0;
    private bool isPlaying = false;

    void Start()
    {
        // 시작 시 검증
        ValidateSettings();

        // 자동 시작
        if (autoStart)
        {
            StartSpawning();
            Debug.Log("🎮 자동 시작! 노트 스포닝 시작!");
        }
    }

    void Update()
    {
        // 스페이스바로 수동 시작 (자동 시작이 꺼져있을 때)
        if (!isPlaying && Input.GetKeyDown(KeyCode.Space))
        {
            StartSpawning();
            Debug.Log("🎮 스페이스바 입력! 노트 스포닝 시작!");
        }

        if (!isPlaying) return;

        songPosition += Time.deltaTime;

        // 다음 노트 생성 체크
        while (noteIndex < noteChart.Count && noteChart[noteIndex].time <= songPosition)
        {
            Debug.Log($"📝 노트 #{noteIndex} 스폰 | Lane: {noteChart[noteIndex].lane}, Time: {noteChart[noteIndex].time:F2}");
            SpawnNote(noteChart[noteIndex].lane);
            noteIndex++;
        }
    }

    public void StartSpawning()
    {
        isPlaying = true;
        songPosition = 0f;
        noteIndex = 0;
        Debug.Log($"✅ 스포닝 시작! 총 노트 수: {noteChart.Count}");
    }

    void SpawnNote(int lane)
    {
        if (notePrefab == null)
        {
            Debug.LogError("❌ Note Prefab이 할당되지 않았습니다!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("❌ Spawn Points가 할당되지 않았습니다!");
            return;
        }

        if (lane >= spawnPoints.Length)
        {
            Debug.LogError($"❌ 잘못된 레인 번호: {lane} (최대: {spawnPoints.Length - 1})");
            return;
        }

        // 노트 생성
        GameObject noteObj = Instantiate(notePrefab, spawnPoints[lane].position, Quaternion.identity);
        noteObj.name = $"Note_Lane{lane}";

        Note note = noteObj.GetComponent<Note>();

        if (note != null)
        {
            note.lane = lane;
            note.speed = noteSpeed;
            Debug.Log($"✅ 노트 생성 성공! Lane: {lane}, 위치: {spawnPoints[lane].position}");
        }
        else
        {
            Debug.LogError("❌ Note 프리팹에 Note.cs 스크립트가 없습니다!");
        }
    }

    // 설정 검증
    void ValidateSettings()
    {
        Debug.Log("=== NoteSpawner 설정 검증 ===");

        if (notePrefab == null)
            Debug.LogError("❌ Note Prefab이 비어있습니다!");
        else
            Debug.Log($"✅ Note Prefab: {notePrefab.name}");

        if (spawnPoints == null || spawnPoints.Length == 0)
            Debug.LogError("❌ Spawn Points가 비어있습니다!");
        else
        {
            Debug.Log($"✅ Spawn Points: {spawnPoints.Length}개");
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnPoints[i] == null)
                    Debug.LogError($"❌ Spawn Point [{i}]가 비어있습니다!");
                else
                    Debug.Log($"  - Lane {i}: {spawnPoints[i].position}");
            }
        }

        if (noteChart == null || noteChart.Count == 0)
            Debug.LogWarning("⚠️ Note Chart가 비어있습니다! 'Generate Test Chart'를 실행하세요.");
        else
            Debug.Log($"✅ Note Chart: {noteChart.Count}개 노트");

        Debug.Log("===========================");
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

        Debug.Log($"✅ 테스트 차트 생성 완료: {noteChart.Count}개 노트");
    }

    // 테스트: 즉시 노트 1개 생성
    [ContextMenu("Test Spawn Single Note")]
    void TestSpawnSingleNote()
    {
        Debug.Log("테스트: Lane 0에 노트 1개 생성");
        SpawnNote(0);
    }
}
