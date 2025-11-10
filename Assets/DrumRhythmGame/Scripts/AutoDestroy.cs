using System.Collections;
using UnityEngine;

/// <summary>
/// 파티클 이펙트가 재생 완료되면 자동으로 오브젝트를 삭제하는 스크립트
/// Tutorial_effect 프리팹에 추가하세요
/// </summary>
public class AutoDestroy : MonoBehaviour
{
    [Header("자동 삭제 설정")]
    [Tooltip("파티클 시스템이 있으면 파티클 duration 기준으로 삭제, 없으면 이 시간 후 삭제")]
    public float destroyDelay = 1f;

    [Tooltip("파티클 duration에 추가할 여유 시간")]
    public float extraTime = 0.5f;

    void Start()
    {
        // 파티클 시스템 찾기
        ParticleSystem ps = GetComponent<ParticleSystem>();
        
        if (ps != null)
        {
            // 파티클이 있으면 duration + lifetime + 여유시간 후 삭제
            var main = ps.main;
            float totalTime = main.duration + main.startLifetime.constantMax + extraTime;
            
            Debug.Log($"🗑️ AutoDestroy: {gameObject.name}을(를) {totalTime}초 후 삭제합니다.");
            Destroy(gameObject, totalTime);
        }
        else
        {
            // 파티클이 없으면 설정된 시간 후 삭제
            Debug.Log($"🗑️ AutoDestroy: {gameObject.name}을(를) {destroyDelay}초 후 삭제합니다.");
            Destroy(gameObject, destroyDelay);
        }
    }
}
