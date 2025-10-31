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
    public float chartStartDelay = 2f; // 노래 시작 후 차트 시작까지 딜레이 (초)

    [Header("Chart Generation Settings")]
    public float songLengthInSeconds = 180f; // 노래 길이 (초) - 기본 3분
    public float noteDensity = 1f; // 노트 밀도 (1 = 1비트마다 1개)
    public bool fillEntireSong = true; // 노래 끝까지 노트 생성

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

        // 모든 노트 생성 완료 시 로그
        if (noteIndex >= noteChart.Count && noteChart.Count > 0)
        {
            Debug.Log($"🎉 모든 노트 생성 완료! 총 {noteChart.Count}개");
            isPlaying = false; // 더 이상 체크하지 않음
        }
    }

    public void StartSpawning()
    {
        isPlaying = true;
        songPosition = 0f;
        noteIndex = 0;
        Debug.Log($"✅ 스포닝 시작! 총 노트 수: {noteChart.Count}");
        if (noteChart.Count > 0)
        {
            Debug.Log($"   첫 노트: {noteChart[0].time:F2}초");
            Debug.Log($"   마지막 노트: {noteChart[noteChart.Count - 1].time:F2}초");
        }
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
            }
        }

        if (noteChart == null || noteChart.Count == 0)
            Debug.LogWarning("⚠️ Note Chart가 비어있습니다! 'Generate Test Chart'를 실행하세요.");
        else
            Debug.Log($"✅ Note Chart: {noteChart.Count}개 노트");

        Debug.Log($"✅ Chart Start Delay: {chartStartDelay}초");
        Debug.Log("===========================");
    }

    // 인스펙터에서 노트 차트 자동 생성 (테스트용)
    [ContextMenu("Generate Test Chart")]
    void GenerateTestChart()
    {
        noteChart.Clear();

        float beatInterval = 60f / songBPM; // 한 비트의 길이 (초)
        float notesInterval = beatInterval / noteDensity; // 노트 간격

        // 노래 길이에 맞춰 노트 개수 계산
        // chartStartDelay부터 시작해서 songLengthInSeconds까지
        float chartDuration = fillEntireSong ? songLengthInSeconds : songLengthInSeconds - chartStartDelay;
        int totalNotes = Mathf.FloorToInt(chartDuration / notesInterval);

        Debug.Log($"📊 차트 생성 중...");
        Debug.Log($"  - 노래 길이: {songLengthInSeconds}초");
        Debug.Log($"  - 차트 시작 딜레이: {chartStartDelay}초");
        Debug.Log($"  - BPM: {songBPM}");
        Debug.Log($"  - 비트 간격: {beatInterval:F3}초");
        Debug.Log($"  - 노트 밀도: {noteDensity}");
        Debug.Log($"  - 생성될 노트 수: {totalNotes}개");

        for (int i = 0; i < totalNotes; i++)
        {
            NoteData data = new NoteData();
            data.lane = Random.Range(0, 4); // 랜덤 레인
            // 차트 시작 딜레이 추가 (2초부터 시작)
            data.time = chartStartDelay + (i * notesInterval);
            noteChart.Add(data);
        }

        if (noteChart.Count > 0)
        {
            Debug.Log($"✅ 테스트 차트 생성 완료: {noteChart.Count}개 노트");
            Debug.Log($"   첫 노트 시간: {noteChart[0].time:F2}초");
            Debug.Log($"   마지막 노트 시간: {noteChart[noteChart.Count - 1].time:F2}초");
        }
    }

    // 노래 길이에 딱 맞는 차트 생성
    [ContextMenu("Generate Chart (Exact Song Length)")]
    void GenerateExactChart()
    {
        noteChart.Clear();

        float beatInterval = 60f / songBPM;

        // chartStartDelay부터 songLengthInSeconds까지 모든 비트에 노트 생성
        float currentTime = chartStartDelay;
        int noteCount = 0;

        while (currentTime <= songLengthInSeconds)
        {
            NoteData data = new NoteData();
            data.lane = Random.Range(0, 4);
            data.time = currentTime;
            noteChart.Add(data);

            currentTime += beatInterval;
            noteCount++;
        }

        Debug.Log($"📊 정확한 길이 차트 생성 중...");
        Debug.Log($"  - 노래 길이: {songLengthInSeconds}초");
        Debug.Log($"  - 차트 시작: {chartStartDelay}초");
        Debug.Log($"✅ 정확한 차트 생성 완료: {noteChart.Count}개 노트");
        if (noteChart.Count > 0)
        {
            Debug.Log($"   첫 노트: {noteChart[0].time:F2}초");
            Debug.Log($"   마지막 노트: {noteChart[noteChart.Count - 1].time:F2}초");
        }
    }

    // 패턴이 있는 차트 생성 (더 재미있음!)
    [ContextMenu("Generate Pattern Chart")]
    void GeneratePatternChart()
    {
        noteChart.Clear();

        float beatInterval = 60f / songBPM;
        float currentTime = chartStartDelay;
        int beatIndex = 0;

        Debug.Log($"🎵 패턴 차트 생성 중...");
        Debug.Log($"  - 시작 시간: {chartStartDelay}초");
        Debug.Log($"  - 종료 시간: {songLengthInSeconds}초");

        while (currentTime <= songLengthInSeconds)
        {
            NoteData data = new NoteData();

            // 4비트마다 패턴 변경
            int patternIndex = (beatIndex / 4) % 4;

            switch (patternIndex)
            {
                case 0: // 왼쪽에서 오른쪽으로
                    data.lane = beatIndex % 4;
                    break;
                case 1: // 오른쪽에서 왼쪽으로
                    data.lane = 3 - (beatIndex % 4);
                    break;
                case 2: // 양 끝부터
                    data.lane = (beatIndex % 2 == 0) ? 0 : 3;
                    break;
                case 3: // 랜덤
                    data.lane = Random.Range(0, 4);
                    break;
            }

            data.time = currentTime;
            noteChart.Add(data);

            currentTime += beatInterval;
            beatIndex++;
        }

        Debug.Log($"✅ 패턴 차트 생성 완료: {noteChart.Count}개 노트");
        if (noteChart.Count > 0)
        {
            Debug.Log($"   첫 노트: {noteChart[0].time:F2}초");
            Debug.Log($"   마지막 노트: {noteChart[noteChart.Count - 1].time:F2}초");
        }
    }

    // 밀도가 점점 증가하는 차트
    [ContextMenu("Generate Progressive Chart")]
    void GenerateProgressiveChart()
    {
        noteChart.Clear();

        float beatInterval = 60f / songBPM;
        float currentTime = chartStartDelay;
        float currentDensity = 0.5f; // 시작은 느리게
        int section = 0;

        Debug.Log($"📈 점진적 난이도 차트 생성 중...");

        while (currentTime <= songLengthInSeconds)
        {
            // 30초마다 밀도 증가
            if (currentTime - chartStartDelay > section * 30f)
            {
                currentDensity += 0.25f;
                section++;
                Debug.Log($"  - {currentTime:F0}초: 밀도 {currentDensity}");
            }

            NoteData data = new NoteData();
            data.lane = Random.Range(0, 4);
            data.time = currentTime;
            noteChart.Add(data);

            currentTime += beatInterval / currentDensity;
        }

        Debug.Log($"✅ 점진적 차트 생성 완료: {noteChart.Count}개 노트");
        if (noteChart.Count > 0)
        {
            Debug.Log($"   첫 노트: {noteChart[0].time:F2}초");
            Debug.Log($"   마지막 노트: {noteChart[noteChart.Count - 1].time:F2}초");
        }
    }

    // Spawn Points 자동 설정 (Inspector에서 실행)
    [ContextMenu("Auto Setup Spawn Points")]
    void AutoSetupSpawnPoints()
    {
        // 4개의 Spawn Point를 자동으로 생성
        Transform spawnParent = transform.Find("SpawnPoints");

        if (spawnParent == null)
        {
            GameObject spawnParentObj = new GameObject("SpawnPoints");
            spawnParentObj.transform.SetParent(transform);
            spawnParent = spawnParentObj.transform;
        }

        // 기존 Spawn Points 삭제
        while (spawnParent.childCount > 0)
        {
            DestroyImmediate(spawnParent.GetChild(0).gameObject);
        }

        spawnPoints = new Transform[4];

        // 화면 상단에 4개의 Spawn Point 생성
        float spawnHeight = 8f; // Y 위치
        float laneWidth = 2f; // 레인 간격
        float startX = -3f; // 시작 X 위치

        for (int i = 0; i < 4; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_Lane{i}");
            spawnPoint.transform.SetParent(spawnParent);
            spawnPoint.transform.position = new Vector3(startX + (i * laneWidth), spawnHeight, 0);
            spawnPoints[i] = spawnPoint.transform;

            Debug.Log($"✅ Spawn Point {i} 생성: {spawnPoint.transform.position}");
        }

        Debug.Log($"✅ Spawn Points 자동 설정 완료! (Y 높이: {spawnHeight})");
    }

    // 테스트: 즉시 노트 1개 생성
    [ContextMenu("Test Spawn Single Note")]
    void TestSpawnSingleNote()
    {
        Debug.Log("테스트: Lane 0에 노트 1개 생성");
        SpawnNote(0);
    }

    // 현재 차트 정보 출력
    [ContextMenu("Show Chart Info")]
    void ShowChartInfo()
    {
        if (noteChart == null || noteChart.Count == 0)
        {
            Debug.LogWarning("⚠️ 차트가 비어있습니다!");
            return;
        }

        Debug.Log("=== 차트 정보 ===");
        Debug.Log($"총 노트 수: {noteChart.Count}");
        Debug.Log($"첫 노트 시간: {noteChart[0].time:F2}초");
        Debug.Log($"마지막 노트 시간: {noteChart[noteChart.Count - 1].time:F2}초");
        Debug.Log($"차트 길이: {noteChart[noteChart.Count - 1].time - noteChart[0].time:F2}초");

        // 레인별 노트 수
        int[] laneCount = new int[4];
        foreach (var note in noteChart)
        {
            if (note.lane >= 0 && note.lane < 4)
                laneCount[note.lane]++;
        }

        for (int i = 0; i < 4; i++)
        {
            Debug.Log($"Lane {i}: {laneCount[i]}개 노트");
        }
        Debug.Log("================");
    }
}