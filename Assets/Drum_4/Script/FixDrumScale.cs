using UnityEngine;

/// <summary>
/// Drums의 모든 음수 Scale을 자동으로 양수로 수정하는 스크립트
/// Drums 오브젝트에 추가하고 우클릭 → "Fix All Negative Scales" 실행
/// </summary>
public class FixDrumScale : MonoBehaviour
{
    [ContextMenu("Fix All Negative Scales")]
    void FixAllNegativeScales()
    {
        Debug.Log("========================================");
        Debug.Log("?? 음수 Scale 자동 수정 시작!");
        Debug.Log("========================================");

        int fixedCount = 0;

        // 현재 오브젝트의 모든 자식 확인
        foreach (Transform child in transform)
        {
            Vector3 originalScale = child.localScale;
            Vector3 newScale = originalScale;
            bool needsFix = false;

            // 음수 값을 양수로 변경
            if (newScale.x < 0)
            {
                newScale.x = Mathf.Abs(newScale.x);
                needsFix = true;
            }
            if (newScale.y < 0)
            {
                newScale.y = Mathf.Abs(newScale.y);
                needsFix = true;
            }
            if (newScale.z < 0)
            {
                newScale.z = Mathf.Abs(newScale.z);
                needsFix = true;
            }

            if (needsFix)
            {
                child.localScale = newScale;
                Debug.Log($"? {child.name}: {originalScale} → {newScale}");
                fixedCount++;
            }
            else
            {
                Debug.Log($"?? {child.name}: Scale 정상 {originalScale}");
            }
        }

        Debug.Log("========================================");
        if (fixedCount > 0)
        {
            Debug.Log($"?? {fixedCount}개 오브젝트의 Scale 수정 완료!");
        }
        else
        {
            Debug.Log("? 모든 Scale이 이미 정상입니다!");
        }
        Debug.Log("========================================");
    }

    [ContextMenu("Show All Scales")]
    void ShowAllScales()
    {
        Debug.Log("========================================");
        Debug.Log("?? 현재 모든 자식의 Scale:");
        Debug.Log("========================================");

        foreach (Transform child in transform)
        {
            Vector3 scale = child.localScale;
            string status = (scale.x >= 0 && scale.y >= 0 && scale.z >= 0) ? "?" : "?";
            Debug.Log($"{status} {child.name}: ({scale.x}, {scale.y}, {scale.z})");
        }

        Debug.Log("========================================");
    }
}
