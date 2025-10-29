using UnityEngine;

public class GrabbableObject : MonoBehaviour
{
    [Header("Settings")]
    public bool isGrabbed = false;              // ���� �����ִ���

    [Header("Highlight")]
    public Color normalColor = Color.white;     // �⺻ ����
    public Color highlightColor = Color.yellow; // ���̶���Ʈ ����

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
        // �ӵ� ��� (�������)
        velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;
    }

    // ���̶���Ʈ ǥ��
    public void Highlight(bool highlight)
    {
        if (objectMaterial != null && !isGrabbed)
        {
            objectMaterial.color = highlight ? highlightColor : originalColor;
        }
    }

    // ���
    public void Grab(Transform hand)
    {
        isGrabbed = true;

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // ���� �ڽ����� �����
        transform.SetParent(hand);

        Debug.Log("��ü ����: " + gameObject.name);
    }

    // ����
    public void Release()
    {
        isGrabbed = false;

        // �θ� ����
        transform.SetParent(null);

        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        // ���� ���� ����
        if (objectMaterial != null)
        {
            objectMaterial.color = originalColor;
        }

        Debug.Log("��ü ����: " + gameObject.name);
    }

    // ������
    public void Throw(float throwForce = 5f)
    {
        Release();

        if (rb != null)
        {
            // ���� �ӵ� �������� �� �߰�
            rb.velocity = velocity * throwForce;

            Debug.Log("��ü ����: " + gameObject.name + ", �ӵ�: " + velocity);
        }
    }

    // �ӵ� ��������
    public Vector3 GetVelocity()
    {
        return velocity;
    }
}
