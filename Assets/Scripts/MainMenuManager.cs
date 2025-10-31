using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{


    // ���� ���� ��ư


    public void OnStartGame()
    {
        Debug.Log("���� ���� ��ư Ŭ��!");

        // �ε� ���� ���� ���� ������ �̵�
        LoadingSceneManager.nextScene = "GameScene";
        SceneManager.LoadScene("LoadingScene");
    }

    // �ɼ� ��ư
    public void OnOption()
    {
        Debug.Log("�ɼ� ��ư Ŭ��!");

        // ���߿� �ɼ� ȭ�� ����
    }
  
    // Ʃ�丮�� ��ư
    public void OnTutorial()
    {
        Debug.Log("Ʃ�丮�� ��ư Ŭ��!");

        // ���߿� Ʃ�丮�� ������ �̵�
        // SceneManager.LoadScene("TutorialScene");
        
        // �ε� ȭ���� ���� Ʃ�丮�� ������ �̵�
        LoadingSceneManager.nextScene = "TutorialScene";
        SceneManager.LoadScene("LoadingScene");
    }

    // ���� ���� ��ư
    public void OnQuitGame()
    {
        Debug.Log("���� ���� ��ư Ŭ��!");

        // Unity �����Ϳ����� ����
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            // ���� ���忡���� ����
            Application.Quit();
#endif
    }
}
