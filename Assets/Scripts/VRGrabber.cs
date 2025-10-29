using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRGrabber : MonoBehaviour
{
    [Header("Grab Settings")]
    public float grabDistance = 2f;             // ��� �Ÿ�
    public LayerMask grabbableLayer;            // ���� �� �ִ� ���̾�
    public Transform hand;                      // �� ��ġ

    [Header("Input")]
    public KeyCode grabKey = KeyCode.G;         // ��� Ű
    public KeyCode throwKey = KeyCode.F;        // ������ Ű

    [Header("Throw")]
    public float throwForce = 10f;               // ������ ��

    private GrabbableObject currentTarget;      // ���� �ܳ��� ��ü
    private GrabbableObject grabbedObject;      // ���� ���� ��ü
    private LineRenderer grabLine;              // ��� ǥ�ü�

    void Start()
    {
        // ���� ������ �ڱ� �ڽ�
        if (hand == null)
        {
            hand = transform;
        }

        CreateGrabLine();
    }

    void Update()
    {
        if (grabbedObject == null)
        {
            // ��ü�� ���� �ʾ��� ��
            DetectGrabbableObject();
            HandleGrab();
        }
        else
        {
            // ��ü�� ����� ��
            HandleRelease();
            HandleThrow();
        }

        UpdateGrabLine();
    }

    void DetectGrabbableObject()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        Debug.DrawRay(transform.position,transform.forward*grabDistance, Color.red);

        // ���� Ÿ�� ���̶���Ʈ ����
        if (currentTarget != null)
        {
            currentTarget.Highlight(false);
            currentTarget = null;
        }

        // ����ĳ��Ʈ�� ��ü ����
        if (Physics.Raycast(ray, out hit, grabDistance, grabbableLayer))
        {
            Debug.Log("����ĳ��Ʈ ��Ʈ: " + hit.collider.name); // ����� �߰�!

            GrabbableObject grabbable = hit.collider.GetComponent<GrabbableObject>();

            if (grabbable != null && !grabbable.isGrabbed)
            {
                currentTarget = grabbable;
                currentTarget.Highlight(true);
                Debug.Log("���� �� �ִ� ��ü ����: " + grabbable.name); // ����� �߰�!
            }
            else
            {
                Debug.Log("GrabbableObject ������Ʈ ���� �Ǵ� �̹� ����"); // ����� �߰�!
            }
        }
        else
        {
            //Debug.Log("����ĳ��Ʈ �̽�"); // ����� �߰�!
        }
    }

    void HandleGrab()
    {
        if (Input.GetKeyDown(grabKey))
        {
            if (currentTarget != null)
            {
                grabbedObject = currentTarget;
                grabbedObject.Grab(hand);
                currentTarget = null;
            }
        }
    }

    void HandleRelease()
    {
        if (Input.GetKeyDown(grabKey))
        {
            if (grabbedObject != null)
            {
                grabbedObject.Release();
                grabbedObject = null;
            }
        }
    }

    void HandleThrow()
    {
        if (Input.GetKeyDown(throwKey))
        {
            if (grabbedObject != null)
            {
                grabbedObject.Throw(throwForce);
                grabbedObject = null;
            }
        }
    }

    void CreateGrabLine()
    {
        GameObject lineObj = new GameObject("GrabLine");
        lineObj.transform.SetParent(transform);

        grabLine = lineObj.AddComponent<LineRenderer>();
        grabLine.startWidth = 0.02f;
        grabLine.endWidth = 0.02f;
        grabLine.positionCount = 2;

        Material lineMat = new Material(Shader.Find("Unlit/Color"));
        lineMat.color = new Color(1f, 0.5f, 0f, 0.5f); // ��Ȳ��
        grabLine.material = lineMat;

        grabLine.enabled = false;
    }

    void UpdateGrabLine()
    {
        if (currentTarget != null)
        {
            // Ÿ���� ������ �� ǥ��
            grabLine.enabled = true;
            grabLine.SetPosition(0, transform.position);
            grabLine.SetPosition(1, currentTarget.transform.position);
        }
        else if (grabbedObject != null)
        {
            // ��� ������ �� ǥ��
            grabLine.enabled = true;
            grabLine.SetPosition(0, transform.position);
            grabLine.SetPosition(1, grabbedObject.transform.position);
        }
        else
        {
            // �ƹ��͵� ������ �� ����
            grabLine.enabled = false;
        }
    }
}
