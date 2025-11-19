using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    PlayerController pc;
    [SerializeField] Transform yawPivot;    // 좌우 회전 (Player)
    [SerializeField] Transform pitchPivot;  // 상하 회전 (PivotObject)

    [SerializeField] float minPitch = -60f;
    [SerializeField] float maxPitch = 80f;

    float pitch; // 현재 상하 각도 누적

    void Awake() {
        pc = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!pc.gate.Can(BlockAct.Rotate)) return;
        Vector2 look = InputManager.Instance.Look;
        

        // 1) 좌우 → 플레이어(YawPivot) 회전
        yawPivot.Rotate(0f, look.x, 0f);

        // 2) 상하 → pitch 값만 누적해서 PitchPivot에 적용
        pitch -= look.y;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        

        Vector3 angles = pitchPivot.localEulerAngles;
        angles.x = pitch;
        pitchPivot.localEulerAngles = angles;
    }
}