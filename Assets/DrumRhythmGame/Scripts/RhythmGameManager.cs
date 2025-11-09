using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RhythmGameManager : MonoBehaviour
{
    public static RhythmGameManager Instance;

    [Header("게임 설정")]
    public BeatChart beatChart;
    public AudioSource musicSource;
    public DrumController[] drums = new DrumController[4];

    [Header("UI")]
    public GameObject startPanel;    // 시작 대기 UI 패널
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

    [Header("판정 범위 (초 단위)")]
    public float perfectRange = 0.07f;
    public float greatRange = 0.5f;
    public float goodRange = 1.2f;
    public float missRange = 1.7f;

    private List<BeatNote> remainingBeats;
    private int currentBeatIndex = 0;
    private bool gameStarted = false;
    private bool isWaitingToStart = true;
    private float gameStartTime;
    private Coroutine comboFadeCo;

    private int maxHighlightCount = 2;
    private HashSet<int> currentlyHighlighted = new HashSet<int>();

    [Header("카운트다운 설정")]
    public TextMeshProUGUI countdownText; // Legacy는 Text countdownText;
    public float countdownTime = 3f; // 3, 2, 1
    public string startText = "Start!"; // 마지막에 표시할 텍스트

    //private bool isGameStarted = false;
    private bool isCountingDown = false;

    [Header("카운트다운 사운드")]
    public AudioClip countdownSound;
    public AudioClip startSound;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (beatChart == null)
        {
            Debug.LogError("Beat Chart가 연결되지 않았습니다!");
            return;
        }
        remainingBeats = new List<BeatNote>(beatChart.beats);
        UpdateUI();
        if (startPanel != null) startPanel.SetActive(true);
        scoreText.gameObject.SetActive(false); // 시작 전 스코어 텍스트 숨기기
        gameStarted = false;
        //isWaitingToStart = true;
    }

    void Update()
    {
        if (!gameStarted && !isCountingDown)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                StartCoroutine(StartGameWithCountdown());   
                //StartGame();
                //isWaitingToStart = false;
                //if (startPanel != null) startPanel.SetActive(false);
                //scoreText.gameObject.SetActive(true); // 게임 시작 시 스코어 텍스트 표시
            }
            return;
        }

        if (!gameStarted) return;

        float currentTime = Time.time - gameStartTime;
        while (currentBeatIndex < remainingBeats.Count)
        {
            BeatNote beat = remainingBeats[currentBeatIndex];
            if (currentTime >= beat.time)
            {
                if (currentlyHighlighted.Count < maxHighlightCount && !currentlyHighlighted.Contains(beat.drumIndex))
                {
                    drums[beat.drumIndex].Highlight();
                    currentlyHighlighted.Add(beat.drumIndex);
                    StartCoroutine(UnhighlightAfterDurationWithTracking(beat.drumIndex, beat.duration));
                    currentBeatIndex++;
                }
                else
                {
                    break;
                }
            }
            else break;
        }
        if (currentBeatIndex >= remainingBeats.Count && gameStarted)
        {
            EndGame();
        }
        // 엔터키로 게임 시작 (카운트다운 포함)
        //if (!isGameStarted && !isCountingDown && Input.GetKeyDown(KeyCode.Return))
        
            //StartCoroutine(StartGameWithCountdown());
        
    }

    IEnumerator UnhighlightAfterDurationWithTracking(int drumIndex, float duration)
    {
        yield return new WaitForSeconds(duration);
        drums[drumIndex].UnHighlight();
        currentlyHighlighted.Remove(drumIndex);
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
            countdownText.text = i.ToString();

            if (countdownText != null && musicSource != null)
            {
                musicSource.PlayOneShot(countdownSound);   
                Debug.Log($"⏰ 카운트다운: {i}");

            }
            yield return new WaitForSeconds(1f);
        }

        // "시작!" 표시
        countdownText.text = startText;

        if (startSound != null && musicSource != null)
        {
            musicSource.PlayOneShot(startSound);
        }

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
       
        if(startPanel != null) startPanel.SetActive(false );
        scoreText.gameObject.SetActive(true );

        if (musicSource != null)
        {
            musicSource.Play();
            Debug.Log("음악 시작!");
        }
    }

    public void OnDrumHit(string judgment, int drumIndex)
    {
        if (drumIndex < 0 || drumIndex >= drums.Length) return;

        if (judgment == "Miss")
        {
            combo = 0;
            missCount++;
            ShowJudgment("Miss");
            PlayJudgmentEffect("Miss", drumIndex);
            if (ScreenEffects.Instance != null)
                ScreenEffects.Instance.Flash(Color.red, 0.3f);
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
                    if (ScreenEffects.Instance != null)
                    {
                        ScreenEffects.Instance.Flash(Color.yellow, 0.4f);
                        ScreenEffects.Instance.CameraShake(0.15f, 0.15f);
                    }
                    break;
                case "Great":
                    points = greatScore;
                    greatCount++;
                    PlayJudgmentEffect("Great", drumIndex);
                    if (ScreenEffects.Instance != null)
                        ScreenEffects.Instance.Flash(Color.green, 0.3f);
                    break;
                case "Good":
                    points = goodScore;
                    goodCount++;
                    PlayJudgmentEffect("Good", drumIndex);
                    if (ScreenEffects.Instance != null)
                        ScreenEffects.Instance.Flash(Color.cyan, 0.2f);
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
        judgmentText.text = judgment;
        judgmentText.color = GetJudgmentColor(judgment);
        judgmentText.gameObject.SetActive(true);
        StopCoroutine("FadeJudgment");
        StartCoroutine("FadeJudgment");
    }

    IEnumerator FadeJudgment()
    {
        yield return new WaitForSeconds(0.2f);
        judgmentText.gameObject.SetActive(false);
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
            case "Miss": effectToSpawn = missEffectPrefab; break;
        }
        if (effectToSpawn != null)
        {
            GameObject effect = Instantiate(effectToSpawn, pos, Quaternion.identity);
            StartCoroutine(FadeEffectText(effect));
            Destroy(effect, 1.0f);
        }
    }

    IEnumerator FadeEffectText(GameObject effect)
    {
        Text uiText = effect.GetComponentInChildren<Text>();
        if (uiText == null) yield break;
        Color originalColor = uiText.color;
        float duration = 0.1f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            uiText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1 - t);
            yield return null;
        }
        uiText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0);
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
        scoreText.text = $"Score: {score}";
        if (combo > 0)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = $"Combo: {combo}";
            if (comboFadeCo != null) StopCoroutine(comboFadeCo);
            comboFadeCo = StartCoroutine(ComboTextFadeWaitAnim());
        }
        else
        {
            comboText.text = "";
            comboText.gameObject.SetActive(true);
        }
    }

    IEnumerator ComboTextFadeWaitAnim()
    {
        comboText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1.4f);
        comboText.gameObject.SetActive(false);
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

    void EndGame()
    {
        gameStarted = false;
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
}
