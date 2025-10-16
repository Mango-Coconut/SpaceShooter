using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerActionGate gate;
    [HideInInspector] public Inventory inventory;
    [HideInInspector] public EquipInventory equipInventory;

    Interactor interactor;
    [SerializeField] float moveSpeed = 5;

    [SerializeField] PlayerWeapon playerWeapon;

    
    public int isMoving = 0;

    readonly int maxHP = 10;
    int curHP;


    public delegate void PlayerDieHandler();
    public static event PlayerDieHandler OnPlayerDie;

    Animator animator;
    void OnEnable()
    {
        InputManager.Instance.OnFire += OnFire;
        InputManager.Instance.OnInteract += OnInteract;
        equipInventory.OnEquipped += EquipItem;
        equipInventory.OnUnequipped += UnEquipItem;
    }

    void OnDisable()
    {
        InputManager.Instance.OnFire -= OnFire;
        InputManager.Instance.OnInteract -= OnInteract;
        equipInventory.OnEquipped += EquipItem;
        equipInventory.OnUnequipped += UnEquipItem;
    }

    void Awake()
    {
        gate = PlayerActionGate.Instance;
        inventory = GetComponent<Inventory>();
        equipInventory = GetComponent<EquipInventory>();
        interactor = GetComponent<Interactor>();
        animator = GetComponent<Animator>();
        curHP = maxHP;
    }


    void Update()
    {
        HandleMovement();
    }

    void OnFire()
    {
        if (!playerWeapon.CanFire()) return;
        if (!gate.Can(Block.Fire)) return;
        if (Cursor.lockState != CursorLockMode.Locked) return;
        playerWeapon.Fire(isMoving);
        animator.SetTrigger("Fire");
    }

    void OnInteract()
    {
        if (!gate.Can(Block.Interact)) return;
        //상호작용 중이면 다른 상호작용 x
        if (interactor.Interact(this))
        {
            gate.PushInteract();
        }
    }

    //equipInventory.OnEquipped += EquipItem
    void EquipItem(StoredItem item)
    {
        playerWeapon.Equip(item);
    }
    void UnEquipItem(StoredItem item)
    {
        playerWeapon.UnEquip();
    }

    public void PlayAnimToTrigger(int triggerHash)
    {
        if (animator) animator.SetTrigger(triggerHash);
    }
    
    void HandleMovement()
    {
        //회전
        Vector2 look = InputManager.Instance.Look;
        transform.Rotate(Vector3.up * look.x * 360 * Time.deltaTime);

        //이동
        Vector2 mv = InputManager.Instance.Move;

        animator.SetFloat("MoveX", mv.x);
        animator.SetFloat("MoveY", mv.y);

        if (mv.x + mv.y != 0) isMoving = 1;
        else isMoving = 0;


        if (!gate.Can(Block.Move)) return;

        Vector3 moveDir = new Vector3(mv.x, 0, mv.y).normalized;
        transform.Translate(moveDir * moveSpeed * Time.deltaTime);
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
