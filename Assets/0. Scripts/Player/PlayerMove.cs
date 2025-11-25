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

    public float verticalVelocity = 0f;
    public bool isMoving = false;

    bool jumpRequested;

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
        wasGrounded = false;
    }

    public void Tick()
    {
        if(wasGrounded) Move();
        ApplyVertical();
        MoveFinal();
        wasGrounded = cc.isGrounded;
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

    public bool TryJump()
    {
        if (gate != null && !gate.Can(BlockAct.Jump)) return false;
        jumpRequested = true;
        return true;
    }


    bool wasGrounded;
    void ApplyVertical()
    {
        // 이전 프레임 기준으로 점프 처리
        if (wasGrounded)
        {
            if (jumpRequested && gate != null && gate.Can(BlockAct.Jump))
            {
                jumpRequested = false;
                verticalVelocity = jumpPower;
                cc.Move(Vector3.up * 0.05f);
            }
            else if (verticalVelocity < 0f)
            {
                // 이전 프레임에도 땅이었다면 살짝 붙이기
                verticalVelocity = -1f;
            }
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

        CollisionFlags flags = cc.Move(move * Time.deltaTime);

        if ((flags & CollisionFlags.Below) != 0 && verticalVelocity < 0f)
        {
            verticalVelocity = -1f;
        }
    }
}
