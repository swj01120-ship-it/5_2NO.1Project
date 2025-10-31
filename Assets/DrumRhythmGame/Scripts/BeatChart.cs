using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Beat Chart", menuName = "Rhythm Game/Beat Chart")]
public class BeatChart : ScriptableObject
{
    public string chartName = "새로운 차트";
    public float bpm = 128f;
    public List<BeatNote> beats = new List<BeatNote>();

    [Header("자동 생성 설정")]
    public float songLength = 98f; // 곡 길이 (초)
    public float noteDensity = 1f;  // 비트 밀도 (1 = 1비트마다 1개)

    // Inspector에서 우클릭으로 테스트 차트 생성
    [ContextMenu("Generate Test Chart")]
    public void GenerateTestChart()
    {
        beats.Clear();

        float beatInterval = 60f / bpm; // 1비트당 시간
        float currentTime = 2f; // 2초부터 시작

        System.Random random = new System.Random();

        while (currentTime < songLength)
        {
            // 랜덤하게 1~3개의 드럼 선택
            int numberOfDrums = random.Next(1, 4);

            for (int i = 0; i < numberOfDrums; i++)
            {
                int randomDrum = random.Next(0, 4);
                beats.Add(new BeatNote(currentTime, randomDrum, 0.4f));
            }

            currentTime += beatInterval / noteDensity;
        }

        Debug.Log($"✅ 테스트 차트 생성 완료! 총 {beats.Count}개의 비트");
    }

    [ContextMenu("Show Chart Info")]
    public void ShowChartInfo()
    {
        Debug.Log($"=== 차트 정보 ===");
        Debug.Log($"차트 이름: {chartName}");
        Debug.Log($"BPM: {bpm}");
        Debug.Log($"총 비트 수: {beats.Count}");
        if (beats.Count > 0)
        {
            Debug.Log($"첫 비트 시간: {beats[0].time}초");
            Debug.Log($"마지막 비트 시간: {beats[beats.Count - 1].time}초");
        }
        Debug.Log($"================");
    }

    [ContextMenu("Clear Chart")]
    public void ClearChart()
    {
        beats.Clear();
        Debug.Log("차트가 비워졌습니다.");
    }
}
