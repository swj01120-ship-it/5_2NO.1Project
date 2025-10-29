using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRGrabber : MonoBehaviour
{
    [Header("Grab Settings")]
    public float grabDistance = 2f;             // 잡기 거리
    public LayerMask grabbableLayer;            // 잡을 수 있는 레이어
    public Transform hand;                      // 손 위치

    [Header("Input")]
    public KeyCode grabKey = KeyCode.G;         // 잡기 키
    public KeyCode throwKey = KeyCode.F;        // 던지기 키

    [Header("Throw")]
    public float throwForce = 10f;               // 던지기 힘

    private GrabbableObject currentTarget;      // 현재 겨냥한 물체
    private GrabbableObject grabbedObject;      // 현재 잡은 물체
    private LineRenderer grabLine;              // 잡기 표시선

    void Start()
    {
        // 손이 없으면 자기 자신
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
            // 물체를 잡지 않았을 때
            DetectGrabbableObject();
            HandleGrab();
        }
        else
        {
            // 물체를 잡았을 때
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

        // 이전 타겟 하이라이트 해제
        if (currentTarget != null)
        {
            currentTarget.Highlight(false);
            currentTarget = null;
        }

        // 레이캐스트로 물체 감지
        if (Physics.Raycast(ray, out hit, grabDistance, grabbableLayer))
        {
            Debug.Log("레이캐스트 히트: " + hit.collider.name); // 디버그 추가!

            GrabbableObject grabbable = hit.collider.GetComponent<GrabbableObject>();

            if (grabbable != null && !grabbable.isGrabbed)
            {
                currentTarget = grabbable;
                currentTarget.Highlight(true);
                Debug.Log("잡을 수 있는 물체 감지: " + grabbable.name); // 디버그 추가!
            }
            else
            {
                Debug.Log("GrabbableObject 컴포넌트 없음 또는 이미 잡힘"); // 디버그 추가!
            }
        }
        else
        {
            //Debug.Log("레이캐스트 미스"); // 디버그 추가!
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
        lineMat.color = new Color(1f, 0.5f, 0f, 0.5f); // 주황색
        grabLine.material = lineMat;

        grabLine.enabled = false;
    }

    void UpdateGrabLine()
    {
        if (currentTarget != null)
        {
            // 타겟이 있으면 선 표시
            grabLine.enabled = true;
            grabLine.SetPosition(0, transform.position);
            grabLine.SetPosition(1, currentTarget.transform.position);
        }
        else if (grabbedObject != null)
        {
            // 잡고 있으면 선 표시
            grabLine.enabled = true;
            grabLine.SetPosition(0, transform.position);
            grabLine.SetPosition(1, grabbedObject.transform.position);
        }
        else
        {
            // 아무것도 없으면 선 숨김
            grabLine.enabled = false;
        }
    }
}
