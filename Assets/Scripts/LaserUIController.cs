using UnityEngine;

public class LaserUIController : MonoBehaviour
{
    [Header("레이저 설정")]
    [SerializeField] private float maxDistance = 10f;
    [SerializeField] private LayerMask uiLayer; // UI 레이어만 감지

    [Header("레이저 비주얼")]
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lineWidth = 0.005f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = Color.green;
    [SerializeField] private Color clickColor = Color.yellow;

    [Header("입력 설정")]
    [SerializeField] private KeyCode clickButton = KeyCode.Mouse0; // 테스트용 (마우스)
    [SerializeField] private string vrTriggerButton = "Fire1"; // VR 트리거

    [Header("포인터 점")]
    [SerializeField] private GameObject pointerDot;

    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;

    private GameObject currentHoverObject;
    private SliderRaycastController currentSlider;
    private bool isClicking = false;

    void Start()
    {
        // LineRenderer 설정
        if (lineRenderer == null)
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }

        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = normalColor;
        lineRenderer.endColor = normalColor;
        lineRenderer.positionCount = 2;

        // UI Layer 자동 설정
        if (uiLayer == 0)
        {
            uiLayer = LayerMask.GetMask("UI");
        }

        if (showDebugLogs)
        {
            Debug.Log($"✅ LaserUIController 초기화: {gameObject.name}");
        }
    }

    void Update()
    {
        PerformRaycast();
        HandleInput();
    }

    void PerformRaycast()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, uiLayer))
        {
            // 레이저 그리기 (hit 지점까지)
            DrawLaser(transform.position, hit.point);

            // 포인터 점 표시
            if (pointerDot != null)
            {
                pointerDot.SetActive(true);
                pointerDot.transform.position = hit.point;
            }

            GameObject hitObject = hit.collider.gameObject;

            // 새로운 오브젝트 호버
            if (hitObject != currentHoverObject)
            {
                // 이전 오브젝트 호버 종료
                if (currentHoverObject != null)
                {
                    SliderRaycastController prevSlider = currentHoverObject.GetComponent<SliderRaycastController>();
                    if (prevSlider != null)
                    {
                        prevSlider.OnRaycastExit();
                    }
                }

                // 새 오브젝트 호버 시작
                currentHoverObject = hitObject;
                currentSlider = currentHoverObject.GetComponent<SliderRaycastController>();

                if (currentSlider != null)
                {
                    currentSlider.OnRaycastEnter();
                    lineRenderer.startColor = hoverColor;
                    lineRenderer.endColor = hoverColor;

                    if (showDebugLogs)
                    {
                        Debug.Log($"🎯 UI 호버: {hitObject.name}");
                    }
                }
            }

            // 클릭 중이면 슬라이더에 hit 정보 전달
            if (isClicking && currentSlider != null)
            {
                currentSlider.OnRaycastHit(hit);
                lineRenderer.startColor = clickColor;
                lineRenderer.endColor = clickColor;
            }
        }
        else
        {
            // Hit 없음 - 레이저를 최대 거리까지 그리기
            DrawLaser(transform.position, transform.position + transform.forward * maxDistance);

            if (pointerDot != null)
            {
                pointerDot.SetActive(false);
            }

            // 호버 종료
            if (currentHoverObject != null)
            {
                if (currentSlider != null)
                {
                    currentSlider.OnRaycastExit();
                }

                currentHoverObject = null;
                currentSlider = null;
                lineRenderer.startColor = normalColor;
                lineRenderer.endColor = normalColor;
            }
        }
    }

    void HandleInput()
    {
        bool clickPressed = Input.GetKeyDown(clickButton) || Input.GetButtonDown(vrTriggerButton);
        bool clickHeld = Input.GetKey(clickButton) || Input.GetButton(vrTriggerButton);
        bool clickReleased = Input.GetKeyUp(clickButton) || Input.GetButtonUp(vrTriggerButton);

        // 클릭 시작
        if (clickPressed && currentHoverObject != null)
        {
            isClicking = true;

            if (showDebugLogs)
            {
                Debug.Log($"🖱️ UI 클릭 시작: {currentHoverObject.name}");
            }
        }

        // 클릭 해제
        if (clickReleased)
        {
            if (isClicking && showDebugLogs)
            {
                Debug.Log($"🖱️ UI 클릭 종료");
            }

            isClicking = false;

            // 색상 복원
            if (currentHoverObject != null)
            {
                lineRenderer.startColor = hoverColor;
                lineRenderer.endColor = hoverColor;
            }
            else
            {
                lineRenderer.startColor = normalColor;
                lineRenderer.endColor = normalColor;
            }
        }
    }

    void DrawLaser(Vector3 start, Vector3 end)
    {
        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(1, end);
    }

    void OnDrawGizmos()
    {
        // Scene 뷰에서 레이 표시
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, transform.forward * maxDistance);
    }
}
