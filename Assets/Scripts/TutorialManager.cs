using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    [Header("NPC 설정")]
    [SerializeField] private Transform npcTransform;
    [SerializeField] private float interactionDistance = 3f;

    [Header("UI 참조")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text npcNameText;
    [SerializeField] private Text dialogueText;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private GameObject taskPanel;
    [SerializeField] private Text taskText;
    [SerializeField] private GameObject interactionPrompt; // "E를 눌러 대화하기"

    [Header("플레이어")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerController playerController;

    [Header("타이핑 효과")]
    [SerializeField] private float typingSpeed = 0.05f;

    [Header("씬 전환")]
    [SerializeField] private string gameSceneName = "GameScene";
    [SerializeField] private float sceneTransitionDelay = 2f;

    [Header("북(드럼) 연습")]
    [SerializeField] private TutorialRhythmManager rhythmManager; // 리듬 매니저
    [SerializeField] private GameObject[] drumObjects = new GameObject[4]; // 4개의 북

    private int currentStep = 0;
    private bool isTyping = false;
    private bool taskCompleted = false;
    private string currentFullText = "";
    private bool tutorialStarted = false; // 튜토리얼 시작 여부
    private bool isNearNPC = false; // NPC 근처에 있는지

    // 이동 체크용
    private bool hasMovedForward = false;
    private bool hasMovedBackward = false;
    private bool hasMovedLeft = false;
    private bool hasMovedRight = false;
    //private bool hasJumped = false;
    private bool rhythmPracticeComplete = false; // 리듬 연습 완료 여부

    // 튜토리얼 단계별 내용
    private TutorialStep[] tutorialSteps;

    void Start()
    {
        InitializeTutorialSteps();

        // 플레이어 자동 찾기
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerController = player.GetComponent<PlayerController>();
            }
        }

        // NPC 자동 찾기
        if (npcTransform == null)
        {
            GameObject npc = GameObject.Find("Tiger_NPC");
            if (npc != null)
            {
                npcTransform = npc.transform;
            }
        }

        // UI 초기화
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (taskPanel != null) taskPanel.SetActive(false);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);

        // 리듬 매니저 자동 찾기
        if (rhythmManager == null)
        {
            rhythmManager = FindObjectOfType<TutorialRhythmManager>();

            if (rhythmManager != null)
            {
                Debug.Log("✅ TutorialRhythmManager 자동 찾기 성공!");
            }
            else
            {
                Debug.LogError("❌ TutorialRhythmManager를 찾을 수 없습니다!");
                Debug.LogError("❌ Hierarchy에 TutorialRhythmManager 오브젝트가 있는지 확인하세요!");
            }
        }
        else
        {
            Debug.Log("✅ TutorialRhythmManager가 이미 연결되어 있습니다!");
        }

        // 북 오브젝트들 비활성화
        for (int i = 0; i < drumObjects.Length; i++)
        {
            if (drumObjects[i] != null)
            {
                drumObjects[i].SetActive(false);
            }
        }
    }

    void Update()
    {
        // NPC와의 거리 체크
        CheckDistanceToNPC();

        // 튜토리얼 시작 전: E 키로 시작
        if (!tutorialStarted && isNearNPC && Input.GetKeyDown(KeyCode.E))
        {
            StartTutorial();
        }

        // 튜토리얼 진행 중에만 나머지 로직 실행
        if (!tutorialStarted) return;

        // 현재 단계의 태스크 체크
        CheckCurrentTask();

        // **리듬 연습 단계에서는 Enter 키 처리 안 함**
        if (currentStep < tutorialSteps.Length && tutorialSteps[currentStep].taskType == TaskType.RhythmPractice)
        {
            // 리듬 연습 완료 시에만 자동 진행
            return;
        }

        // **Enter 키로만 대화 진행** (KeyCode.Return = Enter)
        if (Input.GetKeyDown(KeyCode.Return) && !isTyping)
        {
            if (taskCompleted || tutorialSteps[currentStep].taskType == TaskType.None)
            {
                NextStep();
            }
        }

        // **타이핑 중일 때 Enter로 스킵**
        if (Input.GetKeyDown(KeyCode.Return) && isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentFullText;
            isTyping = false;
            if (continueButton != null) continueButton.SetActive(true);
        }
    }

    void InitializeTutorialSteps()
    {
        tutorialSteps = new TutorialStep[]
        {
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "어서오시게! 사물버스 마을에 오신 것을 환영한다네.\nEnter 키를 눌러 계속 진행합니다.",
                taskType = TaskType.None
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "먼저 기본적인 움직임을 알려주겠네.\nW, A, S, D 키로 앞, 뒤, 좌, 우로 이동할 수 있습니다.",
                taskType = TaskType.Move,
                taskDescription = "W, A, S, D 키로 모든 방향으로 이동해보세요"
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "잘하는구만! 이제 점프를 해보겠나?\nSpace 키를 눌러 점프할 수 있습니다.",
                taskType = TaskType.Jump,
                taskDescription = "Space 키를 눌러 점프해보세요"
            },
            //new TutorialStep
            //{
                //npcName = "호랭도령",
               // dialogue = "완벽합니다! VR 컨트롤러 사용법을 알려드리겠습니다.",
               // taskType = TaskType.None
           // },
           // new TutorialStep
            //{
               // npcName = "호랭도령",
               // dialogue = "VR 컨트롤러에서 나오는 레이저를 이용해\n물체를 가리키고 스틱을 잡을 수 있습니다.",
               // taskType = TaskType.None
           // },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "이제 주변을 둘러보게~ 허허...어서 북 앞으로 이동해 보게나!\n'북이 빛나면 타이밍에 맞춰서 F, G, K, L 키를 눌러주세요.'",
                taskType =TaskType.RhythmPractice,
                taskDescription = "Enter키를 누르면 시작됩니다. 8개 이상 성공시 다음 단계로 넘어갑니다!"
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "실력이 예사롭지 않구만! 이 정도 실력이면 같이 버스에 타도 되겠어",
                taskType = TaskType.None
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "콤보 시스템: 연속으로 정확하게 북을 치면 콤보가 올라간다네!\n'콤보가 높을수록 더 높은 점수를 얻을 수 있습니다.'",
                taskType = TaskType.None
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "스코어 시스템: 정확한 타이밍에 칠수록 높은 점수를 얻을 수 있다네.\n'Perfect > Great > Good > Miss 순으로 점수가 다릅니다.'",
                taskType = TaskType.None
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "랭킹 시스템: 최종 점수에 따라 S, A, B, C, D, F 등급이 매겨진다네.\n'S등급을 목표로 열심히 연습하세요!'",
                taskType = TaskType.None
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "연주가 끝나고 아쉽다면 다시 시작, 혹은 버스를 내릴 수 있다네\n'게임이 끝난 후 '다시 시작' 버튼 혹은 '메인 화면' 버튼으로 돌아갈 수 있습니다.'",
                taskType = TaskType.None
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "자! 이제 모든 준비가 끝났으니 Enter를 치고 나를 따라오게나~?\n행운을 빈다네!",
                taskType = TaskType.None
            }
        };
    }


    void CheckDistanceToNPC()
    {
        if (npcTransform == null || playerTransform == null) return;

        float distance = Vector3.Distance(playerTransform.position, npcTransform.position);

        if (distance <= interactionDistance && !tutorialStarted)
        {
            isNearNPC = true;
            if (interactionPrompt != null) interactionPrompt.SetActive(true);
        }
        else
        {
            isNearNPC = false;
            if (interactionPrompt != null && !tutorialStarted) interactionPrompt.SetActive(false);
        }
    }

    void StartTutorial()
    {
        tutorialStarted = true;
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
        ShowDialogue();
    }

    void ShowDialogue()
    {
        if (currentStep >= tutorialSteps.Length) return;

        TutorialStep step = tutorialSteps[currentStep];

        Debug.Log($"📢 ShowDialogue 호출! 현재 단계: {currentStep}, TaskType: {step.taskType}");

        // 대화창 표시
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (npcNameText != null) npcNameText.text = step.npcName;

        // 타이핑 효과로 텍스트 표시
        StartCoroutine(TypeText(step.dialogue));

        // 태스크 있으면 표시
        if (step.taskType != TaskType.None)
        {
            if (taskPanel != null) taskPanel.SetActive(true);
            if (taskText != null) taskText.text = step.taskDescription;
            taskCompleted = false;

            // 리듬 연습 태스크면 드럼만 보여주고 대기 (게임 시작 안 함!)
            if (step.taskType == TaskType.RhythmPractice)
            {
                Debug.Log("🥁 리듬 연습 단계! 드럼 활성화 중... (게임은 아직 시작 안 함)");
                Debug.Log($"🔍 drumObjects 배열 크기: {drumObjects.Length}");

                // 북 오브젝트 활성화
                for (int i = 0; i < drumObjects.Length; i++)
                {
                    if (drumObjects[i] != null)
                    {
                        drumObjects[i].SetActive(true);
                        Debug.Log($"✅ Drum {i} 활성화 성공: {drumObjects[i].name}");
                    }
                    else
                    {
                        Debug.LogError($"❌ Drum {i}이(가) NULL입니다! Inspector에서 연결하세요!");
                    }
                }

                Debug.Log("⏸️ 드럼이 활성화되었습니다. Enter 키를 눌러 연습을 시작하세요!");

                // ❌ 여기서 게임 시작 안 함! Enter 키 대기
            }
        }
        else
        {
            if (taskPanel != null) taskPanel.SetActive(false);
        }
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        currentFullText = text;
        dialogueText.text = "";
        if (continueButton != null) continueButton.SetActive(false);

        foreach (char c in text)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        if (continueButton != null) continueButton.SetActive(true);
    }

    void CheckCurrentTask()
    {
        if (currentStep >= tutorialSteps.Length) return;
        if (taskCompleted) return;

        TutorialStep step = tutorialSteps[currentStep];

        switch (step.taskType)
        {
            case TaskType.None:
                break;

            case TaskType.Move:
                // WASD 모두 눌렀는지 체크
                if (Input.GetKey(KeyCode.W)) hasMovedForward = true;
                if (Input.GetKey(KeyCode.S)) hasMovedBackward = true;
                if (Input.GetKey(KeyCode.A)) hasMovedLeft = true;
                if (Input.GetKey(KeyCode.D)) hasMovedRight = true;

                if (hasMovedForward && hasMovedBackward && hasMovedLeft && hasMovedRight)
                {
                    CompleteTask();
                }
                break;

            case TaskType.Jump:
                // **점프는 Space만 체크 (Enter와 분리됨)**
                if (Input.GetKeyDown(KeyCode.Space))
                {
                   // hasJumped = true;
                    CompleteTask();
                }
                break;

            case TaskType.RhythmPractice:
                // **Enter 키로 리듬 연습 시작**
                if (Input.GetKeyDown(KeyCode.Return) && !rhythmPracticeComplete && rhythmManager != null)
                {
                    Debug.Log("🎮 Enter 키 입력! 리듬 연습 시작!");

                    // 태스크 텍스트 변경
                    if (taskText != null)
                    {
                        taskText.text = "빛나는 북을 타이밍에 맞춰 쳐보세요!";
                    }

                    // 리듬 게임 시작
                    rhythmManager.StartTutorialRhythm();
                }

                // 리듬 연습 완료 체크
                if (rhythmPracticeComplete)
                {
                    CompleteTask();
                }
                break;

            case TaskType.ClickDrum:
                // 사용 안 함 (RhythmPractice로 대체)
                break;
        }
    }

    // 리듬 튜토리얼 완료 시 호출 (TutorialRhythmManager에서)
    public void OnDrumTutorialComplete()
    {
        rhythmPracticeComplete = true;
        Debug.Log("✅ 리듬 튜토리얼 완료!");

        // 북 비활성화
        for (int i = 0; i < drumObjects.Length; i++)
        {
            if (drumObjects[i] != null)
            {
                drumObjects[i].SetActive(false);
            }
        }

        // 자동으로 다음 단계로 진행
        Debug.Log("➡️ 다음 단계로 자동 진행...");

        // 잠깐 대기 후 다음 단계
        StartCoroutine(AutoNextStepAfterRhythm());
    }

    System.Collections.IEnumerator AutoNextStepAfterRhythm()
    {
        // 1초 대기 (완료 메시지 볼 시간)
        yield return new WaitForSeconds(1f);

        // 다음 단계로
        NextStep();
    }

    // 북을 쳤을 때 외부에서 호출하는 함수 (이제 사용 안 함)
    public void OnDrumHit()
    {
        // 이전 단순 북 치기 - 더 이상 사용 안 함
    }

    void CompleteTask()
    {
        taskCompleted = true;
        if (taskPanel != null && taskText != null)
        {
            taskText.text = "✓ 완료!";
            taskText.color = Color.green;
        }
    }

    public void NextStep()
    {
        currentStep++;

        // 태스크 초기화
        taskCompleted = false;
        if (taskText != null) taskText.color = Color.white;

        if (currentStep < tutorialSteps.Length)
        {
            ShowDialogue();
        }
        else
        {
            EndTutorial();
        }
    }

    void EndTutorial()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (taskPanel != null) taskPanel.SetActive(false);

        Debug.Log("튜토리얼 완료! 게임 씬으로 전환합니다...");

        // 게임 씬으로 전환
        StartCoroutine(TransitionToGameScene());
    }

    IEnumerator TransitionToGameScene()
    {
        // 전환 전 딜레이
        yield return new WaitForSeconds(sceneTransitionDelay);

        // 씬 전환
        SceneManager.LoadScene(gameSceneName);
    }

    // 에디터에서 거리 시각화
    void OnDrawGizmosSelected()
    {
        if (npcTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(npcTransform.position, interactionDistance);
        }
    }
}

// 튜토리얼 단계 구조체
[System.Serializable]
public class TutorialStep
{
    public string npcName;
    [TextArea(3, 5)]
    public string dialogue;
    public TaskType taskType;
    public string taskDescription;
}

// 태스크 타입
public enum TaskType
{
    None,
    Move,
    Jump,
    RhythmPractice, // 리듬 연습 (새로 추가)
    ClickDrum // 사용 안 함
}