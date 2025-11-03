using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = 20f;

    [Header("마우스 감도")]
    [SerializeField] private float lookSpeedX = 2f;
    [SerializeField] private float lookSpeedY = 2f;
    [SerializeField] private float lookLimitY = 80f;

    [Header("카메라 설정")]
    [SerializeField] private Transform cameraRig; // VRControllers 또는 카메라 부모
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private bool useVRCamera = true;
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 0.6f, 0);

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0;
    private bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        // 카메라 자동 찾기
        if (cameraTransform == null || cameraRig == null)
        {
            // VR 카메라 찾기
            GameObject vrControllers = GameObject.Find("VRControllers");
            if (vrControllers != null && useVRCamera)
            {
                cameraRig = vrControllers.transform;
                cameraTransform = vrControllers.GetComponentInChildren<Camera>()?.transform;
                Debug.Log("VR 카메라 시스템 감지됨!");
            }
            else
            {
                // 일반 카메라
                Camera mainCam = Camera.main;
                if (mainCam != null)
                {
                    cameraTransform = mainCam.transform;
                    cameraRig = cameraTransform;
                    Debug.Log("일반 카메라 사용");
                }
            }
        }

        // 초기 카메라 위치 설정
        if (cameraRig != null)
        {
            cameraRig.position = transform.position + cameraOffset;
        }

        // 마우스 커서 잠금
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();

        // 카메라를 플레이어 위치로 동기화
        SyncCameraToPlayer();

        // VR 모드가 아닐 때만 마우스 시점 제어
        if (canMove)
        {
            HandleMouseLook();
        }

        HandleCursor();
    }

    void HandleMovement()
    {
        // 앞뒤좌우 입력
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // 디버그: 입력 확인
        if (moveX != 0 || moveZ != 0)
        {
            Debug.Log($"입력 감지! X: {moveX}, Z: {moveZ}");
        }

        // 이동 방향 계산 (플레이어 기준)
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // 달리기 체크
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        float moveY = moveDirection.y;
        moveDirection = (forward * moveZ + right * moveX) * currentSpeed;

        // 점프
        if (Input.GetButton("Jump") && characterController.isGrounded)
        {
            moveDirection.y = jumpForce;
            Debug.Log("점프!");
        }
        else
        {
            moveDirection.y = moveY;
        }

        // 중력 적용
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // 캐릭터 이동
        characterController.Move(moveDirection * Time.deltaTime);
    }

    // 카메라를 플레이어 위치에 동기화
    void SyncCameraToPlayer()
    {
        if (cameraRig != null)
        {
            // VRControllers(카메라)를 플레이어 위치로 이동
            // 단, VR 헤드셋의 로컬 회전/위치는 유지
            cameraRig.position = transform.position + cameraOffset;

            // 플레이어의 Y축 회전만 카메라에 적용 (좌우 회전)
            Vector3 currentRotation = cameraRig.eulerAngles;
            cameraRig.rotation = Quaternion.Euler(currentRotation.x, transform.eulerAngles.y, currentRotation.z);
        }
    }

    void HandleMouseLook()
    {
        if (!canMove) return;
        if (cameraTransform == null) return;

        // 마우스 입력
        float mouseX = Input.GetAxis("Mouse X") * lookSpeedX;
        float mouseY = Input.GetAxis("Mouse Y") * lookSpeedY;

        // 좌우 회전 (플레이어)
        transform.Rotate(0, mouseX, 0);

        // 상하 회전 (카메라)
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -lookLimitY, lookLimitY);
        cameraTransform.localRotation = Quaternion.Euler(rotationX, 0, 0);
    }

    void HandleCursor()
    {
        // ESC로 커서 토글
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                canMove = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                canMove = true;
            }
        }

        // 화면 클릭으로 다시 잠금
        if (Cursor.visible && Input.GetMouseButtonDown(0))
        {
            // UI 위가 아닐 때만
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                canMove = true;
            }
        }
    }
}