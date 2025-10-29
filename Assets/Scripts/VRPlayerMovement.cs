using UnityEngine;

public class VRPlayerMovement : MonoBehaviour
{
   
    [Header("Movement Settings")]
    public float moveSpeed = 3f;                // 이동 속도
    public float sprintSpeed = 5f;              // 달리기 속도
    public float rotationSpeed = 90f;           // 회전 속도

    [Header("Teleport Settings")]
    public float teleportDistance = 5f;         // 텔레포트 거리
    public LayerMask teleportMask;              // 텔레포트 가능한 레이어
    public GameObject teleportMarker;           // 텔레포트 표시

    [Header("Components")]
    public Transform vrCamera;                  // VR 카메라
    public CharacterController characterController;

    [Header("Input")]
    public KeyCode teleportKey = KeyCode.T;     // 텔레포트 키
    public KeyCode sprintKey = KeyCode.LeftShift; // 달리기 키

    private bool isTeleporting = false;
    private Vector3 teleportTarget;

    void Start()
    {
        
        // CharacterController 자동 추가
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.5f;
            characterController.center = new Vector3(0, 1, 0);
        }

        // VR Camera 자동 찾기
        if (vrCamera == null)
        {
            vrCamera = Camera.main.transform;
        }

        // 텔레포트 마커 생성
        CreateTeleportMarker();
        //CreateTeleportLine();
    }
    

    void Update()
    {
        HandleKeyboardMovement();
        HandleRotation();
        HandleTeleport();
        ApplyGravity();
    }

    // 키보드 이동 (테스트용)
    void HandleKeyboardMovement()
    {
        // WASD 입력
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        if (horizontal == 0 && vertical == 0) return;

        // 카메라 방향 기준으로 이동
        Vector3 forward = vrCamera.forward;
        Vector3 right = vrCamera.right;

        // Y축 제거 (수평 이동만)
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // 이동 방향 계산
        Vector3 moveDirection = (forward * vertical + right * horizontal);

        // 속도 적용 (Shift로 달리기)
        float speed = Input.GetKey(sprintKey) ? sprintSpeed : moveSpeed;

        // 이동
        characterController.Move(moveDirection * speed * Time.deltaTime);
    }

    // 회전 (Q/E 키)
    void HandleRotation()
    {
        float rotation = 0f;

        if (Input.GetKey(KeyCode.Q))
        {
            rotation = -rotationSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            rotation = rotationSpeed * Time.deltaTime;
        }

        if (rotation != 0)
        {
            transform.Rotate(0, rotation, 0);
        }
    }

    // 텔레포트
    void HandleTeleport()
    {
        // T 키를 누르고 있는 동안 텔레포트 대상 표시
        if (Input.GetKey(teleportKey))
        {
            ShowTeleportTarget();
        }
        else if (Input.GetKeyUp(teleportKey))
        {
            // T 키를 떼면 텔레포트 실행
            if (isTeleporting)
            {
                ExecuteTeleport();
            }
        }

        // 텔레포트 중이 아니면 마커 숨기기
        if (!Input.GetKey(teleportKey) && teleportMarker != null)
        {
            teleportMarker.SetActive(false);
        }
    }

    void ShowTeleportTarget()
    {
        Ray ray = new Ray(vrCamera.position, vrCamera.forward);
        RaycastHit hit;

         //바닥에 레이캐스트
        if (Physics.Raycast(ray, out hit, teleportDistance, teleportMask))
        {
            // 텔레포트 가능한 위치
            teleportTarget = hit.point;
            isTeleporting = true;

            // 마커 표시
            if (teleportMarker != null)
            {
                teleportMarker.SetActive(true);
                teleportMarker.transform.position = teleportTarget + Vector3.up * 0.1f;
            }
        }
        else
        {
            isTeleporting = false;
            if (teleportMarker != null)
            {
                teleportMarker.SetActive(false);
            }
        }
    }

    void ExecuteTeleport()
    {
        if (isTeleporting)
        {
            // 플레이어를 텔레포트 위치로 이동
            Vector3 newPosition = teleportTarget;
            newPosition.y = transform.position.y; // 높이 유지

            characterController.enabled = false;
            transform.position = newPosition;
            characterController.enabled = true;

            Debug.Log("텔레포트 완료: " + teleportTarget);
        }

        isTeleporting = false;
    }

    void ApplyGravity()
    {
        // 간단한 중력
        if (!characterController.isGrounded)
        {
            characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
        }
    }

    void CreateTeleportMarker()
    {
        // 텔레포트 마커 생성 (실린더)
        teleportMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        teleportMarker.name = "TeleportMarker";

        // 크기 조정
        teleportMarker.transform.localScale = new Vector3(1f, 0.05f, 1f);

        // Collider 제거
        Destroy(teleportMarker.GetComponent<Collider>());

        // 머터리얼 설정
        Renderer renderer = teleportMarker.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = new Color(0, 1, 0, 0.5f); // 반투명 초록
        renderer.material = mat;

        // 처음엔 숨김
        teleportMarker.SetActive(false);
    }
}
