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
    [Range(0f, 1f)]
    public float randomness = 0.8f; // 0: 완전 순차, 1: 완전 랜덤

    [Header("UI")]
    public Text progressText; // "성공: 5/8" 표시용
    public Text countText;    // 큰 숫자 "5" 표시용 (선택사항)

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
        // 패턴이 끝났지만 아직 목표 달성 안 했으면 패턴 반복
        if (currentBeatIndex >= tutorialBeats.Count && successfulHits < requiredSuccessfulHits)
        {
            Debug.Log("🔄 패턴 반복! 계속 연습하세요!");
            RestartPattern();
        }

        // 목표 달성 체크
        if (successfulHits >= requiredSuccessfulHits && isPlaying)
        {
            CompleteTutorialRhythm();
        }
    }

    // 패턴 재시작 (시간만 리셋)
    void RestartPattern()
    {
        currentBeatIndex = 0;
        gameStartTime = Time.time;

        // 새 랜덤 패턴 생성
        GenerateRandomPattern();

        Debug.Log($"✅ 패턴 재시작! 현재 성공: {successfulHits}/{requiredSuccessfulHits}");
    }

    IEnumerator UnhighlightAfterDuration(int drumIndex, float duration)
    {
        yield return new WaitForSeconds(duration);
        drums[drumIndex].UnHighlight();
    }

    // 튜토리얼 리듬 시작
    public void StartTutorialRhythm()
    {
        // 이미 시작했으면 무시
        if (isPlaying)
        {
            Debug.LogWarning("⚠️ 리듬 게임이 이미 진행 중입니다!");
            return;
        }

        Debug.Log("🎮 튜토리얼 리듬 게임 시작!");

        // 랜덤 패턴 생성
        GenerateRandomPattern();

        isPlaying = true;
        gameStartTime = Time.time;
        currentBeatIndex = 0;
        successfulHits = 0;

        UpdateProgressUI();
    }

    // 현재 플레이 중인지 확인
    public bool IsPlaying()
    {
        return isPlaying;
    }

    // 🎲 랜덤 리듬 패턴 생성
    void GenerateRandomPattern()
    {
        tutorialBeats = new List<SimpleBeat>();

        float beatInterval = 60f / tutorialBPM;
        float currentTime = 1f; // 1초부터 시작

        int lastDrumIndex = -1; // 이전 드럼 인덱스
        int consecutiveCount = 0; // 연속 같은 드럼 카운트

        // 8개 비트 생성
        for (int i = 0; i < 8; i++)
        {
            int drumIndex;

            // 랜덤 드럼 선택
            if (Random.value < randomness)
            {
                // 랜덤 선택
                drumIndex = Random.Range(0, 4);

                // 같은 드럼이 3번 연속되지 않도록
                if (drumIndex == lastDrumIndex)
                {
                    consecutiveCount++;
                    if (consecutiveCount >= 2)
                    {
                        // 다른 드럼 선택
                        drumIndex = (drumIndex + Random.Range(1, 4)) % 4;
                        consecutiveCount = 0;
                    }
                }
                else
                {
                    consecutiveCount = 0;
                }
            }
            else
            {
                // 순차적 패턴 (초보자 친화적)
                drumIndex = i % 4;
            }

            tutorialBeats.Add(new SimpleBeat(currentTime, drumIndex));
            currentTime += beatInterval;

            lastDrumIndex = drumIndex;

            Debug.Log($"🎵 비트 {i}: 드럼 {drumIndex} @ {currentTime:F2}초");
        }

        Debug.Log($"✅ 랜덤 패턴 생성 완료! 총 {tutorialBeats.Count}개의 비트");
    }

    // 북을 쳤을 때 호출 (DrumController에서)
    public void OnTutorialDrumHit(string judgment, int drumIndex)
    {
        if (!isPlaying)
        {
            Debug.LogWarning("⚠️ 리듬 게임이 플레이 중이 아닙니다!");
            return;
        }
        Debug.Log($"🥁 드럼 타격! 판정: {judgment}, 드럼: {drumIndex}");

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

            // ✨ 목표 달성 체크 추가!
            if (successfulHits >= requiredSuccessfulHits)
            {
                Debug.Log("🎉 목표 달성! CompleteTutorialRhythm() 호출!");
                CompleteTutorialRhythm();
            }
        }
    }
    

    void UpdateProgressUI()
    {
        if (progressText != null)
        {
            progressText.text = $"성공: {successfulHits}/{requiredSuccessfulHits}";
        }
        // 큰 숫자 표시 "5"
        if (countText != null)
        {
            countText.text = successfulHits.ToString();
        }

        Debug.Log($"📊 UI 업데이트: {successfulHits}/{requiredSuccessfulHits}");
    }

    void CompleteTutorialRhythm()
    {
        isPlaying = false;

        Debug.Log("🎉 튜토리얼 리듬 완료!");

        // TutorialManager에 완료 알림
        if (tutorialManager != null)
        {
           tutorialManager.OnDrumTutorialComplete();
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