using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    Animator animator;
    float animDamp = 0.05f;       // 애니메이션 파라미터 감쇠

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Jump()
    {
        animator.SetTrigger("Jump");
    }

    public void EquipToggle(bool b)
    {
        animator.SetBool("IsEquip", b);
    }

    static readonly int MoveXHash = Animator.StringToHash("MoveX");
    static readonly int MoveYHash = Animator.StringToHash("MoveY");
    public void Move(Vector3 mv)
    {
        animator.SetFloat(MoveXHash, mv.x, animDamp, Time.deltaTime);
        animator.SetFloat(MoveYHash, mv.y, animDamp, Time.deltaTime);
    }

    public void Fire()
    {
        animator.SetTrigger("Fire");
    }
    

    public void PlayAnimToTrigger(int triggerHash)
    {
        animator.SetTrigger(triggerHash);
    }

}
