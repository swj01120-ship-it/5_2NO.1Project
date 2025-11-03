using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -20f;

    [Header("카메라 설정 (1인칭)")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float cameraMaxAngle = 80f;
    [SerializeField] private float cameraHeight = 0.8f;

    [Header("지면 체크")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("UI 상호작용")]
    [SerializeField] private bool enableUIInteraction = true;

    [Header("헤드 밥 (걸을 때 카메라 흔들림)")]
    [SerializeField] private bool enableHeadBob = true;
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float bobAmplitude = 0.05f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;
    private bool cursorLocked = true;

    // 헤드 밥 변수
    private float bobTimer = 0f;
    private Vector3 cameraStartPosition;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        LockCursor();
    }

    private void Start()
    {
        // 카메라 설정
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // 카메라를 플레이어의 자식으로 만들고 1인칭 위치로 설정
        if (playerCamera != null)
        {
            playerCamera.transform.SetParent(transform);
            playerCamera.transform.localPosition = new Vector3(0, cameraHeight, 0);
            playerCamera.transform.localRotation = Quaternion.identity;
            cameraStartPosition = playerCamera.transform.localPosition;
        }
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();

        // UI와 상호작용할 때는 마우스 시점 비활성화
        if (cursorLocked)
        {
            HandleMouseLook();
        }

        HandleHeadBob();
        HandleCursorToggle();
    }

    // 지면 체크 (개선된 버전)
    private void HandleGroundCheck()
    {
        // CharacterController의 isGrounded 사용
        isGrounded = controller.isGrounded;

        // 추가 체크: 발 아래 레이캐스트
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        if (Physics.Raycast(ray, 0.3f))
        {
            isGrounded = true;
        }

        // 바닥에 닿았을 때 y 속도 리셋
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 디버그 로그 (테스트용)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"점프 시도! isGrounded: {isGrounded}, Velocity.y: {velocity.y}");
        }
    }

    // 이동 처리
    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 플레이어가 바라보는 방향 기준으로 이동
        Vector3 move = transform.right * horizontal + transform.forward * vertical;

        // 달리기
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        // 점프 (스페이스바 또는 Jump 버튼)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            Debug.Log("점프!");
        }

        // 중력 적용
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // 1인칭 마우스 시점
    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 상하 시점 (카메라만 회전)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -cameraMaxAngle, cameraMaxAngle);

        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        // 좌우 시점 (플레이어 전체 회전)
        transform.Rotate(Vector3.up * mouseX);
    }

    // 헤드 밥 (걸을 때 카메라 자연스러운 흔들림)
    private void HandleHeadBob()
    {
        if (!enableHeadBob || playerCamera == null) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // 움직이고 있고 지면에 있을 때만
        if (isGrounded && (Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f))
        {
            bobTimer += Time.deltaTime * bobFrequency;

            // 사인파를 이용한 상하 움직임
            float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;

            playerCamera.transform.localPosition = new Vector3(
                cameraStartPosition.x,
                cameraStartPosition.y + bobOffset,
                cameraStartPosition.z
            );
        }
        else
        {
            // 멈추면 원래 위치로 부드럽게 돌아감
            bobTimer = 0f;
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition,
                cameraStartPosition,
                Time.deltaTime * 5f
            );
        }
    }

    // ESC 키로 커서 토글 (UI 클릭 가능하게)
    private void HandleCursorToggle()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (cursorLocked)
            {
                UnlockCursor();
            }
            else
            {
                LockCursor();
            }
        }

        // 마우스 왼쪽 클릭으로도 다시 잠금 (UI 클릭 후 게임으로 돌아갈 때)
        if (!cursorLocked && Input.GetMouseButtonDown(0))
        {
            // UI를 클릭하지 않았을 때만 커서 잠금
            if (enableUIInteraction && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                LockCursor();
            }
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorLocked = true;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorLocked = false;
    }

    // 디버깅용 기즈모
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, groundCheckDistance);
    }
}