using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    // 게임 시작 버튼
    public void OnStartGame()
    {
        Debug.Log("게임 시작 버튼 클릭!");

        // 나중에 게임 씬으로 이동
        // SceneManager.LoadScene("GameScene");
    }

    // 옵션 버튼
    public void OnOption()
    {
        Debug.Log("옵션 버튼 클릭!");

        // 나중에 옵션 화면 열기
    }

    // 튜토리얼 버튼
    public void OnTutorial()
    {
        Debug.Log("튜토리얼 버튼 클릭!");

        // 나중에 튜토리얼 씬으로 이동
        // SceneManager.LoadScene("TutorialScene");
    }

    // 게임 종료 버튼
    public void OnQuitGame()
    {
        Debug.Log("게임 종료 버튼 클릭!");

        // Unity 에디터에서는 정지
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // 실제 빌드에서는 종료
            Application.Quit();
#endif
    }
}
