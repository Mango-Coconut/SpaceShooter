using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    PlayerController pc;
    PlayerActionGate gate;
    CharacterController cc;

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpPower = 20f;
    [SerializeField] float gravity = 9.81f;

    float verticalVelocity = 0f;
    public bool isMoving = false;
    bool jumpRequested;

    float deadZone = 0.15f;

    Vector3 horizontalDir;
    Vector2 lastMoveInput;

    public Vector2 LastMoveInput => lastMoveInput;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        pc = GetComponent<PlayerController>();
        gate = pc.gate;
    }


    // 매 프레임 호출: 이동 + 중력 + 점프 처리
    public void TickGround()
    {
        Move();
        ApplyJump();    // 점프 시작 처리
        ApplyGravity(); // 공중 중력
        MoveFinal();    // cc.Move()
    }

    public void TickLadder()
    {
        Vector2 mv = InputManager.Instance.Move;
        // Move가 막혀 있으면 입력 무시
        if (gate != null && !gate.Can(BlockAct.Move))
        {
            mv = Vector2.zero;
        }

        float upDown = mv.y;

        //애니메이션에서 블렌드 트리 이용하기 위해(0이면 idle, 1이면 위아래 상관없이 climbing)
        lastMoveInput = new Vector2(0, upDown != 0 ? 1f : 0f);

        // 사다리 축 방향으로만 이동
        Vector3 move = pc.curLadder.transform.up * upDown;

        cc.Move(move * Time.deltaTime);
    }

    void Move()
    {
        Vector2 mv = InputManager.Instance.Move;
        lastMoveInput = mv;

        // Move가 막혀 있으면 입력 무시
        if (gate != null && !gate.Can(BlockAct.Move))
        {
            mv = Vector2.zero;
        }

        float mag = mv.magnitude;
        if (mag < deadZone)
        {
            mv = Vector2.zero;
            mag = 0f;
        }
        else if (mag > 1f)
        {
            mv /= mag;
            mag = 1f;
        }

        isMoving = mag > 0f ? true : false;

        horizontalDir = Vector3.zero;

        if (mag > 0f)
        {
            horizontalDir =
                transform.forward * mv.y +
                transform.right * mv.x;

            horizontalDir = horizontalDir.normalized * moveSpeed;
        }
    }
    public void SnapTo(Vector3 pos, Quaternion rot)
    {
        cc.enabled = false;
        transform.SetPositionAndRotation(pos, rot);
        cc.enabled = true;
    }

    public bool TryJump()
    {
        // 게이트에서 Jump 차단 중이면 실패
        if (gate != null && !gate.Can(BlockAct.Jump)) return false;

        // 공중에서는 점프 요청 무시
        if (!cc.isGrounded) return false;

        jumpRequested = true;
        return true;
    }

    void ApplyJump()
    {
        if (!cc.isGrounded) return;

        // 바닥에 붙여주기
        verticalVelocity = -1f;

        if (jumpRequested)
        {
            jumpRequested = false;
            verticalVelocity = jumpPower;
        }
    }

    void ApplyGravity()
    {
        if (cc.isGrounded) return;

        verticalVelocity -= gravity * Time.deltaTime;
    }

    void MoveFinal()
    {
        Vector3 move = horizontalDir;
        move.y = verticalVelocity;

        cc.Move(move * Time.deltaTime);
    }

}
