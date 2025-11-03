using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RhythmGameManager : MonoBehaviour
{
    public static RhythmGameManager Instance;

    [Header("게임 설정")]
    public BeatChart beatChart;
    public AudioSource musicSource;
    public DrumController[] drums = new DrumController[4];

    [Header("시작 딜레이")]
    public float startDelay = 2f; // 게임 시작 전 대기 시간 (초)

    [Header("UI")]
    public Text scoreText;
    public Text comboText;
    public Text judgmentText;

    [Header("게임 상태")]
    private int score = 0;
    private int combo = 0;
    private int maxCombo = 0;

    [Header("판정 통계")]
    private int perfectCount = 0;
    private int greatCount = 0;
    private int goodCount = 0;
    private int missCount = 0;


    [Header("점수 설정")]
    public int perfectScore = 100;
    public int greatScore = 70;
    public int goodScore = 40;
    public int missScore = 0;

    [Header("비트 처리")]
    private List<BeatNote> remainingBeats;
    private int currentBeatIndex = 0;
    private bool gameStarted = false;
    private float gameStartTime;

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
        // 초기 검증
        if (beatChart == null)
        {
            Debug.LogError("❌ Beat Chart가 연결되지 않았습니다! Inspector에서 연결하세요.");
            return;
        }

        if(DifficultySettings.Instance != null)
        {
            float bpm = DifficultySettings.Instance.GetBPM();
            float density = DifficultySettings.Instance.GetDensity();
            float duration = DifficultySettings.Instance.GetHighlightDuration();

            Debug.Log($"🎯 난이도: {DifficultySettings.Instance.currentDifficulty}");
            Debug.Log($"📊 BPM: {bpm}, Density: {density}, Duration: {duration}");

            GenerateDynamicChart(bpm, density, duration);
        }

        else
        {
            Debug.LogWarning("⚠️ DifficultySettings가 없습니다. 기존 차트 사용");

            if (beatChart.beats.Count == 0)
            {
                Debug.LogError("❌ Beat Chart가 비어있습니다!");
                return;
            }
        }

        if (musicSource == null)
        {
            Debug.LogWarning("⚠️ Music Source가 연결되지 않았습니다!");
        }

        // 드럼 검증
        for (int i = 0; i < drums.Length; i++)
        {
            if (drums[i] == null)
            {
                Debug.LogError($"❌ Drum {i}이(가) 연결되지 않았습니다!");
            }
        }

        remainingBeats = new List<BeatNote>(beatChart.beats);

        UpdateUI();

        Debug.Log($"🎮 게임 준비 완료! 총 {remainingBeats.Count}개의 비트");
        Debug.Log($"⏰ {startDelay}초 후 자동 시작...");

        // 자동으로 게임 시작
        Invoke("StartGame", startDelay);
    }

    void Update()
    {
        if (!gameStarted) return;

        float currentTime = Time.time - gameStartTime;

        // 비트 처리
        while (currentBeatIndex < remainingBeats.Count)
        {
            BeatNote beat = remainingBeats[currentBeatIndex];

            if (currentTime >= beat.time)
            {
                // 북 강조
                drums[beat.drumIndex].Highlight();

                // 강조 해제 예약
                StartCoroutine(UnhighlightAfterDuration(beat.drumIndex, beat.duration));

                Debug.Log($"🎵 Beat #{currentBeatIndex} | Drum {beat.drumIndex} at {currentTime:F2}s");

                currentBeatIndex++;
            }
            else
            {
                break;
            }
        }

        // 게임 종료 체크
        if (currentBeatIndex >= remainingBeats.Count && gameStarted)
        {
            EndGame();
        }
    }

    System.Collections.IEnumerator UnhighlightAfterDuration(int drumIndex, float duration)
    {
        yield return new WaitForSeconds(duration);
        drums[drumIndex].UnHighlight();
    }

    void StartGame()
    {
        // 음악 자동 재생
        if (musicSource != null)
        {
            musicSource.Play();
            Debug.Log("🎵 음악 재생 시작!");
        }

        gameStarted = true;
        gameStartTime = Time.time;
        currentBeatIndex = 0;

        Debug.Log("🎮 게임 자동 시작!");
    }

    public void OnDrumHit(string judgment, int drumIndex)
    {
        if (judgment == "Miss")
        {
            combo = 0;
            missCount++;
            ShowJudgment("Miss");

            if (ScreenEffects.Instance != null)
            {
                ScreenEffects.Instance.Flash(Color.red, 0.3f);
            }
        }
        else
        {
            // 점수 추가
            int points = 0;
            if (judgment == "Perfect")
            {
                points = perfectScore;
                perfectCount++;

                if (ScreenEffects.Instance != null)
                {
                    ScreenEffects.Instance.Flash(Color.yellow, 0.4f);
                    ScreenEffects.Instance.CameraShake(0.15f, 0.15f);
                }
            }

            else if (judgment == "Great")
            {
                points = greatScore;
                greatCount++;

                if (ScreenEffects.Instance != null)
                {
                    ScreenEffects.Instance.Flash(Color.green, 0.3f);
                }
            }

            else if (judgment == "Good")
            {
                points = goodScore;
                goodCount++;

                if (ScreenEffects.Instance != null)
                {
                    ScreenEffects.Instance.Flash(Color.cyan, 0.2f);
                }
            }

            // 콤보 보너스
            combo++;
            if (combo > maxCombo) maxCombo = combo;

            int comboBonus = Mathf.FloorToInt(combo / 10f) * 10;
            score += points + comboBonus;

            ShowJudgment(judgment);
        }

        UpdateUI();
    }

    void ShowJudgment(string judgment)
    {
        if (judgmentText != null)
        {
            judgmentText.text = judgment;
            judgmentText.color = GetJudgmentColor(judgment);

            StopCoroutine("FadeJudgment");
            StartCoroutine("FadeJudgment");
        }
    }

    System.Collections.IEnumerator FadeJudgment()
    {
        if (judgmentText != null)
        {
            judgmentText.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            judgmentText.gameObject.SetActive(false);
        }
    }

    Color GetJudgmentColor(string judgment)
    {
        switch (judgment)
        {
            case "Perfect": return Color.yellow;
            case "Great": return Color.green;
            case "Good": return Color.cyan;
            case "Miss": return Color.red;
            default: return Color.white;
        }
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {score}";

        if (comboText != null)
        {
            if (combo > 0)
            {
                comboText.text = $"Combo: {combo}";

                // ✅ 콤보 증가 시 애니메이션
                StartCoroutine(ComboScaleAnimation());
            }

            
            else
            {
                comboText.text = "";
            }
        }
    }
    IEnumerator ComboScaleAnimation()
    {
        if (comboText == null) yield break;

        // 원래 스케일 저장
        Vector3 originalScale = comboText.transform.localScale;
        Vector3 targetScale = originalScale * 1.3f;

        // 커지기
        float elapsed = 0f;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            comboText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        // 작아지기
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            comboText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        comboText.transform.localScale = originalScale;
    }

    void EndGame()
    {
        gameStarted = false;
        Debug.Log($"🎉 게임 종료! 최종 점수: {score}, 최대 콤보: {maxCombo}");

        if(ResultScreenManager.Instance != null)
        {
            GameResult result = new GameResult
            {
                finalScore = score,
                maxCombo = maxCombo,
                perfectCount = perfectCount,
                greatCount = greatCount,
                goodCount = goodCount,
                missCount = missCount
            };

            ResultScreenManager.Instance.ShowResult(result);
        }
    }
    void GenerateDynamicChart(float bpm, float density, float duration)
    {
        beatChart.beats.Clear();

        float beatInterval = 60f / bpm;
        float currentTime = 2f; // 2초부터 시작

        System.Random random = new System.Random();

        while (currentTime < beatChart.songLength)
        {
            // 랜덤하게 1~3개의 드럼 선택
            int numberOfDrums = random.Next(1, 4);

            for (int i = 0; i < numberOfDrums; i++)
            {
                int randomDrum = random.Next(0, 4);
                beatChart.beats.Add(new BeatNote(currentTime, randomDrum, duration));
            }

            currentTime += beatInterval / density;
        }

        Debug.Log($"✅ 동적 차트 생성 완료! 총 {beatChart.beats.Count}개의 비트");
    }
}
