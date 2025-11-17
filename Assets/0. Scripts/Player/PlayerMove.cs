using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    CharacterController cc;
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float jumpPower = 20;
    float verticalVelocity = 0f;
    public float gravity = 9.81f;
    public int isMoving = 0;
    bool jump;

    float deadZone = 0.15f;       // 너무 미세한 입력 무시


    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        if (state == PlayerMoveState.Climb)
        {
            ClimbLadder();
            return;
        }

        ApplyJump();    // 점프 입력 처리
        ApplyGravity(); // 중력 처리
        MoveFinal();    // cc.Move()
    }

    public void Jump()
    {
        if (state == PlayerMoveState.Jump)
        {
            return;
        }

        // 땅에 붙어있을 때만 점프 입력 받기
        if (!cc.isGrounded)
        {
            return;
        }

        jump = true;
    }

    Vector3 horizontalDir;
    public void Move(Vector3 mv)
    {

        // DeadZone 처리 & 정규화
        float mag = mv.magnitude;
        if (mag < deadZone)
        {
            mv = Vector3.zero;
            mag = 0f;
        }
        else if (mag > 1f)
        {
            mv /= mag;
            mag = 1f;
        }

        isMoving = mag > 0f ? 1 : 0;

        horizontalDir = Vector3.zero;

        if (mag > 0f)
        {
            horizontalDir =
                transform.forward * mv.y +
                transform.right * mv.x;

            horizontalDir = horizontalDir.normalized * moveSpeed;
        }
    }

    void ApplyJump()
    {
        if (!cc.isGrounded) return;

        // 바닥 붙여주기
        verticalVelocity = -1f;

        if (jump)
        {
            jump = false;
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

    void ClimbLadder()
    {

    }

}
