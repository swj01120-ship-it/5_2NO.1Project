using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    [Header("Settings")]
    public bool isGrabbed = false;              // 현재 잡혀있는지

    [Header("Highlight")]
    public Color normalColor = Color.white;     // 기본 색상
    public Color highlightColor = Color.yellow; // 하이라이트 색상

    private Rigidbody rb;
    private Renderer objectRenderer;
    private Material objectMaterial;
    private Color originalColor;
    private Vector3 lastPosition;
    private Vector3 velocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        objectRenderer = GetComponent<Renderer>();

        if (objectRenderer != null)
        {
            objectMaterial = objectRenderer.material;
            originalColor = objectMaterial.color;
        }

        lastPosition = transform.position;
    }

    void Update()
    {
        // 속도 계산 (던지기용)
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }

    // 하이라이트 표시
    public void Highlight(bool highlight)
    {
        if (objectMaterial != null && !isGrabbed)
        {
            objectMaterial.color = highlight ? highlightColor : originalColor;
        }
    }

    // 잡기
    public void Grab(Transform hand)
    {
        isGrabbed = true;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // 손의 자식으로 만들기
        transform.SetParent(hand);

        Debug.Log("물체 잡음: " + gameObject.name);
    }

    // 놓기
    public void Release()
    {
        isGrabbed = false;

        // 부모 제거
        transform.SetParent(null);

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        // 원래 색상 복원
        if (objectMaterial != null)
        {
            objectMaterial.color = originalColor;
        }

        Debug.Log("물체 놓음: " + gameObject.name);
    }

    // 던지기
    public void Throw(float throwForce = 5f)
    {
        Release();

        if (rb != null)
        {
            // 현재 속도 방향으로 힘 추가
            rb.velocity = velocity * throwForce;

            Debug.Log("물체 던짐: " + gameObject.name + ", 속도: " + velocity);
        }
    }

    // 속도 가져오기
    public Vector3 GetVelocity()
    {
        return velocity;
    }
}
