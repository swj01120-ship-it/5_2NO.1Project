using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    // ���� �޴��� ���ư���
    public void BackToMainMenu()
    {
        Debug.Log("���� �޴��� ���ư���");
        SceneManager.LoadScene("MainMenu");
    }
}
