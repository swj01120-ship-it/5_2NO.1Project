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
    private bool hasJumped = false;
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
                dialogue = "반갑습니다! 사물버스 마을에 오신 것을 환영합니다.\nEnter 키를 눌러 계속 진행하세요.",
                taskType = TaskType.None
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "먼저 기본적인 움직임을 배워봅시다.\nW, A, S, D 키로 앞, 뒤, 좌, 우로 이동할 수 있습니다.",
                taskType = TaskType.Move,
                taskDescription = "W, A, S, D 키로 모든 방향으로 이동해보세요"
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "잘하셨습니다! 이제 점프를 배워볼까요?\nSpace 키를 눌러 점프할 수 있습니다.",
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
                dialogue = "짠~! 옆에 보시면 북이 생겼어요, 이제 사물놀이의 핵심인 북 치는 법을 알려드리겠습니다.",
                taskType =TaskType.RhythmPractice,
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "게임이 시작되면 북이 빛나기 시작합니다.\n빛나는 북을 키보드 A,S,D,F 키로 타이밍 맞춰서 치면 됩니다!",
                taskType = TaskType.None
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "콤보 시스템: 연속으로 정확하게 북을 치면 콤보가 올라갑니다!\n콤보가 높을수록 더 높은 점수를 얻을 수 있습니다.",
                taskType = TaskType.None
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "스코어 시스템: 정확한 타이밍에 칠수록 높은 점수를 얻습니다.\nPerfect > Great > Good > Miss 순으로 점수가 다릅니다.",
                taskType = TaskType.None
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "랭킹 시스템: 최종 점수에 따라 S, A, B, C, D, F 등급이 매겨집니다.\nS등급을 목표로 열심히 연습하세요!",
                taskType = TaskType.None
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "게임이 끝나면 '다시 시작' 버튼으로 재도전하거나,\n'메인 화면' 버튼으로 돌아갈 수 있습니다.",
                taskType = TaskType.None
            },
            new TutorialStep
            {
                npcName = "호랭도령",
                dialogue = "튜토리얼이 끝났습니다! 이제 실전으로 가볼까요?\n행운을 빕니다!",
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

            // 리듬 연습 태스크면 북 활성화 및 시작
            if (step.taskType == TaskType.RhythmPractice)
            {
                Debug.Log("🥁 리듬 연습 시작! 드럼 활성화 중...");
                Debug.Log($"🔍 drumObjects 배열 크기: {drumObjects.Length}");

                // 북 오브젝트 활성화
                for (int i = 0; i < drumObjects.Length; i++)
                {
                    if (drumObjects[i] != null)
                    {
                        drumObjects[i].SetActive(true);
                        Debug.Log($"✅ Drum {i} 활성화: {drumObjects[i].name}");
                    }
                    else
                    {
                        Debug.LogError($"❌ Drum {i}이(가) 연결되지 않았습니다!");
                    }
                }
                Debug.Log($"🔍 활성화 완료! 총 {drumObjects.Length}개 처리");

                // 리듬 게임 시작
                if (rhythmManager != null)
                {
                    Debug.Log("✅ TutorialRhythmManager 발견! StartTutorialRhythm() 호출...");
                    rhythmPracticeComplete = false;
                    rhythmManager.StartTutorialRhythm();
                }
                else
                {
                    Debug.LogError("❌ TutorialRhythmManager가 연결되지 않았습니다!");
                    Debug.LogError("❌ Inspector에서 Rhythm Manager 필드를 확인하세요!");
                }
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
                    hasJumped = true;
                    CompleteTask();
                }
                break;

            case TaskType.RhythmPractice:
                // 리듬 연습은 TutorialRhythmManager에서 처리
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