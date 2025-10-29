using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteSpawner : MonoBehaviour
{
    public GameObject notePrefab; // ��Ʈ ������
    public Transform spawnPoint; // ��Ʈ ���� ��ġ
    public float bpm = 128f; // ������ BPM
    public int maxNotes = 100; // ������ �ִ� ��Ʈ ��

    private float beatInterval; // �� ������ �ð� ����
    private float nextBeatTime;
    private int noteCount = 0;
    private bool isSpawning = false;

    void Start()
    {
        // BPM�� �� ������ ��ȯ
        beatInterval = 60f / bpm;
        nextBeatTime = Time.time + beatInterval * 2; // 2���� �� ����
    }

    void Update()
    {
        // ���� ���� �� ��Ʈ ����
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
            // ���� X ��ġ (-1 ~ 1 ����)
            Vector3 spawnPos = spawnPoint.position;
            spawnPos.x = Random.Range(-1f, 1f);

            Instantiate(notePrefab, spawnPos, Quaternion.identity);
            noteCount++;
        }
    }
}
