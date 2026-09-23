using UnityEngine;

public class UnlockCursor : MonoBehaviour
{
    private bool isLocked = true;

    void Start()
    {
        SetCursorState(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            isLocked = !isLocked;
            SetCursorState(isLocked);
        }
    }

    void SetCursorState(bool lockValue)
    {
        if (lockValue)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}