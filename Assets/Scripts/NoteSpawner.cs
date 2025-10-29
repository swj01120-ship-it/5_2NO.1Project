using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab; // 노트 프리팹
    public Transform spawnPoint; // 노트 생성 위치
    public float bpm = 128f; // 음악의 BPM
    public int maxNotes = 100; // 생성할 최대 노트 수

    private float beatInterval; // 한 박자의 시간 간격
    private float nextBeatTime;
    private int noteCount = 0;
    private bool isSpawning = false;

    void Start()
    {
        // BPM을 초 단위로 변환
        beatInterval = 60f / bpm;
        nextBeatTime = Time.time + beatInterval * 2; // 2박자 후 시작
    }

    void Update()
    {
        // 게임 시작 후 노트 생성
        if (isSpawning && Time.time >= nextBeatTime && noteCount < maxNotes)
        {
            SpawnNote();
            nextBeatTime += beatInterval;
        }
    }

    public void StartSpawning()
    {
        isSpawning = true;
    }

    void SpawnNote()
    {
        if (notePrefab != null && spawnPoint != null)
        {
            // 랜덤 X 위치 (-1 ~ 1 범위)
            Vector3 spawnPos = spawnPoint.position;
            spawnPos.x = Random.Range(-1f, 1f);

            Instantiate(notePrefab, spawnPos, Quaternion.identity);
            noteCount++;
        }
    }
}
