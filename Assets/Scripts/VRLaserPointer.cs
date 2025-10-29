using UnityEngine;
using UnityEngine.EventSystems;

public class VRLaserPointer : MonoBehaviour
{
    [Header("Laser Settings")]
    public float maxDistance = 10f;              // 레이저 최대 거리
    public LineRenderer laserLine;               // 레이저 선
    public Transform laserOrigin;                // 레이저 시작점

    [Header("Colors")]
    public Color normalColor = Color.cyan;       // 기본 색상
    public Color hoverColor = Color.yellow;      // 버튼 위 색상
    public Color clickColor = Color.green;       // 클릭 시 색상

    [Header("Input")]
    public KeyCode clickButton = KeyCode.Mouse0; // 클릭 버튼 (마우스 좌클릭)
                                                 // VR 컨트롤러 연결 시: OVRInput.Button.PrimaryIndexTrigger 등으로 변경

    private GameObject currentTarget;            // 현재 포인터가 가리키는 UI

    void Start()
    {
        // LineRenderer 자동 생성
        if (laserLine == null)
        {
            laserLine = gameObject.AddComponent<LineRenderer>();
            SetupLineRenderer();
        }

        // 레이저 시작점이 없으면 자기 자신
        if (laserOrigin == null)
        {
            laserOrigin = transform;
        }
    }

    void SetupLineRenderer()
    {
        laserLine.startWidth = 0.01f;
        laserLine.endWidth = 0.01f;
        laserLine.material = new Material(Shader.Find("Sprites/Default"));
        laserLine.startColor = normalColor;
        laserLine.endColor = normalColor;
        laserLine.positionCount = 2;
    }

    void Update()
    {
        UpdateLaser();
        HandleInput();
    }

    void UpdateLaser()
    {
        Ray ray = new Ray(laserOrigin.position, laserOrigin.forward);
        RaycastHit hit;

        Vector3 endPosition;

        // 레이캐스트로 충돌 감지
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            endPosition = hit.point;
            currentTarget = hit.collider.gameObject;

            // UI 버튼 위에 있는지 확인
            if (IsUIElement(hit.collider.gameObject))
            {
                laserLine.startColor = hoverColor;
                laserLine.endColor = hoverColor;
            }
            else
            {
                laserLine.startColor = normalColor;
                laserLine.endColor = normalColor;
            }
        }
        else
        {
            endPosition = laserOrigin.position + laserOrigin.forward * maxDistance;
            currentTarget = null;
            laserLine.startColor = normalColor;
            laserLine.endColor = normalColor;
        }

        // 레이저 선 그리기
        laserLine.SetPosition(0, laserOrigin.position);
        laserLine.SetPosition(1, endPosition);
    }

    void HandleInput()
    {
        // 클릭 감지
        if (Input.GetKeyDown(clickButton))
        {
            if (currentTarget != null)
            {
                // 클릭 효과
                laserLine.startColor = clickColor;
                laserLine.endColor = clickColor;

                // UI 버튼 클릭 처리
                ExecuteEvents.Execute(currentTarget, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);

                Debug.Log("클릭: " + currentTarget.name);
            }
        }

        // 버튼을 떼면 색상 복원
        if (Input.GetKeyUp(clickButton))
        {
            UpdateLaser();
        }
    }

    bool IsUIElement(GameObject obj)
    {
        // UI 요소인지 확인
        return obj.GetComponent<UnityEngine.UI.Selectable>() != null;
    }
}
