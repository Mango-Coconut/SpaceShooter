using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerActionGate gate;
    [SerializeField] InventoryUI inventoryUI;
    Inventory inventory;
    PickUp pickUp;
    [SerializeField] float moveSpeed = 5;

    FireBullet fireBullet;
    float firetimer = 0;
    [SerializeField] float delay = 0.1f;

    public float fireHeat = 0;
    public int isMoving = 0;

    readonly int maxHP = 10;
    int curHP;


    public delegate void PlayerDieHandler();
    public static event PlayerDieHandler OnPlayerDie;

    Animator animator;
    void OnEnable()                           // ← 추가
    {
        InputManager.Instance.OnFire += OnFire;
        InputManager.Instance.OnPick += OnPick;
    }

    void OnDisable()                          // ← 추가
    {
        InputManager.Instance.OnFire -= OnFire;
        InputManager.Instance.OnPick -= OnPick;
    }

    void Awake()
    {
        gate = new PlayerActionGate();
        inventory = GetComponent<Inventory>();
        pickUp = GetComponent<PickUp>();
        animator = GetComponent<Animator>();
        fireBullet = GetComponent<FireBullet>();
        curHP = maxHP;
    }


    void Update()
    {
        firetimer += Time.deltaTime;
        //연발하지 않는 동안은 총알 spread 줄이기
        if (firetimer > delay) fireHeat = Math.Clamp(fireHeat - Time.deltaTime, 0, 1);
        HandleMovement();
    }

    void OnFire()
    {
        if (!gate.Can(Block.Fire)) return;
        if (Cursor.lockState != CursorLockMode.Locked) return;
        if (inventoryUI != null && inventoryUI.IsOpen) return;
        if (firetimer < delay) return;

        fireHeat = Math.Clamp(fireHeat+0.1f, 0, 1);
        firetimer = 0f;
        fireBullet.Fire(isMoving, fireHeat);
        animator.SetTrigger("Fire");
    }

    void OnPick()
    {
        if (!gate.Can(Block.Pick)) return;
        if (inventoryUI == null) return;
        if (!pickUp.CanPickUp()) return;
        ItemData id = pickUp.PickItems();
        if (id != null)
        {
            inventory.AddItem(id);
            animator.SetTrigger("Pick");
        }
    }
    void HandleMovement()
    {
        
        Vector2 mv = InputManager.Instance.Move;
        Vector2 look = InputManager.Instance.Look;

        animator.SetFloat("MoveX", mv.x);
        animator.SetFloat("MoveY", mv.y);

        transform.Rotate(Vector3.up * look.x * 360 * Time.deltaTime);

        if (!gate.Can(Block.Move)) return;

        Vector3 moveDir = new Vector3(mv.x, 0, mv.y).normalized;
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);

        if (mv.x + mv.y != 0) isMoving = 1;
        else isMoving = 0;
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
            if (curHP <= 0)
            {
                Die();
            }
        }
    }
}
