using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public static bool LookEnabled { get; private set; } = true;

    void Awake() => Apply(LookEnabled);

    public static void Apply(bool enable)
    {
        LookEnabled = enable;
        Cursor.lockState = enable ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible   = !enable;
    }
}
