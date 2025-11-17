using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public enum PlayerMoveState
    {
        None,
        Climb,
        Parkour,
        Fly
    }

    CharacterController cc;
    [SerializeField] float moveSpeed = 5;
    float verticalVelocity = 0f;
    public float gravity = 9.81f;
    public int isMoving = 0;
    
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
        // 1) 이동 입력
        Vector2 mv = InputManager.Instance.Move;

        // 3) 데드존 및 정규화
        float mag = mv.magnitude;
        if (mag < deadZone)
        {
            mv = Vector2.zero;
            mag = 0f;
        }
        else if (mag > 1f)
        {
            mv /= mag; // 과도 입력 정규화
            mag = 1f;
        }

        isMoving = mag > 0f ? 1 : 0;

        // 5) 실제 이동 (월드 기준)
        Vector3 moveDir = Vector3.zero;

        if (mag > 0f)
        {
            moveDir =
                transform.forward * mv.y +
                transform.right * mv.x;

            moveDir = moveDir.normalized;
        }

        // 6) 중력 처리
        if (cc.isGrounded)
            verticalVelocity = -1f;
        else
            verticalVelocity -= gravity * Time.deltaTime;

        Vector3 move = moveDir * moveSpeed;
        move.y = verticalVelocity;

        cc.Move(move * Time.deltaTime);
    }

}
