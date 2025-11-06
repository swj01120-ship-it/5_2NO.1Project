using UnityEngine;
using UnityEngine.UI;

public class SliderRaycastController : MonoBehaviour
{
    [Header("슬라이더 참조")]
    [SerializeField] private Slider slider;

    [Header("Box Collider 설정")]
    [SerializeField] private BoxCollider boxCollider;

    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private Color hoverColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;

    private bool isDragging = false;
    private Material sliderMaterial;
    private Camera mainCamera;

    void Start()
    {
        // 자동으로 컴포넌트 찾기
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }

        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
        }

        if (slider == null)
        {
            Debug.LogError("❌ Slider 컴포넌트를 찾을 수 없습니다!");
            enabled = false;
            return;
        }

        if (boxCollider == null)
        {
            Debug.LogError("❌ BoxCollider 컴포넌트를 찾을 수 없습니다!");
            enabled = false;
            return;
        }

        // Box Collider를 Trigger로 설정
        boxCollider.isTrigger = true;

        // 메인 카메라 찾기
        mainCamera = Camera.main;

        if (showDebugLogs)
        {
            Debug.Log($"✅ SliderRaycastController 초기화 완료: {gameObject.name}");
        }

        // 슬라이더 색상 변경용 Material 가져오기 (선택사항)
        Image handleImage = slider.handleRect?.GetComponent<Image>();
        if (handleImage != null && handleImage.material != null)
        {
            sliderMaterial = handleImage.material;
        }
    }

    void Update()
    {
        // 레이저/마우스 클릭 감지 (RightController 또는 마우스)
        if (Input.GetMouseButton(0)) // 마우스 왼쪽 버튼 or VR Trigger
        {
            if (isDragging)
            {
                UpdateSliderFromRaycast();
            }
        }
        else
        {
            if (isDragging)
            {
                isDragging = false;
                if (showDebugLogs)
                {
                    Debug.Log($"🎚️ 슬라이더 드래그 종료");
                }
            }
        }
    }

    // 레이캐스트가 이 오브젝트를 hit 했을 때 호출 (외부 스크립트에서)
    public void OnRaycastHit(RaycastHit hit)
    {
        if (showDebugLogs)
        {
            Debug.Log($"🎯 슬라이더 히트: {gameObject.name}");
        }

        // 드래그 시작
        if (!isDragging)
        {
            isDragging = true;
            if (showDebugLogs)
            {
                Debug.Log($"🎚️ 슬라이더 드래그 시작");
            }
        }

        // 슬라이더 값 업데이트
        UpdateSliderValue(hit.point);
    }

    // 마우스/레이저로 슬라이더 값 업데이트
    void UpdateSliderFromRaycast()
    {
        Ray ray;

        // 마우스 또는 VR 컨트롤러에서 Ray 생성
        if (mainCamera != null)
        {
            ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        }
        else
        {
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider == boxCollider)
            {
                UpdateSliderValue(hit.point);
            }
        }
    }

    // hit point를 기반으로 슬라이더 값 계산 및 설정
    void UpdateSliderValue(Vector3 worldHitPoint)
    {
        if (slider == null) return;

        // World 좌표를 슬라이더의 로컬 좌표로 변환
        Vector3 localPoint = transform.InverseTransformPoint(worldHitPoint);

        // 슬라이더 방향에 따라 값 계산
        RectTransform rectTransform = slider.GetComponent<RectTransform>();
        if (rectTransform == null) return;

        float normalizedValue = 0f;

        if (slider.direction == Slider.Direction.LeftToRight ||
            slider.direction == Slider.Direction.RightToLeft)
        {
            // 가로 슬라이더
            float width = rectTransform.rect.width * transform.localScale.x;
            normalizedValue = Mathf.Clamp01((localPoint.x + width / 2f) / width);

            if (slider.direction == Slider.Direction.RightToLeft)
            {
                normalizedValue = 1f - normalizedValue;
            }
        }
        else
        {
            // 세로 슬라이더
            float height = rectTransform.rect.height * transform.localScale.y;
            normalizedValue = Mathf.Clamp01((localPoint.y + height / 2f) / height);

            if (slider.direction == Slider.Direction.TopToBottom)
            {
                normalizedValue = 1f - normalizedValue;
            }
        }

        // 슬라이더 값 설정
        float newValue = Mathf.Lerp(slider.minValue, slider.maxValue, normalizedValue);
        slider.value = newValue;

        if (showDebugLogs)
        {
            Debug.Log($"🎚️ 슬라이더 값: {slider.value:F2}");
        }
    }

    // 레이저 호버 (색상 변경용, 선택사항)
    public void OnRaycastEnter()
    {
        if (showDebugLogs)
        {
            Debug.Log($"🎯 슬라이더 호버 시작: {gameObject.name}");
        }
        // 색상 변경 등의 피드백
    }

    public void OnRaycastExit()
    {
        if (showDebugLogs)
        {
            Debug.Log($"🎯 슬라이더 호버 종료: {gameObject.name}");
        }
    }

    // 디버그 시각화
    void OnDrawGizmos()
    {
        if (boxCollider != null)
        {
            Gizmos.color = isDragging ? Color.yellow : Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
    }
}
