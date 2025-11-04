using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;      // 타임라인 제어
using UnityEngine.SceneManagement; // 씬 전환(선택)

public class StartOnClick : MonoBehaviour
{
    [SerializeField] PlayableDirector director;  // 인트로 타임라인
    [SerializeField] Animator characterAnimator; // 캐릭터 애니메이터(선택)
    [SerializeField] string startTrigger = "StartGame";
    [SerializeField] string nextScene = "MainGameScene";
    [SerializeField] bool loadNextScene = true;

    void OnMouseDown()  // 오브젝트를 클릭하면 자동 호출(콜라이더 필요)
    {
        if (director && director.state == PlayState.Playing) director.Stop();
        if (characterAnimator && !string.IsNullOrEmpty(startTrigger))
            characterAnimator.SetTrigger(startTrigger);

        if (loadNextScene)
            SceneManager.LoadScene(nextScene);
    }
}
