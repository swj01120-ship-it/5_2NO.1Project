using UnityEngine;

public class BoxClampMovement : MonoBehaviour
{
    public Vector3 minBounds = new Vector3(-5, 0, -5); // 영역 최소값
    public Vector3 maxBounds = new Vector3(5, 0, 5); // 영역 최대값
    public bool lockY = true; // 바닥 높이 고정

    void LateUpdate()
    {
        var p = transform.position;
        p.x = Mathf.Clamp(p.x, minBounds.x, maxBounds.x);
        p.z = Mathf.Clamp(p.z, minBounds.z, maxBounds.z);
        if (!lockY) p.y = Mathf.Clamp(p.y, minBounds.y, maxBounds.y);
        else p.y = minBounds.y; // 바닥 고정 시

        transform.position = p;
    }
}
