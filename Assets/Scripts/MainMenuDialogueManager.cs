using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuDialogueManager : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text npcNameText;
    [SerializeField] private Text dialogueText;
    [SerializeField] private GameObject continuePrompt; // "Enter를 눌러 계속..." UI (선택사항)

    [Header("NPC 및 플레이어")]
    [SerializeField] private Transform npcTransform; // 호랭도령 위치
    [SerializeField] private Transform playerTransform; // 플레이어 위치

    [Header("트리거 설정")]
    [SerializeField] private float approachDistance = 5f; // NPC와의 거리
    [SerializeField] private Transform signboardTransform; // 전광판 위치
    [SerializeField] private float signboardViewAngle = 30f; // 전광판 바라보는 각도

    [Header("타이핑 효과")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private bool autoHideDialogue = false; // 자동으로 숨김 여부
    [SerializeField] private float dialogueDisplayTime = 5f; // 대화 표시 시간 (autoHide가 true일 때만)

    // 대화 진행 상태
    private int currentStep = 0;
    private bool isTyping = false;
    private bool[] stepCompleted = new bool[3]; // 3단계 완료 체크
    private string currentFullText = "";

    // 대화 내용
    private DialogueStep[] dialogueSteps;

    void Start()
    {
        InitializeDialogues();

        // UI 초기화
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (continuePrompt != null) continuePrompt.SetActive(false);

        // 모든 단계 미완료로 초기화
        for (int i = 0; i < stepCompleted.Length; i++)
        {
            stepCompleted[i] = false;
        }
    }

    void Update()
    {
        // 각 단계별로 조건 체크
        CheckStep0_Approach();
        CheckStep1_MoveToSignboard();
        CheckStep2_LookAtSignboard();

        // Enter 키로 대화 닫기 (타이핑 완료 후)
        if (Input.GetKeyDown(KeyCode.Return) && !isTyping && dialoguePanel != null && dialoguePanel.activeSelf)
        {
            dialoguePanel.SetActive(false);
            if (continuePrompt != null) continuePrompt.SetActive(false);
            Debug.Log("💬 대화창 닫힘 (Enter)");
        }

        // 타이핑 중 Enter로 스킵
        if (Input.GetKeyDown(KeyCode.Return) && isTyping)
        {
            StopAllCoroutines();
            if (dialogueText != null) dialogueText.text = currentFullText;
            isTyping = false;

            // 스킵 후 continue prompt 표시
            if (!autoHideDialogue && continuePrompt != null)
            {
                continuePrompt.SetActive(true);
            }

            Debug.Log("💬 타이핑 스킵");
        }
    }

    void InitializeDialogues()
    {
        dialogueSteps = new DialogueStep[]
        {
            new DialogueStep
            {
                npcName = "호랭도령",
                text = "거기, 멀뚱멀뚱 서있지 말고, W 키를 눌러 이리 가까이 와보게나",
                checkCondition = CheckCondition_Step0
            },
            new DialogueStep
            {
                npcName = "호랭도령",
                text = "여기 왼쪽에 전광판부터 살펴보자구나",
                checkCondition = CheckCondition_Step1
            },
            new DialogueStep
            {
                npcName = "호랭도령",
                text = "마우스 방향이 가르키고 있는 레이저가 초록색으로 바뀌면 선택할 수 있네",
                checkCondition = CheckCondition_Step2
            }
        };
    }

    // ========== 단계별 조건 체크 ==========

    void CheckStep0_Approach()
    {
        if (stepCompleted[0] || currentStep != 0) return;

        // 조건: 게임 시작 후 1초 뒤 자동 표시
        if (!stepCompleted[0])
        {
            stepCompleted[0] = true;
            StartCoroutine(DelayedShowDialogue(0, 1f));
        }
    }

    void CheckStep1_MoveToSignboard()
    {
        if (stepCompleted[1] || currentStep != 1) return;

        // 조건: 플레이어가 NPC에게 가까이 감
        if (playerTransform != null && npcTransform != null)
        {
            float distance = Vector3.Distance(playerTransform.position, npcTransform.position);

            if (distance <= approachDistance)
            {
                stepCompleted[1] = true;
                ShowDialogue(1);
            }
        }
    }

    void CheckStep2_LookAtSignboard()
    {
        if (stepCompleted[2] || currentStep != 2) return;

        // 조건: 플레이어가 전광판을 바라봄
        if (playerTransform != null && signboardTransform != null)
        {
            Vector3 directionToSignboard = (signboardTransform.position - playerTransform.position).normalized;
            Vector3 playerForward = playerTransform.forward;

            float angle = Vector3.Angle(playerForward, directionToSignboard);

            if (angle <= signboardViewAngle)
            {
                stepCompleted[2] = true;
                ShowDialogue(2);
            }
        }
    }

    // ========== 조건 체크 함수 (DialogueStep용) ==========

    bool CheckCondition_Step0()
    {
        return true; // 자동 실행
    }

    bool CheckCondition_Step1()
    {
        if (playerTransform == null || npcTransform == null) return false;
        float distance = Vector3.Distance(playerTransform.position, npcTransform.position);
        return distance <= approachDistance;
    }

    bool CheckCondition_Step2()
    {
        if (playerTransform == null || signboardTransform == null) return false;
        Vector3 directionToSignboard = (signboardTransform.position - playerTransform.position).normalized;
        Vector3 playerForward = playerTransform.forward;
        float angle = Vector3.Angle(playerForward, directionToSignboard);
        return angle <= signboardViewAngle;
    }

    // ========== 대화 표시 ==========

    IEnumerator DelayedShowDialogue(int step, float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowDialogue(step);
    }

    void ShowDialogue(int step)
    {
        if (step >= dialogueSteps.Length) return;
        if (isTyping) return; // 이미 대화 중이면 무시

        currentStep = step + 1; // 다음 단계로
        DialogueStep dialogueStep = dialogueSteps[step];

        Debug.Log($"💬 대화 {step}: {dialogueStep.text}");

        // 대화창 표시
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (npcNameText != null) npcNameText.text = dialogueStep.npcName;
        if (continuePrompt != null) continuePrompt.SetActive(false); // 타이핑 중엔 숨김

        // 타이핑 효과
        StartCoroutine(TypeText(dialogueStep.text));
    }

    IEnumerator TypeText(string text)
    {
        isTyping = true;
        currentFullText = text;
        if (dialogueText != null) dialogueText.text = "";

        foreach (char c in text)
        {
            if (dialogueText != null) dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // autoHideDialogue가 true일 때만 자동으로 숨김
        if (autoHideDialogue)
        {
            yield return new WaitForSeconds(dialogueDisplayTime);
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (continuePrompt != null) continuePrompt.SetActive(false);
            Debug.Log("💬 대화창 자동 닫힘");
        }
        else
        {
            // 수동 모드: Continue Prompt 표시
            if (continuePrompt != null) continuePrompt.SetActive(true);
            Debug.Log("💬 Enter 키를 눌러 계속하세요...");
        }
    }

    // ========== 디버그용 기즈모 ==========
    void OnDrawGizmos()
    {
        // NPC 주변 접근 거리 표시
        if (npcTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(npcTransform.position, approachDistance);
        }

        // 전광판 방향 표시
        if (playerTransform != null && signboardTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(playerTransform.position, signboardTransform.position);
        }
    }
}

// 대화 단계 구조체
[System.Serializable]
public class DialogueStep
{
    public string npcName;
    [TextArea(2, 4)]
    public string text;
    public System.Func<bool> checkCondition; // 조건 체크 함수
}
