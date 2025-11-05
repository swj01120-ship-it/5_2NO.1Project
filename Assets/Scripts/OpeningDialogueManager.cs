using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class OpeningDialogueManager : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Text npcNameText;
    [SerializeField] private Text dialogueText;
    [SerializeField] private GameObject continueButton;

    [Header("NPC 설정")]
    [SerializeField] private Transform npcTransform; // 호랭도령
    [SerializeField] private bool waitForGroundLanding = true; // Ground 착지 대기
    [SerializeField] private float dialogueDelayAfterLanding = 2f; // 착지 후 대기 시간

    [Header("착지 감지 설정")]
    [SerializeField] private float groundCheckDistance = 0.5f; // 지면 체크 거리
    [SerializeField] private LayerMask groundLayer; // Ground 레이어

    [Header("타이핑 효과")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float delayBetweenDialogues = 1f;

    [Header("씬 전환")]
    [SerializeField] private string nextSceneName = "MainMenu";
    [SerializeField] private float sceneTransitionDelay = 2f;

    private int currentDialogueIndex = 0;
    private bool isTyping = false;
    private bool dialogueStarted = false;
    private bool hasLanded = false;
    private string currentFullText = "";

    // 대화 내용
    private DialogueLine[] dialogueLines;

    void Start()
    {
        // 대화 내용 초기화
        InitializeDialogues();

        // UI 초기화
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // 착지 대기를 사용하지 않으면 바로 시작
        if (!waitForGroundLanding)
        {
            StartCoroutine(AutoStartDialogue(2f));
        }
    }

    void Update()
    {
        // 착지 대기 중이고, 대화가 아직 시작 안 했고, NPC가 설정되어 있으면
        if (waitForGroundLanding && !dialogueStarted && !hasLanded && npcTransform != null)
        {
            // Ground 착지 체크
            if (IsGrounded())
            {
                hasLanded = true;
                Debug.Log("✅ NPC가 Ground에 착지했습니다!");
                StartCoroutine(AutoStartDialogue(dialogueDelayAfterLanding));
            }
        }

        // Enter 키로 대화 진행
        if (dialogueStarted && Input.GetKeyDown(KeyCode.Return) && !isTyping)
        {
            NextDialogue();
        }

        // 타이핑 중 Enter로 스킵
        if (Input.GetKeyDown(KeyCode.Return) && isTyping)
        {
            StopAllCoroutines();
            dialogueText.text = currentFullText;
            isTyping = false;
            if (continueButton != null) continueButton.SetActive(true);
        }
    }

    // NPC가 Ground에 착지했는지 체크
    bool IsGrounded()
    {
        if (npcTransform == null) return false;

        // Raycast로 아래쪽 확인
        RaycastHit hit;
        Vector3 rayOrigin = npcTransform.position;

        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            Debug.DrawRay(rayOrigin, Vector3.down * groundCheckDistance, Color.green);
            return true;
        }

        Debug.DrawRay(rayOrigin, Vector3.down * groundCheckDistance, Color.red);
        return false;
    }

    void InitializeDialogues()
    {
        dialogueLines = new DialogueLine[]
        {
            new DialogueLine
            {
                npcName = "호랭도령",
                text = "자네가 이번에 새롭게 탑승하게 될 친구인가?"
            },
            new DialogueLine
            {
                npcName = "호랭도령",
                text = "오래 기다렸네...버스에 탑승하기 전 몇 가지 알려주겠네!"
            },
            new DialogueLine
            {
                npcName = "호랭도령",
                text = "자..준비가 되었으면 Enter 키를 눌러보게나"
            }
        };
    }

    IEnumerator AutoStartDialogue(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartDialogue();
    }

    void StartDialogue()
    {
        if (dialogueStarted) return;

        dialogueStarted = true;
        currentDialogueIndex = 0;

        Debug.Log("🎬 오프닝 대화 시작!");

        ShowCurrentDialogue();
    }

    void ShowCurrentDialogue()
    {
        if (currentDialogueIndex >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = dialogueLines[currentDialogueIndex];

        // 대화창 표시
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (npcNameText != null) npcNameText.text = line.npcName;

        // 타이핑 효과
        StartCoroutine(TypeText(line.text));
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

    void NextDialogue()
    {
        currentDialogueIndex++;

        if (currentDialogueIndex < dialogueLines.Length)
        {
            StartCoroutine(ShowNextDialogueAfterDelay());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator ShowNextDialogueAfterDelay()
    {
        yield return new WaitForSeconds(delayBetweenDialogues);
        ShowCurrentDialogue();
    }

    void EndDialogue()
    {
        Debug.Log("🎬 오프닝 대화 완료!");

        // 대화창 숨기기
        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        // 다음 씬으로 전환
        StartCoroutine(TransitionToNextScene());
    }

    IEnumerator TransitionToNextScene()
    {
        Debug.Log($"⏳ {sceneTransitionDelay}초 후 {nextSceneName} 씬으로 이동...");

        yield return new WaitForSeconds(sceneTransitionDelay);

        Debug.Log($"🎬 {nextSceneName} 씬으로 전환!");
        SceneManager.LoadScene(nextSceneName);
    }

    // 외부에서 수동으로 대화 시작 (버튼이나 애니메이션 이벤트에서 호출)
    public void TriggerDialogue()
    {
        StartDialogue();
    }
}

// 대화 라인 구조체
[System.Serializable]
public class DialogueLine
{
    public string npcName;
    [TextArea(2, 4)]
    public string text;
}