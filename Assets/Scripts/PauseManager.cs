using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;           // 전체 패널
    public GameObject pauseMenuPanel;       // 버튼들이 있는 패널
    public GameObject pauseBackground;      // ⭐ 어두운 배경 (따로 분리)
    public GameObject countdownPanel;       // 카운트다운 패널
    public Text countdownText;              // 카운트다운 텍스트

    [Header("Buttons")]
    public Button resumeButton;
    public Button songSelectButton;
    public Button mainMenuButton;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string songSelectSceneName = "SongSelectionScene";

    [Header("Managers")]
    public RhythmGameManager rhythmGameManager;

    private bool isPaused = false;
    private bool isCountingDown = false;

    void Start()
    {
        // ⭐ 버튼 이벤트를 Start가 아닌 Awake에서 연결하거나
        // 리스너를 제거하고 다시 추가
        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(OnResumeButtonClicked);
            Debug.Log("✅ Resume 버튼 연결됨");
        }

        if (songSelectButton != null)
        {
            songSelectButton.onClick.RemoveAllListeners();
            songSelectButton.onClick.AddListener(OnSongSelectButtonClicked);
            Debug.Log("✅ SongSelect 버튼 연결됨");
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
            Debug.Log("✅ MainMenu 버튼 연결됨");
        }

        // RhythmGameManager 자동 찾기
        if (rhythmGameManager == null)
        {
            rhythmGameManager = FindObjectOfType<RhythmGameManager>();
            if (rhythmGameManager != null)
            {
                Debug.Log("✅ RhythmGameManager 자동 연결됨");
            }
        }

        // 처음엔 일시정지 패널 비활성화
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Debug.Log("✅ PauseManager 초기화 완료");
    }

    void Update()
    {
        // ESC 키 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // ⭐ 게임이 실제로 시작된 상태에서만 일시정지 가능
            if (rhythmGameManager != null && rhythmGameManager.IsGameStarted())
            {
                if (!isCountingDown)
                {
                    if (isPaused)
                    {
                        OnResumeButtonClicked();
                    }
                    else
                    {
                        PauseGame();
                    }
                }
            }
        }
    }

    // 게임 일시정지
    void PauseGame()
    {
        if (rhythmGameManager == null)
        {
            Debug.LogError("❌ RhythmGameManager가 없습니다!");
            return;
        }

        isPaused = true;
        Time.timeScale = 0f;

        // BGM 일시정지
        rhythmGameManager.PauseBGM();

        // UI 표시
        if (pausePanel != null) pausePanel.SetActive(true);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (pauseBackground != null) pauseBackground.SetActive(true);  // ⭐ 배경 표시
        if (countdownPanel != null) countdownPanel.SetActive(false);

        Debug.Log("⏸️ 게임 일시정지");
    }

    // 게임 재개 버튼 클릭
    public void OnResumeButtonClicked()
    {
        Debug.Log("🎮 Resume 버튼 클릭됨!");

        if (isPaused)
        {
            StartCoroutine(ResumeWithCountdown());
        }
    }

    // 카운트다운 후 게임 재개
    IEnumerator ResumeWithCountdown()
    {
        isCountingDown = true;

        // ⭐ 메뉴 패널과 어두운 배경 숨기고, 카운트다운만 표시
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (pauseBackground != null) pauseBackground.SetActive(false);  // ⭐ 배경 끄기
        if (countdownPanel != null) countdownPanel.SetActive(true);

        // 3, 2, 1 카운트다운
        for (int i = 3; i > 0; i--)
        {
            if (countdownText != null)
            {
                countdownText.text = i.ToString();
                countdownText.fontSize = 120;
            }

            // Time.timeScale = 0이어도 작동하는 대기
            float timer = 0f;
            while (timer < 1f)
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            Debug.Log($"⏰ 재개 카운트: {i}");
        }

        // "시작!" 표시
        if (countdownText != null)
        {
            countdownText.text = "시작!";
        }

        float startTimer = 0f;
        while (startTimer < 0.5f)
        {
            startTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        // 게임 재개
        ResumeGame();
    }

    // 실제 게임 재개
    void ResumeGame()
    {
        isPaused = false;
        isCountingDown = false;
        Time.timeScale = 1f;

        // BGM 재개
        if (rhythmGameManager != null)
        {
            rhythmGameManager.ResumeBGM();
        }

        // UI 완전히 숨기기
        if (pausePanel != null) pausePanel.SetActive(false);
        if (countdownPanel != null) countdownPanel.SetActive(false);

        Debug.Log("▶️ 게임 재개");
    }

    // 음악 선택 버튼 클릭
    public void OnSongSelectButtonClicked()
    {
        Debug.Log("🎵 SongSelect 버튼 클릭됨!");

        Time.timeScale = 1f;  // ⭐ 먼저 timeScale 복구

        if (rhythmGameManager != null)
        {
            rhythmGameManager.StopBGM();
        }

        SceneManager.LoadScene(songSelectSceneName);
    }

    // 메인 메뉴 버튼 클릭
    public void OnMainMenuButtonClicked()
    {
        Debug.Log("🏠 MainMenu 버튼 클릭됨!");

        Time.timeScale = 1f;  // ⭐ 먼저 timeScale 복구

        if (rhythmGameManager != null)
        {
            rhythmGameManager.StopBGM();
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    // 외부에서 일시정지 상태 확인
    public bool IsPaused()
    {
        return isPaused;
    }
}