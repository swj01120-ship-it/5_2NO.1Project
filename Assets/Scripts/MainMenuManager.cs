using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems; // ← 새로 추가
public class MainMenuManager : MonoBehaviour
{
    // ✨ 새로 추가: 사운드 설정
    [Header("Sound Settings")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 1f;
    [Range(0f, 1f)]
    [SerializeField] private float hoverVolume = 0.5f;

    private AudioSource audioSource; // ← 새로 추가

    // ✨ 새로 추가: Awake 함수
    private void Awake()
    {
        // AudioSource 컴포넌트 가져오기 또는 추가
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // AudioSource 기본 설정
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D 사운드
    }

    // 게임 시작 버튼


    public void OnStartGame()
    {
        PlayClickSound(); // ← 새로 추가
        Debug.Log("게임 시작 버튼 클릭!");

        // 로딩 씬을 통해 노래 선택 씬으로 이동
        LoadingSceneManager.nextScene = "SongSelectionScene";
        SceneManager.LoadScene("LoadingScene");
    }

    // 옵션 버튼
    public void OnOption()
    {
        PlayClickSound(); // ← 새로 추가
        Debug.Log("옵션 버튼 클릭!");

        // 나중에 옵션 화면 열기
    }
  
    // 튜토리얼 버튼
    public void OnTutorial()
    {
        PlayClickSound(); // ← 새로 추가
        Debug.Log("튜토리얼 버튼 클릭!");

        
        // 로딩 화면을 통해 튜토리얼 씬으로 이동
        LoadingSceneManager.nextScene = "TutorialScene";
        SceneManager.LoadScene("LoadingScene");
    }

    // 게임 종료 버튼
    public void OnQuitGame()
    {
        PlayClickSound(); // ← 새로 추가
        Debug.Log("게임 종료 버튼 클릭!");

        // Unity 에디터에서는 정지
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 실제 빌드에서는 종료
            Application.Quit();
#endif
    }
    // ✨ 새로 추가: 클릭 사운드 재생
    private void PlayClickSound()
    {
        if (audioSource != null && buttonClickSound != null)
        {
            audioSource.PlayOneShot(buttonClickSound, clickVolume);
            Debug.Log("🔊 버튼 클릭 사운드 재생!");
        }
        else
        {
            if (buttonClickSound == null)
            {
                Debug.LogWarning("⚠️ 버튼 클릭 사운드가 연결되지 않았습니다!");
            }
        }
    }

    // ✨ 새로 추가: 호버 사운드 재생
    private void PlayHoverSound()
    {
        if (audioSource != null && buttonHoverSound != null)
        {
            audioSource.PlayOneShot(buttonHoverSound, hoverVolume);
        }
    }

    // ✨ 새로 추가: 버튼에 호버 사운드 추가 (외부에서 호출 가능)
    public void SetupButtonSound(Button button)
    {
        if (button == null || buttonHoverSound == null) return;

        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((data) => { PlayHoverSound(); });
        trigger.triggers.Add(entry);
    }
   
}
