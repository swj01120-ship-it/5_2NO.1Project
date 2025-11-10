using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("판정 범위 (초 단위)")]
    public float perfectRange = 0.07f;
    public float greatRange = 0.5f;
    public float goodRange = 1.2f;
    public float missRange = 1.7f;

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

    [Header("카운트다운 설정")]
    public TextMeshProUGUI countdownText; // Legacy는 Text countdownText;
    public float countdownTime = 3f; // 3, 2, 1
    public string startText = "START!"; // 마지막에 표시할 텍스트
    public AudioClip countdownSound; // 카운트다운 사운드 (3, 2, 1)
    public AudioClip startSound; // 시작 사운드 (START!)
    public AudioClip resultSound; // 결과 패널 사운드

    [Header("결과 사운드 설정")]
    public AudioClip resultPerfectSound; // S 등급 사운드 (대성공!)
    public AudioClip resultGreatSound;   // A, B 등급 사운드 (성공!)
    public AudioClip resultGoodSound;    // C 등급 사운드 (보통)
    public AudioClip resultFailSound;    // D 등급 이하 사운드 (실패...)

    private bool isCountingDown = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // ⭐ DSP 시간 초기화 (더 정확한 타이밍)
        AudioSettings.dspTime.ToString();
    }

    void Start()
    {
        LoadSelectedSong();

        if (beatChart == null)
        {
            Debug.LogError("Beat Chart가 연결되지 않았습니다!");
            return;
        }

        UpdateUI();
        if (startPanel != null) startPanel.SetActive(true);
        if (scoreText != null) scoreText.gameObject.SetActive(false);
        gameStarted = false;
        isWaitingToStart = true;
        gameEnded = false;
        allNotesSpawned = false;
        musicEndLogged = false; // ⭐ 초기화
    }

    void LoadSelectedSong()
    {
        // SongSelectionManager에서 선택된 노래 가져오기
        if (SongSelectionManager.Instance != null)
        {
            SongData selectedSong = SongSelectionManager.Instance.GetSelectedSong();

            if (selectedSong != null)
            {
                // 비트차트 설정
                beatChart = selectedSong.beatChart;

                // 음악 설정
                if (musicSource != null && selectedSong.musicClip != null)
                {
                    musicSource.clip = selectedSong.musicClip;
                }

                Debug.Log($"✅ 노래 로드 완료: {selectedSong.songName} - {selectedSong.artist}");
            }
            else
            {
                Debug.LogWarning("⚠️ 선택된 노래가 없습니다. 기본 설정을 사용합니다.");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ SongSelectionManager를 찾을 수 없습니다. 기본 설정을 사용합니다.");
        }
    }

    void Update()
    {
        if (isWaitingToStart)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                // ✅ 엔터키를 누르면 카운트다운 시작
                StartCoroutine(StartGameWithCountdown());
                isWaitingToStart = false;
                if (startPanel != null) startPanel.SetActive(false); // "엔터키를 누르면..." 문구 숨김
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
    // ✅ 카운트다운과 함께 게임 시작
    IEnumerator StartGameWithCountdown()
    {
        isCountingDown = true;

        // 카운트다운 텍스트 활성화
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        // 3, 2, 1 카운트다운
        for (int i = (int)countdownTime; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                Debug.Log($"⏰ 카운트다운: {i}");
            }

            // 🔊 카운트다운 사운드 재생 (띡!)
            if (countdownSound != null && musicSource != null)
            {
                musicSource.PlayOneShot(countdownSound);
            }

            yield return new WaitForSeconds(1f);
        }

        // "START!" 표시
        if (countdownText != null)
        {
            countdownText.text = startText;
        }

        // 🔊 시작 사운드 재생
        if (startSound != null && musicSource != null)
        {
            musicSource.PlayOneShot(startSound);
        }

        yield return new WaitForSeconds(0.5f);

        // 카운트다운 텍스트 숨기기
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }

        // ✅ 실제 게임 시작
        StartGame();

        isCountingDown = false;
    }

    void StartGame()
    {
        gameStarted = true;
        gameStartTime = Time.time;
        currentBeatIndex = 0;
        gameEnded = false;
        allNotesSpawned = false;
        musicEndLogged = false;

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

        // ✅ UI 표시 (여기로 이동)
        if (scoreText != null) scoreText.gameObject.SetActive(true);

        // ✅ 음악 재생 (카운트다운 후에 재생됨)
        if (musicSource != null)
        {
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

    void EndGame()
    {
        gameStarted = false;

        // 🔊 점수에 따른 결과 사운드 재생
        AudioClip soundToPlay = GetResultSound();
        if (soundToPlay != null && musicSource != null)
        {
            musicSource.PlayOneShot(soundToPlay);
            Debug.Log("🔊 결과 사운드 재생!");
        }

        Debug.Log($"게임 종료! 최종점수: {score}, 최대 콤보: {maxCombo}");

        if (ResultScreenManager.Instance != null)
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

    // 3. 점수에 따른 사운드 선택 메서드 추가
    AudioClip GetResultSound()
    {
        // 총 노트 수 계산
        int totalNotes = perfectCount + greatCount + goodCount + missCount;
        if (totalNotes == 0) return resultFailSound;

        // 정확도 계산
        float accuracy = (float)(perfectCount + greatCount) / totalNotes * 100f;

        // 등급별 사운드 선택
        if (accuracy >= 95f && perfectCount > totalNotes * 0.7f)
        {
            // S 등급: 95% 이상 + Perfect가 70% 이상
            Debug.Log("🏆 S등급 - Perfect 사운드 재생!");
            return resultPerfectSound;
        }
        else if (accuracy >= 85f)
        {
            // A~B 등급: 85% 이상
            Debug.Log("⭐ A~B등급 - Great 사운드 재생!");
            return resultGreatSound;
        }
        else if (accuracy >= 70f)
        {
            // C 등급: 70% 이상
            Debug.Log("👍 C등급 - Good 사운드 재생!");
            return resultGoodSound;
        }
        else
        {
            // D 등급 이하: 70% 미만
            Debug.Log("😢 D등급 - Fail 사운드 재생!");
            return resultFailSound;
        }
    }
}
