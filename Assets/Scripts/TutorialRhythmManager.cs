using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialRhythmManager : MonoBehaviour
{
    public static TutorialRhythmManager Instance;

    [Header("드럼 설정")]
    public DrumController[] drums = new DrumController[4];

    [Header("튜토리얼 설정")]
    public float tutorialBPM = 80f; // 느린 BPM
    public int requiredSuccessfulHits = 8; // 성공해야 하는 횟수

    [Header("UI")]
    public Text progressText;

    [Header("게임 상태")]
    private bool isPlaying = false;
    private int successfulHits = 0;
    private int currentBeatIndex = 0;
    private List<SimpleBeat> tutorialBeats;
    private float gameStartTime;
    private TutorialManager tutorialManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        tutorialManager = FindObjectOfType<TutorialManager>();

        // 드럼 검증
        for (int i = 0; i < drums.Length; i++)
        {
            if (drums[i] == null)
            {
                Debug.LogError($"❌ Tutorial Drum {i}이(가) 연결되지 않았습니다!");
            }
        }

        UpdateProgressUI();
    }

    void Update()
    {
        if (!isPlaying) return;

        float currentTime = Time.time - gameStartTime;

        // 비트 처리
        while (currentBeatIndex < tutorialBeats.Count)
        {
            SimpleBeat beat = tutorialBeats[currentBeatIndex];

            if (currentTime >= beat.time)
            {
                // 북 강조
                drums[beat.drumIndex].Highlight();

                // 강조 해제 예약
                StartCoroutine(UnhighlightAfterDuration(beat.drumIndex, 1.0f));

                Debug.Log($"🎵 Tutorial Beat #{currentBeatIndex} | Drum {beat.drumIndex} at {currentTime:F2}s");

                currentBeatIndex++;
            }
            else
            {
                break;
            }
        }

        // 목표 달성 체크
        if (successfulHits >= requiredSuccessfulHits && isPlaying)
        {
            CompleteTutorialRhythm();
        }
    }

    IEnumerator UnhighlightAfterDuration(int drumIndex, float duration)
    {
        yield return new WaitForSeconds(duration);
        drums[drumIndex].UnHighlight();
    }

    // 튜토리얼 리듬 시작
    public void StartTutorialRhythm()
    {
        Debug.Log("🎮 튜토리얼 리듬 게임 시작!");

        // 간단한 패턴 생성
        GenerateSimplePattern();

        isPlaying = true;
        gameStartTime = Time.time;
        currentBeatIndex = 0;
        successfulHits = 0;

        UpdateProgressUI();
    }

    // 간단하고 쉬운 패턴 생성
    void GenerateSimplePattern()
    {
        tutorialBeats = new List<SimpleBeat>();

        float beatInterval = 60f / tutorialBPM;
        float currentTime = 1f; // 1초부터 시작

        // 패턴: 0 -> 1 -> 2 -> 3 -> 0 -> 1 -> 2 -> 3 (순차적으로 반복)
        for (int i = 0; i < 12; i++) // 12개 비트 (여유있게)
        {
            int drumIndex = i % 4; // 0, 1, 2, 3 순서 반복
            tutorialBeats.Add(new SimpleBeat(currentTime, drumIndex));
            currentTime += beatInterval;
        }

        Debug.Log($"✅ 튜토리얼 패턴 생성 완료! 총 {tutorialBeats.Count}개의 비트");
    }

    // 북을 쳤을 때 호출 (DrumController에서)
    public void OnTutorialDrumHit(string judgment, int drumIndex)
    {
        if (!isPlaying) return;

        if (judgment == "Miss")
        {
            Debug.Log($"❌ Miss! 다시 시도하세요!");
        }
        else
        {
            // Perfect, Great, Good 모두 성공으로 인정
            successfulHits++;
            Debug.Log($"✅ 성공! ({successfulHits}/{requiredSuccessfulHits})");

            UpdateProgressUI();
        }
    }

    void UpdateProgressUI()
    {
        if (progressText != null)
        {
            progressText.text = $"성공: {successfulHits}/{requiredSuccessfulHits}";
        }
    }

    void CompleteTutorialRhythm()
    {
        isPlaying = false;

        Debug.Log("🎉 튜토리얼 리듬 완료!");

        // TutorialManager에 완료 알림
        if (tutorialManager != null)
        {
           // tutorialManager.OnDrumTutorialComplete();
        }

        // 모든 드럼 강조 해제
        for (int i = 0; i < drums.Length; i++)
        {
            drums[i].UnHighlight();
        }
    }

    // 튜토리얼 리듬 정지
    public void StopTutorialRhythm()
    {
        isPlaying = false;

        // 모든 드럼 강조 해제
        for (int i = 0; i < drums.Length; i++)
        {
            drums[i].UnHighlight();
        }
    }
}

// 간단한 비트 데이터
[System.Serializable]
public class SimpleBeat
{
    public float time;
    public int drumIndex;

    public SimpleBeat(float time, int drumIndex)
    {
        this.time = time;
        this.drumIndex = drumIndex;
    }
}
