using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    PickUp pickUp;
    public bool isPicking = false;
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float turnSpeed;

    FireBullet fireBullet;
    float firetimer = 0;
    [SerializeField] float delay = 0.1f;


    readonly int maxHP = 10;
    int curHP;

    public delegate void PlayerDieHandler();
    public static event PlayerDieHandler OnPlayerDie;

    Animator animator;

    void Awake()
    {
        pickUp = GetComponent<PickUp>();
        animator = GetComponent<Animator>();
        fireBullet = GetComponent<FireBullet>();
        curHP = maxHP;
    }


    void Update()
    {
        //사격 시스템
        firetimer += Time.deltaTime;
        if (Cursor.lockState == CursorLockMode.Locked && firetimer > delay && Input.GetMouseButton(0))
        {
            firetimer = 0;
            fireBullet.Fire();
        }


        //줍기 시스템
        if (isPicking) return;
        if (!isPicking && Input.GetKeyDown(KeyCode.F) && pickUp.CanPickUp())
        {
            pickUp.PickItems();
            animator.SetTrigger("Pick");
        }

        //움직임 시스템
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        float r = 0;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            r = Input.GetAxisRaw("Mouse X");
        }
        if (Mathf.Abs(r) < 0.01f) r = 0f;

        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
        gameObject.transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);
        gameObject.transform.Rotate(Vector3.up * r * turnSpeed * Time.deltaTime);


        animator.SetFloat("MoveX", h);
        animator.SetFloat("MoveY", v);
    }
    void Die()
    {
        Debug.Log($"사망");
        OnPlayerDie();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MonsterAttack"))
        {
            curHP--;
            Debug.Log($"hited, {curHP}");
            if (curHP <= 0)
            {
                Die();
            }
        }
    }
}
