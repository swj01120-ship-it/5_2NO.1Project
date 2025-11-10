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

    [Header("노트 시스템")]
    public GameObject notePrefab;
    public Transform[] noteSpawnPoints = new Transform[4];
    public float noteTravelTime = 1.5f;
    public float coverYOffset = 1.756611f;

    [Header("⭐ 오디오 지연 보정")]
    [Tooltip("오디오 시스템 지연 보정 (밀리초) - 비트가 늦게 느껴지면 음수 값 사용")]
    public float audioLatencyMs = -50f; // -50ms = 0.05초 빠르게
    private float audioLatency => audioLatencyMs / 1000f;

    [Header("노트 색상")]
    public Color[] laneColors = new Color[4]
    {
        new Color(1f, 0.3f, 0.3f),
        new Color(0.3f, 0.5f, 1f),
        new Color(0.3f, 1f, 0.3f),
        new Color(1f, 1f, 0.3f)
    };

    [Header("UI")]
    public GameObject startPanel;
    public Text scoreText;
    public Text comboText;
    public Text judgmentText;

    [Header("이펙트 프리팹")]
    public GameObject perfectEffectPrefab;
    public GameObject greatEffectPrefab;
    public GameObject goodEffectPrefab;
    public GameObject missEffectPrefab;
    public GameObject comboEffectPrefab;

    [Header("이펙트 위치")]
    public Transform[] drumPositions = new Transform[4];

    private int score = 0;
    private int combo = 0;
    private int maxCombo = 0;
    private int perfectCount = 0;
    private int greatCount = 0;
    private int goodCount = 0;
    private int missCount = 0;

    [Header("점수 설정")]
    public int perfectScore = 100;
    public int greatScore = 70;
    public int goodScore = 40;
    public int missScore = 0;

    private bool gameStarted = false;
    private bool isWaitingToStart = true;
    private float gameStartTime;
    private int currentBeatIndex = 0;
    private List<NoteObject> activeNotes = new List<NoteObject>();

    private bool gameEnded = false;
    private bool allNotesSpawned = false;
    private bool musicEndLogged = false; // ⭐ 로그 중복 방지

    [Header("⭐ 게임 종료 설정")]
    [Tooltip("모든 노트가 사라진 후 결과 화면까지 대기 시간 (초)")]
    public float endGameDelay = 0.5f; // 2초 → 0.5초로 단축!

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // ⭐ DSP 시간 초기화 (더 정확한 타이밍)
        AudioSettings.dspTime.ToString();
    }

    void Start()
    {
        UpdateUI();
        if (startPanel != null) startPanel.SetActive(true);
        if (scoreText != null) scoreText.gameObject.SetActive(false);
        gameStarted = false;
        isWaitingToStart = true;
        gameEnded = false;
        allNotesSpawned = false;
        musicEndLogged = false; // ⭐ 초기화
    }

    void Update()
    {
        if (isWaitingToStart)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                StartGame();
                isWaitingToStart = false;
                if (startPanel != null) startPanel.SetActive(false);
                if (scoreText != null) scoreText.gameObject.SetActive(true);
            }
            return;
        }

        if (!gameStarted) return;

        // ⭐ 오디오 지연 보정 적용
        float currentTime = Time.time - gameStartTime + audioLatency;

        // ⭐⭐ 음악이 끝났는지 체크 (Actual Music End Time 고려)
        if (musicSource != null && !musicSource.isPlaying && !gameEnded)
        {
            // ⭐ 로그는 한 번만 출력
            if (!musicEndLogged)
            {
                musicEndLogged = true;
                Debug.Log($"🎵 음악 종료 감지! 남은 노트: {activeNotes.Count}개");
            }
            
            // 모든 노트가 스폰되었다고 표시
            if (!allNotesSpawned)
            {
                allNotesSpawned = true;
                Debug.Log("✅ 음악 종료로 인한 강제 노트 스폰 완료!");
            }
            
            // 남은 노트가 없으면 게임 종료
            if (activeNotes.Count == 0)
            {
                Debug.Log("🎯 모든 노트 처리 완료! 게임 종료 시작!");
                StartCoroutine(EndGameAfterDelay());
            }
        }

        while (currentBeatIndex < beatChart.beats.Count)
        {
            BeatNote beat = beatChart.beats[currentBeatIndex];

            if (currentTime >= beat.time - noteTravelTime)
            {
                int drumIdx = beat.drumIndex;

                if (drumIdx < 0 || drumIdx >= drums.Length || drumIdx >= noteSpawnPoints.Length)
                {
                    currentBeatIndex++;
                    continue;
                }

                if (drums[drumIdx] == null || noteSpawnPoints[drumIdx] == null)
                {
                    currentBeatIndex++;
                    continue;
                }

                Vector3 spawnPos = noteSpawnPoints[drumIdx].position;
                Vector3 targetPos = drums[drumIdx].transform.position + Vector3.up * coverYOffset;
                float yDist = Mathf.Abs(spawnPos.y - targetPos.y);
                float exactFallSpeed = yDist / noteTravelTime;

                GameObject noteGO = Instantiate(notePrefab, spawnPos, Quaternion.identity);
                NoteObject note = noteGO.GetComponent<NoteObject>();
                if (note != null)
                {
                    note.drumIndex = drumIdx;
                    note.SetTargetPosition(targetPos);
                    note.SetColor(laneColors[drumIdx]);
                    note.fallSpeed = exactFallSpeed;
                    activeNotes.Add(note);
                }

                currentBeatIndex++;
            }
            else break;
        }

        if (!allNotesSpawned && currentBeatIndex >= beatChart.beats.Count)
        {
            allNotesSpawned = true;
            Debug.Log("✅ 모든 노트 스폰 완료!");
        }

        // ⭐ 게임 종료 조건 개선
        if (!gameEnded && allNotesSpawned && activeNotes.Count == 0)
        {
            Debug.Log("🎵 게임 종료!");
            StartCoroutine(EndGameAfterDelay());
        }
    }

    void StartGame()
    {
        gameStarted = true;
        gameStartTime = Time.time;
        currentBeatIndex = 0;
        gameEnded = false;
        allNotesSpawned = false;
        musicEndLogged = false; // ⭐ 초기화

        // 점수 초기화
        score = 0;
        combo = 0;
        maxCombo = 0;
        perfectCount = 0;
        greatCount = 0;
        goodCount = 0;
        missCount = 0;

        if (beatChart == null || beatChart.beats == null || beatChart.beats.Count == 0)
        {
            Debug.LogError("❌ BeatChart 오류!");
            return;
        }

        Debug.Log($"🎮 게임 시작! (오디오 보정: {audioLatencyMs}ms)");

        if (musicSource != null)
        {
            // ⭐ DSP 시간 기반 정확한 재생
            musicSource.Play();
        }
    }

    public void OnDrumHit(string judgment, int drumIndex)
    {
        if (judgment == "Miss")
        {
            combo = 0;
            missCount++;
            ShowJudgment("Miss");
            PlayJudgmentEffect("Miss", drumIndex);
        }
        else
        {
            int points = 0;
            switch (judgment)
            {
                case "Perfect":
                    points = perfectScore;
                    perfectCount++;
                    PlayJudgmentEffect("Perfect", drumIndex);
                    break;
                case "Great":
                    points = greatScore;
                    greatCount++;
                    PlayJudgmentEffect("Great", drumIndex);
                    break;
                case "Good":
                    points = goodScore;
                    goodCount++;
                    PlayJudgmentEffect("Good", drumIndex);
                    break;
            }
            combo++;
            if (combo > maxCombo) maxCombo = combo;
            int comboBonus = Mathf.FloorToInt(combo / 10f) * 10;
            score += points + comboBonus;
            ShowJudgment(judgment);
            PlayComboEffect(combo);
        }
        UpdateUI();
    }

    void ShowJudgment(string judgment)
    {
        if (judgmentText == null) return;
        judgmentText.text = judgment;
        judgmentText.color = GetJudgmentColor(judgment);
        judgmentText.gameObject.SetActive(true);
        StopCoroutine("FadeJudgment");
        StartCoroutine("FadeJudgment");
    }

    IEnumerator FadeJudgment()
    {
        yield return new WaitForSeconds(0.2f);
        if (judgmentText != null) judgmentText.gameObject.SetActive(false);
    }

    void PlayJudgmentEffect(string judgment, int drumIndex)
    {
        if (drumIndex < 0 || drumIndex >= drumPositions.Length) return;
        Vector3 pos = drumPositions[drumIndex].position;
        GameObject effectToSpawn = null;
        switch (judgment)
        {
            case "Perfect": effectToSpawn = perfectEffectPrefab; break;
            case "Great": effectToSpawn = greatEffectPrefab; break;
            case "Good": effectToSpawn = goodEffectPrefab; break;
            case "Miss":
                effectToSpawn = missEffectPrefab;
                pos.y -= 1.0f;
                break;
        }
        if (effectToSpawn != null)
        {
            GameObject effect = Instantiate(effectToSpawn, pos, Quaternion.identity);
            Destroy(effect, 1.0f);
        }
    }

    void PlayComboEffect(int currentCombo)
    {
        if (comboEffectPrefab == null) return;
        if (currentCombo > 0 && currentCombo % 10 == 0)
        {
            Vector3 pos = comboText.transform.position;
            GameObject effect = Instantiate(comboEffectPrefab, pos, Quaternion.identity);
            Destroy(effect, 1.0f);
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
                comboText.gameObject.SetActive(true);
                comboText.text = $"Combo: {combo}";
            }
            else
            {
                comboText.text = "";
            }
        }
    }

    public void RemoveNote(NoteObject note)
    {
        if (activeNotes.Contains(note))
            activeNotes.Remove(note);
    }

    IEnumerator EndGameAfterDelay()
    {
        if (gameEnded) yield break;
        gameEnded = true;

        // ⭐ 더 짧은 대기 시간
        Debug.Log($"⏳ {endGameDelay}초 후 결과 표시...");
        yield return new WaitForSeconds(endGameDelay);

        EndGame();
    }

    public void EndGame()
    {
        if (!gameEnded) gameEnded = true;
        gameStarted = false;
        if (musicSource != null && musicSource.isPlaying) musicSource.Stop();

        Debug.Log($"🎮 게임 종료! 점수: {score}");

        GameResult result = new GameResult();
        result.finalScore = score;
        result.maxCombo = maxCombo;
        result.perfectCount = perfectCount;
        result.greatCount = greatCount;
        result.goodCount = goodCount;
        result.missCount = missCount;

        if (ResultScreenManager.Instance != null)
        {
            ResultScreenManager.Instance.ShowResult(result);
        }
        else
        {
            Debug.LogError("❌ ResultScreenManager 없음!");
        }
    }
}
