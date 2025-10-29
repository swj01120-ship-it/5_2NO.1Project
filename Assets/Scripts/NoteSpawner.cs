using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab; // ��Ʈ ������
    public Transform spawnPoint; // ��Ʈ�� ������ ��ġ
    public float bpm = 120f; // ������ BPM (Beats Per Minute)

    private float beatInterval; // �� ������ �ð�
    private float nextBeatTime;

    void Start()
    {
        // BPM�� �ð����� ��ȯ (60�� / BPM)
        beatInterval = 60f / bpm;
        nextBeatTime = beatInterval;
    }

    void Update()
    {
        // ���ڸ��� ��Ʈ ����
        if (Time.time >= nextBeatTime)
        {
            SpawnNote();
            nextBeatTime += beatInterval;
        }
    }

    void SpawnNote()
    {
        // �ణ�� ���� X ��ġ (���û���)
        Vector3 spawnPos = spawnPoint.position;
        spawnPos.x += Random.Range(-0.5f, 0.5f);

        Instantiate(notePrefab, spawnPos, Quaternion.identity);
    }
}
