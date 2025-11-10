using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true; // 디버그 로그 표시

    private GameObject currentTarget;            // 현재 포인터가 가리키는 UI
    private Slider currentSlider;                // 현재 드래그 중인 슬라이더
    private bool isDragging = false;             // 드래그 중인지 여부
    private RaycastHit currentHit;               // 현재 hit 정보

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

        if (showDebugLogs)
        {
            Debug.Log("✅ VRLaserPointer 초기화 완료");
        }
    }

    void SetupLineRenderer()
    {
        laserLine.startWidth = 0.02f; // 굵게
        laserLine.endWidth = 0.01f;

        // Material 설정 개선
        Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
        lineMaterial.color = normalColor;
        laserLine.material = lineMaterial;

        // 색상 설정
        laserLine.startColor = normalColor;
        laserLine.endColor = normalColor;

        // 포지션 개수
        laserLine.positionCount = 2;

        // 추가 설정 (Game 뷰에서 보이게)
        laserLine.useWorldSpace = true;
        laserLine.sortingOrder = 1000; // 다른 UI 위에 표시
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
            currentHit = hit; // hit 정보 저장

            // UI 버튼 위에 있는지 확인
            if (IsUIElement(hit.collider.gameObject))
            {
                laserLine.startColor = isDragging ? clickColor : hoverColor;
                laserLine.endColor = isDragging ? clickColor : hoverColor;

                if (showDebugLogs && currentTarget != currentSlider?.gameObject)
                {
                    Debug.Log($"🎯 UI 호버: {currentTarget.name}");
                }
            }
            else
            {
                laserLine.startColor = normalColor;
                laserLine.endColor = normalColor;
            }

            // 드래그 중이면 슬라이더 값 업데이트
            if (isDragging && currentSlider != null)
            {
                UpdateSliderValue(hit);
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
        // 클릭 시작
        if (Input.GetKeyDown(clickButton))
        {
            if (currentTarget != null)
            {
                // 클릭 효과
                laserLine.startColor = clickColor;
                laserLine.endColor = clickColor;

                // 슬라이더인지 확인
                Slider slider = currentTarget.GetComponent<Slider>();
                if (slider == null)
                {
                    // 슬라이더가 아니면 부모에서 찾기
                    slider = currentTarget.GetComponentInParent<Slider>();
                }

                if (slider != null)
                {
                    // 슬라이더 드래그 시작
                    currentSlider = slider;
                    isDragging = true;
                    UpdateSliderValue(currentHit);

                    if (showDebugLogs)
                    {
                        Debug.Log($"🎚️ 슬라이더 드래그 시작: {slider.gameObject.name}");
                    }
                }
                else
                {
                    // ✅ 일반 버튼 클릭 처리 개선
                    // 먼저 Button 컴포넌트 직접 찾기
                    Button button = currentTarget.GetComponent<Button>();
                    if (button == null)
                    {
                        button = currentTarget.GetComponentInParent<Button>();
                    }

                    if (button != null && button.interactable)
                    {
                        // Button.onClick 직접 호출
                        button.onClick.Invoke();

                        if (showDebugLogs)
                        {
                            Debug.Log($"✅ 버튼 클릭 성공: {button.gameObject.name}");
                        }
                    }
                    else
                    {
                        // 버튼이 아니면 EventSystem 사용
                        ExecuteEvents.Execute(currentTarget, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);

                        if (showDebugLogs)
                        {
                            Debug.Log($"🖱️ 클릭: {currentTarget.name}");
                        }
                    }
                }
            }
        }

        // 클릭 유지 중
        if (Input.GetKey(clickButton))
        {
            if (isDragging && currentSlider != null && currentTarget != null)
            {
                // UpdateLaser()에서 이미 처리되고 있음
            }
        }

        // 클릭 해제
        if (Input.GetKeyUp(clickButton))
        {
            if (isDragging)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"🎚️ 슬라이더 드래그 종료: {currentSlider?.value:F2}");
                }

                isDragging = false;
                currentSlider = null;
            }

            // 색상 복원
            UpdateLaser();
        }
    }

    void UpdateSliderValue(RaycastHit hit)
    {
        if (currentSlider == null) return;

        // World 좌표를 슬라이더의 로컬 좌표로 변환
        Transform sliderTransform = currentSlider.transform;
        Vector3 localPoint = sliderTransform.InverseTransformPoint(hit.point);

        // 슬라이더 RectTransform 가져오기
        RectTransform rectTransform = currentSlider.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        float normalizedValue = 0f;

        // 슬라이더 방향에 따라 값 계산
        if (currentSlider.direction == Slider.Direction.LeftToRight ||
            currentSlider.direction == Slider.Direction.RightToLeft)
        {
            // 가로 슬라이더
            float width = rectTransform.rect.width * sliderTransform.localScale.x;
            normalizedValue = Mathf.Clamp01((localPoint.x + width / 2f) / width);

            if (currentSlider.direction == Slider.Direction.RightToLeft)
            {
                normalizedValue = 1f - normalizedValue;
            }
        }
        else
        {
            // 세로 슬라이더
            float height = rectTransform.rect.height * sliderTransform.localScale.y;
            normalizedValue = Mathf.Clamp01((localPoint.y + height / 2f) / height);

            if (currentSlider.direction == Slider.Direction.TopToBottom)
            {
                normalizedValue = 1f - normalizedValue;
            }
        }

        // 슬라이더 값 설정
        float newValue = Mathf.Lerp(currentSlider.minValue, currentSlider.maxValue, normalizedValue);
        currentSlider.value = newValue;

        if (showDebugLogs)
        {
            Debug.Log($"🎚️ 슬라이더 값: {currentSlider.value:F2}");
        }
    }

    bool IsUIElement(GameObject obj)
    {
        // UI 요소인지 확인 (Selectable 또는 Slider)
        return obj.GetComponent<UnityEngine.UI.Selectable>() != null ||
               obj.GetComponentInParent<UnityEngine.UI.Selectable>() != null;
    }
}



