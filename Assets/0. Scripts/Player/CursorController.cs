using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public static bool LookEnabled { get; private set; } = true;

    void Awake() => Apply(LookEnabled);

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Apply(!LookEnabled);

        // 에디터의 자동 재락/포커스 변동 대비: 매 프레임 강제
        Apply(LookEnabled);
    }

    public static void Apply(bool enable)
    {
        LookEnabled = enable;
        Cursor.lockState = enable ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !enable;
    }
}
