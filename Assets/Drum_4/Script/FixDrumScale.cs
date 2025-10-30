using UnityEngine;

/// <summary>
/// Drums�� ��� ���� Scale�� �ڵ����� ����� �����ϴ� ��ũ��Ʈ
/// Drums ������Ʈ�� �߰��ϰ� ��Ŭ�� �� "Fix All Negative Scales" ����
/// </summary>
public class FixDrumScale : MonoBehaviour
{
    [ContextMenu("Fix All Negative Scales")]
    void FixAllNegativeScales()
    {
        Debug.Log("========================================");
        Debug.Log("?? ���� Scale �ڵ� ���� ����!");
        Debug.Log("========================================");

        int fixedCount = 0;

        // ���� ������Ʈ�� ��� �ڽ� Ȯ��
        foreach (Transform child in transform)
        {
            Vector3 originalScale = child.localScale;
            Vector3 newScale = originalScale;
            bool needsFix = false;

            // ���� ���� ����� ����
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
                Debug.Log($"? {child.name}: {originalScale} �� {newScale}");
                fixedCount++;
            }
            else
            {
                Debug.Log($"?? {child.name}: Scale ���� {originalScale}");
            }
        }

        Debug.Log("========================================");
        if (fixedCount > 0)
        {
            Debug.Log($"?? {fixedCount}�� ������Ʈ�� Scale ���� �Ϸ�!");
        }
        else
        {
            Debug.Log("? ��� Scale�� �̹� �����Դϴ�!");
        }
        Debug.Log("========================================");
    }

    [ContextMenu("Show All Scales")]
    void ShowAllScales()
    {
        Debug.Log("========================================");
        Debug.Log("?? ���� ��� �ڽ��� Scale:");
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
