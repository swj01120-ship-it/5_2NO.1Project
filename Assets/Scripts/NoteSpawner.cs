using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab; // 노트 프리팹
    public Transform spawnPoint; // 노트가 생성될 위치
    public float bpm = 120f; // 음악의 BPM (Beats Per Minute)

    private float beatInterval; // 한 박자의 시간
    private float nextBeatTime;

    void Start()
    {
        // BPM을 시간으로 변환 (60초 / BPM)
        beatInterval = 60f / bpm;
        nextBeatTime = beatInterval;
    }

    void Update()
    {
        // 박자마다 노트 생성
        if (Time.time >= nextBeatTime)
        {
            SpawnNote();
            nextBeatTime += beatInterval;
        }
    }

    void SpawnNote()
    {
        // 약간의 랜덤 X 위치 (선택사항)
        Vector3 spawnPos = spawnPoint.position;
        spawnPos.x += Random.Range(-0.5f, 0.5f);

        Instantiate(notePrefab, spawnPos, Quaternion.identity);
    }
}
