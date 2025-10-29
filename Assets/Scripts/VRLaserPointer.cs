using UnityEngine;
using UnityEngine.EventSystems;

public class VRLaserPointer : MonoBehaviour
{
    [Header("Laser Settings")]
    public float maxDistance = 10f;              // ������ �ִ� �Ÿ�
    public LineRenderer laserLine;               // ������ ��
    public Transform laserOrigin;                // ������ ������

    [Header("Colors")]
    public Color normalColor = Color.cyan;       // �⺻ ����
    public Color hoverColor = Color.yellow;      // ��ư �� ����
    public Color clickColor = Color.green;       // Ŭ�� �� ����

    [Header("Input")]
    public KeyCode clickButton = KeyCode.Mouse0; // Ŭ�� ��ư (���콺 ��Ŭ��)
                                                 // VR ��Ʈ�ѷ� ���� ��: OVRInput.Button.PrimaryIndexTrigger ������ ����

    private GameObject currentTarget;            // ���� �����Ͱ� ����Ű�� UI

    void Start()
    {
        // LineRenderer �ڵ� ����
        if (laserLine == null)
        {
            laserLine = gameObject.AddComponent<LineRenderer>();
            SetupLineRenderer();
        }

        // ������ �������� ������ �ڱ� �ڽ�
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

        // ����ĳ��Ʈ�� �浹 ����
        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            endPosition = hit.point;
            currentTarget = hit.collider.gameObject;

            // UI ��ư ���� �ִ��� Ȯ��
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

        // ������ �� �׸���
        laserLine.SetPosition(0, laserOrigin.position);
        laserLine.SetPosition(1, endPosition);
    }

    void HandleInput()
    {
        // Ŭ�� ����
        if (Input.GetKeyDown(clickButton))
        {
            if (currentTarget != null)
            {
                // Ŭ�� ȿ��
                laserLine.startColor = clickColor;
                laserLine.endColor = clickColor;

                // UI ��ư Ŭ�� ó��
                ExecuteEvents.Execute(currentTarget, new PointerEventData(EventSystem.current), ExecuteEvents.pointerClickHandler);

                Debug.Log("Ŭ��: " + currentTarget.name);
            }
        }

        // ��ư�� ���� ���� ����
        if (Input.GetKeyUp(clickButton))
        {
            UpdateLaser();
        }
    }

    bool IsUIElement(GameObject obj)
    {
        // UI ������� Ȯ��
        return obj.GetComponent<UnityEngine.UI.Selectable>() != null;
    }
}
