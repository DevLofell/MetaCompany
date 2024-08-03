using UnityEngine;

public class MouseLock : MonoBehaviour
{
    public bool lockCursor = true;
    public bool hideCursor = true;

    void Start()
    {
        SetCursorState(lockCursor, hideCursor);
    }

    void Update()
    {
        // ESC 키를 누르면 커서 잠금을 해제하고 보이게 함
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetCursorState(false, false);
        }

        // 마우스 왼쪽 버튼을 클릭하면 커서를 다시 잠그고 숨김
        if (Input.GetMouseButtonDown(0))
        {
            SetCursorState(true, true);
        }
    }

    void SetCursorState(bool locked, bool hidden)
    {
        lockCursor = locked;
        hideCursor = hidden;

        // 커서 잠금 설정
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;

        // 커서 표시 설정
        Cursor.visible = !hidden;
    }
}