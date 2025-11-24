using UnityEngine;
public class PlayerMove : MonoBehaviour
{
    PlayerController pc;
    PlayerActionGate gate;
    CharacterController cc;

    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpPower = 20f;
    [SerializeField] float gravity = 9.81f;
    [SerializeField] float ladderJumpHorizontalPower = 5f;

    float verticalVelocity = 0f;
    public bool isMoving = false;

    bool jumpRequested;
    bool ladderJumpRequested;

    float deadZone = 0.15f;

    Vector3 horizontalDir;
    Vector2 lastMoveInput;

    public Vector2 LastMoveInput => lastMoveInput;
    public float VerticalSpeed => cc.velocity.y;
    public bool IsGrounded => cc.isGrounded;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        pc = GetComponent<PlayerController>();
        gate = pc.gate;
    }

    // Normal / Air 공통 이동
    public void TickGround()
    {
        Move();
        ApplyJump();     // 점프 요청 처리 + 바닥 붙이기
        ApplyGravity();  // 공중 중력
        MoveFinal();     // cc.Move()
    }
    public void TickAir()
    {
        Move();         // 에어 컨트롤
        ApplyGravity(); // 중력만
        MoveFinal();    // 최종 이동
    }

    public void TickLadder()
    {
        Vector2 mv = InputManager.Instance.Move;

        if (gate != null && !gate.Can(BlockAct.Move))
        {
            mv = Vector2.zero;
        }

        float upDown = mv.y;
        lastMoveInput = new Vector2(0, upDown != 0 ? 1f : 0f);

        Vector3 move = pc.curLadder.transform.up * upDown;
        cc.Move(move * Time.deltaTime);
    }

    void Move()
    {
        Vector2 mv = InputManager.Instance.Move;
        lastMoveInput = mv;

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

        isMoving = mag > 0f;

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

    // 점프 요청: 여기서는 그냥 "다음 프레임에 점프해줘"만 표시
    public bool TryJump()
    {
        if (gate != null && !gate.Can(BlockAct.Jump)) return false;
        jumpRequested = true;
        return true;
    }

    public bool TryLadderJump()
    {
        if (gate != null && !gate.Can(BlockAct.Jump)) return false;
        if (pc.curLadder == null) return false;

        // 사다리에서 튀어나올 방향: 사다리 반대 + 위
        Vector3 dir = -pc.curLadder.transform.forward + Vector3.up;
        dir = dir.normalized;

        // 수평 임펄스 (한번에 튕겨 나가는 느낌)
        horizontalDir = new Vector3(dir.x, 0f, dir.z) * ladderJumpHorizontalPower;

        // 위로 점프
        verticalVelocity = jumpPower;
        return true;
    }

    void ApplyJump()
    {
        if (!cc.isGrounded) return;

        if (jumpRequested)
        {
            jumpRequested = false;
            verticalVelocity = jumpPower;

            // 살짝 띄워서 바닥 판정 벗기기
            cc.Move(Vector3.up * 0.05f);
        }
        else
        {
            // 붙이기
            verticalVelocity = -1f;
        }
    }

    void ApplyGravity()
    {
        // 아주 살짝 떠 있을 때도 중력 적용되게 약간 관대하게
        if (cc.isGrounded && verticalVelocity <= 0f)
        {
            verticalVelocity = -1f; // 붙이기
        }
        else
        {
            verticalVelocity -= gravity * Time.deltaTime;
        }
    }

    void MoveFinal()
    {
        Vector3 move = horizontalDir;
        move.y = verticalVelocity;

        cc.Move(move * Time.deltaTime);
    }
}
