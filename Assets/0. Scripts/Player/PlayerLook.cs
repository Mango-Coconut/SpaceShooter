using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    PlayerController pc;
    [SerializeField] Transform yawPivot;    // 좌우 회전 (Player)
    [SerializeField] Transform pitchPivot;  // 상하 회전 (PivotObject)

    [SerializeField] float minPitch = -60f;
    [SerializeField] float maxPitch = 80f;

    float pitch; // 현재 상하 각도 누적

    void Awake()
    {
        pc = GetComponent<PlayerController>();
    }

    void Update()
    {
        Vector2 look = InputManager.Instance.Look;

        bool canPlayerRotate = pc.gate.Can(BlockAct.PlayerRotate);
        bool canSightRotate = pc.gate.Can(BlockAct.SightRotate);

        // 둘 다 불가능하면 입력 무시
        if (!canPlayerRotate && !canSightRotate)
            return;

        if (canPlayerRotate)
        {
            PlayerRotate(look);
        }
        else if (canSightRotate)
        {
            SightRotate(look);
        }
    }

    void PlayerRotate(Vector2 look)
    {
        // 1) 좌우 → 플레이어(YawPivot) 회전
        yawPivot.Rotate(0f, look.x, 0f);

        // 2) 상하 → pitch 값 누적 후 적용
        pitch -= look.y;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Vector3 angles = pitchPivot.localEulerAngles;
        angles.x = pitch;
        pitchPivot.localEulerAngles = angles;
    }

    void SightRotate(Vector2 look)
    {
        // 플레이어 몸은 고정, 카메라만 회전
        pitch -= look.y;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Vector3 angles = pitchPivot.localEulerAngles;
        angles.x = pitch;
        pitchPivot.localEulerAngles = angles;
    }
}
