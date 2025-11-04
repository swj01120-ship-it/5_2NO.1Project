using System.Collections;
using UnityEngine;

public class DoJump : MonoBehaviour
{
    [Header("Watch this mover (platform/vehicle)")]
    public Transform mover;                 // 이동 오브젝트
    public Vector3 axis = -Vector3.forward;  // 감시 축 (Z 기준이면 (0,0,1))
    public float thresholdValue = 10f;      // 예: Z >= 10에 도달시
    public float delayTime = 1.5f;          // 좌표 도달 후 N초 뒤 트리거
    public bool fireOnce = true;            // 한 번만 수행
    private bool fired = false;

    [Header("Rider (character)")]
    public Transform character;             // 캐릭터 루트
    public Animator animator;               // 캐릭터 Animator
    public string jumpTrigger = "DoJump";   // 점프 트리거
    public string idleStateName = "Idle";   // 복귀할 Idle 스테이트명 (Standing 쓰면 여기 맞춰 바꾸세요)
    public bool unparentOnJump = true;      // 점프 시작 시 탑승 해제(부모 끊기)

    [Header("Optional: Physics jump")]
    public bool usePhysicsJump = false;     // true면 물리 점프 사용
    public Rigidbody characterRb;           // 캐릭터 리지드바디 (선택)
    public float jumpUpVelocity = 5f;       // 위로 속도
    public float jumpForwardVelocity = 2f;  // 앞으로 속도 (axis 방향)
    public LayerMask groundMask;            // 바닥 레이어
    public float groundCheckRadius = 0.2f;  // 착지 판정 반경
    public Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);

    private Transform originalParent;


    void Reset()
    {
        axis = Vector3.forward;
        thresholdValue = 10f;
        jumpTrigger = "DoJump";
        idleStateName = "Idle";
        unparentOnJump = true;
        usePhysicsJump = false;
        groundCheckRadius = 0.2f;
        groundCheckOffset = new Vector3(0, 0.1f, 0);
    }

    void Awake()
    {
        if (!character) character = transform;
        if (!animator) animator = character.GetComponentInChildren<Animator>();
        if (!mover) Debug.LogWarning("[DoJump] Mover not assigned.");
        originalParent = character.parent;
    }

    void Update()
    {
        if (fireOnce && fired) return;
        if (!mover || !character || !animator) return;

        float projected = Vector3.Dot(mover.position, axis.normalized);
        if (projected >= thresholdValue)
        {
            fired = true;
            StartCoroutine(DelayTriggerThenJump());
        }
    }

    private IEnumerator DelayTriggerThenJump()
    {
        // 좌표 도달 후 대기
        yield return new WaitForSeconds(2.0f);

        // 점프 시퀀스 실행
        yield return StartCoroutine(JumpSequence());
    }

    private IEnumerator JumpSequence()
    {
        // 1) 탑승 해제(선택)
        if (unparentOnJump && character.parent != null)
        {
            character.SetParent(null, true); // 월드 좌표 유지
        }

        // 2) 점프 시작
        if (usePhysicsJump && characterRb != null)
        {
            characterRb.isKinematic = false;
            // Vector3 fwd = axis.normalized;
            Vector3 fwd = Vector3.ProjectOnPlane(character.forward, Vector3.up).normalized;
            //Vector3 vel = fwd * jumpForwardVelocity + Vector3.up * jumpUpVelocity;
            //characterRb.velocity = vel;
            Vector3 vel = fwd * jumpForwardVelocity + Vector3.up * jumpUpVelocity;
            characterRb.velocity = vel;
        }

        // 애니메이터 트리거 발사 (하드코딩 "p" 대신 jumpTrigger 사용)
        if (animator && !string.IsNullOrEmpty(jumpTrigger))
        {
            animator.ResetTrigger(jumpTrigger);
            animator.SetTrigger(jumpTrigger);
        }

        // 3) 착지 대기
        if (usePhysicsJump)
        {
            // 물리 기반 바닥 검출
            yield return new WaitForSeconds(0.05f); // 초기 프레임 안정화
            while (!IsGrounded())
                yield return null;
        }
        else
        {
            // 애니메이션 기반 대기 (안전 타임아웃)
            int baseLayer = 0;
            float safety = 3f;
            float t = 0f;
            while (t < safety)
            {
                var info = animator.GetCurrentAnimatorStateInfo(baseLayer);
                if (info.normalizedTime >= 0.98f && !info.loop) break;
                t += Time.deltaTime;
                yield return null;
            }
        }

        // 4) Idle로 복귀(애니메이터 전이 or 강제 페이드)
        if (animator && !string.IsNullOrEmpty(idleStateName))
        {
            animator.CrossFadeInFixedTime(idleStateName, 0.15f);
        }

        // 필요하면 다시 부모 복귀 (옵션)
        // if (!unparentOnJump && originalParent != null && character.parent == null)
        // {
        //     character.SetParent(originalParent, true);
        // }
    }

    private bool IsGrounded()
    {
        Vector3 origin = character.position + groundCheckOffset;
        return Physics.CheckSphere(origin, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);
    }

    private void OnDrawGizmosSelected()
    {
        // 임계 위치 시각화(간단)
        Gizmos.color = Color.yellow;
        Vector3 n = axis.normalized;
        Vector3 p = n * thresholdValue;
        Gizmos.DrawLine(p - Vector3.up * 2f, p + Vector3.up * 2f);

        if (character)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(character.position + groundCheckOffset, groundCheckRadius);
        }
    }
}
