using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    // 메인 메뉴로 돌아가기
    public void BackToMainMenu()
    {
        Debug.Log("메인 메뉴로 돌아가기");
        SceneManager.LoadScene("MainMenu");
    }
}
