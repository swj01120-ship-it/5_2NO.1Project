using UnityEngine;

public class VRPlayerMovement : MonoBehaviour
{
   
    [Header("Movement Settings")]
    public float moveSpeed = 3f;                // �̵� �ӵ�
    public float sprintSpeed = 5f;              // �޸��� �ӵ�
    public float rotationSpeed = 90f;           // ȸ�� �ӵ�

    [Header("Teleport Settings")]
    public float teleportDistance = 5f;         // �ڷ���Ʈ �Ÿ�
    public LayerMask teleportMask;              // �ڷ���Ʈ ������ ���̾�
    public GameObject teleportMarker;           // �ڷ���Ʈ ǥ��

    [Header("Components")]
    public Transform vrCamera;                  // VR ī�޶�
    public CharacterController characterController;

    [Header("Input")]
    public KeyCode teleportKey = KeyCode.T;     // �ڷ���Ʈ Ű
    public KeyCode sprintKey = KeyCode.LeftShift; // �޸��� Ű

    private bool isTeleporting = false;
    private Vector3 teleportTarget;

    void Start()
    {
        
        // CharacterController �ڵ� �߰�
        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            characterController.height = 2f;
            characterController.radius = 0.5f;
            characterController.center = new Vector3(0, 1, 0);
        }

        // VR Camera �ڵ� ã��
        if (vrCamera == null)
        {
            vrCamera = Camera.main.transform;
        }

        // �ڷ���Ʈ ��Ŀ ����
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

    // Ű���� �̵� (�׽�Ʈ��)
    void HandleKeyboardMovement()
    {
        // WASD �Է�
        float horizontal = Input.GetAxis("Horizontal"); // A/D
        float vertical = Input.GetAxis("Vertical");     // W/S

        if (horizontal == 0 && vertical == 0) return;

        // ī�޶� ���� �������� �̵�
        Vector3 forward = vrCamera.forward;
        Vector3 right = vrCamera.right;

        // Y�� ���� (���� �̵���)
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // �̵� ���� ���
        Vector3 moveDirection = (forward * vertical + right * horizontal);

        // �ӵ� ���� (Shift�� �޸���)
        float speed = Input.GetKey(sprintKey) ? sprintSpeed : moveSpeed;

        // �̵�
        characterController.Move(moveDirection * speed * Time.deltaTime);
    }

    // ȸ�� (Q/E Ű)
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

    // �ڷ���Ʈ
    void HandleTeleport()
    {
        // T Ű�� ������ �ִ� ���� �ڷ���Ʈ ��� ǥ��
        if (Input.GetKey(teleportKey))
        {
            ShowTeleportTarget();
        }
        else if (Input.GetKeyUp(teleportKey))
        {
            // T Ű�� ���� �ڷ���Ʈ ����
            if (isTeleporting)
            {
                ExecuteTeleport();
            }
        }

        // �ڷ���Ʈ ���� �ƴϸ� ��Ŀ �����
        if (!Input.GetKey(teleportKey) && teleportMarker != null)
        {
            teleportMarker.SetActive(false);
        }
    }

    void ShowTeleportTarget()
    {
        Ray ray = new Ray(vrCamera.position, vrCamera.forward);
        RaycastHit hit;

         //�ٴڿ� ����ĳ��Ʈ
        if (Physics.Raycast(ray, out hit, teleportDistance, teleportMask))
        {
            // �ڷ���Ʈ ������ ��ġ
            teleportTarget = hit.point;
            isTeleporting = true;

            // ��Ŀ ǥ��
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
            // �÷��̾ �ڷ���Ʈ ��ġ�� �̵�
            Vector3 newPosition = teleportTarget;
            newPosition.y = transform.position.y; // ���� ����

            characterController.enabled = false;
            transform.position = newPosition;
            characterController.enabled = true;

            Debug.Log("�ڷ���Ʈ �Ϸ�: " + teleportTarget);
        }

        isTeleporting = false;
    }

    void ApplyGravity()
    {
        // ������ �߷�
        if (!characterController.isGrounded)
        {
            characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
        }
    }

    void CreateTeleportMarker()
    {
        // �ڷ���Ʈ ��Ŀ ���� (�Ǹ���)
        teleportMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        teleportMarker.name = "TeleportMarker";

        // ũ�� ����
        teleportMarker.transform.localScale = new Vector3(1f, 0.05f, 1f);

        // Collider ����
        Destroy(teleportMarker.GetComponent<Collider>());

        // ���͸��� ����
        Renderer renderer = teleportMarker.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = new Color(0, 1, 0, 0.5f); // ������ �ʷ�
        renderer.material = mat;

        // ó���� ����
        teleportMarker.SetActive(false);
    }
}
