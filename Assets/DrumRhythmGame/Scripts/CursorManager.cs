using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    [Header("커서 설정")]
    [SerializeField] private bool showCursorOnStart = false;
    [SerializeField] private bool allowToggle = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    [Header("디버그")]
    [SerializeField] private bool showDebugLogs = true;

    void Start()
    {
        // 게임 시작 시 커서 상태 설정
        SetCursorState(showCursorOnStart);
    }

    void Update()
    {
        // ESC 키로 커서 토글
        if (allowToggle && Input.GetKeyDown(toggleKey))
        {
            ToggleCursor();
        }
    }

    // 커서 보이기/숨기기
    public void SetCursorState(bool visible)
    {
        Cursor.visible = visible;
        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;

        if (showDebugLogs)
        {
            Debug.Log($"🖱️ 커서 상태: {(visible ? "보임" : "숨김")}");
        }
    }

    // 커서 토글
    public void ToggleCursor()
    {
        SetCursorState(!Cursor.visible);
    }

    // 커서 보이기 (외부에서 호출용)
    public void ShowCursor()
    {
        SetCursorState(true);
    }

    // 커서 숨기기 (외부에서 호출용)
    public void HideCursor()
    {
        SetCursorState(false);
    }

    // 씬 전환 시 커서 보이기
    void OnDestroy()
    {
        // 씬 전환될 때 커서 보이게 (다음 씬에서 문제 방지)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
