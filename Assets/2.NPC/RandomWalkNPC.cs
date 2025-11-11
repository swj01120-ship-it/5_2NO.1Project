using UnityEngine;
using System.Collections;

public class RandomWalkNPC : MonoBehaviour
{
    [Header("Move Settings")]
    public float moveSpeed = 2f;
    public float rotateSpeed = 360f;
    public float stopDuration = 1f; // 목적지 도착 후 잠깐 멈춤

    [Header("Random Area")]
    public Vector2 areaSize = new Vector2(10f, 10f); // X,Z 범위
    public Vector3 areaCenter = Vector3.zero;        // 중앙 기준 위치

    [Header("Animator")]
    public Animator animator;
    public string walkStateName = "Walk"; // 애니메이터 걷기 모션 상태 이름

    Vector3 targetPos;
    bool isWalking = false;

    void Start()
    {
        if (animator) animator.Play(walkStateName, 0, 0f); // 항상 걷기 모션
        StartCoroutine(RandomWalkRoutine());
    }

    IEnumerator RandomWalkRoutine()
    {
        while (true)
        {
            // 1) 랜덤 목적지 선택
            targetPos = GetRandomPoint();

            // 2) 목적지까지 이동
            isWalking = true;
            while (Vector3.Distance(transform.position, targetPos) > 0.2f)
            {
                MoveTowards(targetPos);
                yield return null;
            }

            // 3) 도착 → 대기
            isWalking = false;
            yield return new WaitForSeconds(stopDuration);
        }
    }

    Vector3 GetRandomPoint()
    {
        float x = Random.Range(-areaSize.x / 2f, areaSize.x / 2f);
        float z = Random.Range(-areaSize.y / 2f, areaSize.y / 2f);
        return areaCenter + new Vector3(x, 0, z);
    }

    void MoveTowards(Vector3 target)
    {
        // 방향
        Vector3 dir = (target - transform.position).normalized;

        // 회전
        if (dir != Vector3.zero)
        {
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                look,
                rotateSpeed * Time.deltaTime
            );
        }

        // 이동
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    void OnDrawGizmosSelected()
    {
        // 에디터에서 영역 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(areaCenter, new Vector3(areaSize.x, 0.1f, areaSize.y));
    }
}
