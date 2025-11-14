using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;


// [RequireComponent(typeof(PlayerActionGate))]
public class PlayerController : MonoBehaviour
{
    public PlayerActionGate gate;
    [HideInInspector] public InventoryMono inventory;
    [HideInInspector] public EquipInventoryMono equipInventory;

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
        InputManager.Instance.OnFire += HandleFire;
        InputManager.Instance.OnInteract += HandleInteract;
        equipInventory.OnChanged += HandleEquipChanged;
    }

    void OnDisable()
    {
        InputManager.Instance.OnFire -= HandleFire;
        InputManager.Instance.OnInteract -= HandleInteract;
        equipInventory.OnChanged -= HandleEquipChanged;
    }

    void Awake()
    {
        gate = GetComponent<PlayerActionGate>();
        inventory = GetComponent<InventoryMono>();
        equipInventory = GetComponent<EquipInventoryMono>();
        interactor = GetComponent<Interactor>();
        animator = GetComponent<Animator>();
        curHP = maxHP;
    }


    void Update()
    {
        HandleMovement();
    }

    void HandleFire()
    {
        if (!playerWeapon.CanFire()) return;
        if (!gate.Can(BlockAct.Fire)) return;
        if (Cursor.lockState != CursorLockMode.Locked) return;
        playerWeapon.Fire(isMoving);
        animator.SetTrigger("Fire");
    }

    void HandleInteract()
    {
        if (!gate.Can(BlockAct.Interact)) return;
        interactor.Interact(this);
    }

    //equipInventory.OnChanged += HandleEquipChanged
    void HandleEquipChanged()
    {
        StoredItem item;
        bool isWeaponEquip = equipInventory.TryGetEquipped(EquipType.Weapon, out item);
        playerWeapon.Equip(item);
        animator.SetBool("IsEquip", isWeaponEquip);

        //playerArmor.HelmetEquip(equipInventory.GetEquipped(EquipType.Helmet, out item));
        //playerArmor.ChestArmorEquip(equipInventory.GetEquipped(EquipType.ChestArmor, out item));
    }

    // 외부에서 플레이어 Animator 조작
    public void PlayAnimToTrigger(int triggerHash)
    {
        if (animator) animator.SetTrigger(triggerHash);
    }


    float deadZone = 0.15f;       // 너무 미세한 입력 무시
    float animDamp = 0.05f;       // 애니메이션 파라미터 감쇠

    static readonly int MoveXHash = Animator.StringToHash("MoveX");
    static readonly int MoveYHash = Animator.StringToHash("MoveY");
    void HandleMovement()
    {
        // Vector2 look = InputManager.Instance.Look;
        // if (look.x != 0f)
        // {
        //     transform.Rotate(Vector3.up * look.x * rotateSpeed * Time.deltaTime);
        // }

        // 2) 이동 입력
        Vector2 mv = InputManager.Instance.Move;

        // 3) 이동 불가면 애니 파라미터 0으로 감쇠 후 종료
        if (!gate.Can(BlockAct.Move))
        {
            animator.SetFloat(MoveXHash, 0f, animDamp, Time.deltaTime);
            animator.SetFloat(MoveYHash, 0f, animDamp, Time.deltaTime);
            isMoving = 0;
            return;
        }

        // 4) 데드존 및 정규화
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

        // 5) 애니메이터 파라미터(감쇠 적용)
        animator.SetFloat(MoveXHash, mv.x, animDamp, Time.deltaTime);
        animator.SetFloat(MoveYHash, mv.y, animDamp, Time.deltaTime);

        isMoving = mag > 0f ? 1 : 0;

        // 6) 실제 이동 (월드 기준)
        if (mag > 0f)
        {
            Vector3 dir = new Vector3(mv.x, 0f, mv.y);
            transform.Translate(dir * moveSpeed * Time.deltaTime);
        }
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
