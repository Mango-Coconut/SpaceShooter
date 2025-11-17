using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    public PlayerActionGate gate;
    [HideInInspector] public InventoryMono inventory;
    [HideInInspector] public EquipInventoryMono equipInventory;

    Interactor interactor;
    
    [SerializeField] PlayerWeapon playerWeapon;

    PlayerAnimController AnimController;
    PlayerMove playerMove;

    readonly int maxHP = 10;
    int curHP;


    public delegate void PlayerDieHandler();
    public static event PlayerDieHandler OnPlayerDie;

    


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
        AnimController = GetComponent<PlayerAnimController>();
        curHP = maxHP;
    }

    void Update()
    {
        HandleMoveAnimation();
    }
    
    void HandleFire()
    {
        if (!playerWeapon.CanFire()) return;
        if (!gate.Can(BlockAct.Fire)) return;
        if (Cursor.lockState != CursorLockMode.Locked) return;
        playerWeapon.Fire(playerMove.isMoving);
        AnimController.PlayFire();
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
        AnimController.EquipToggle(isWeaponEquip);

        //playerArmor.HelmetEquip(equipInventory.GetEquipped(EquipType.Helmet, out item));
        //playerArmor.ChestArmorEquip(equipInventory.GetEquipped(EquipType.ChestArmor, out item));
    }

    public void UseLadder()
    {
        
    }

    // 외부에서 플레이어 Animator 조작
    public void PlayAnimToTrigger(int triggerHash)
    {
        AnimController.PlayAnimToTrigger(triggerHash);
    }


    void HandleMoveAnimation()
    {
        // 1) 이동 입력
        Vector2 mv = InputManager.Instance.Move;

        // 2) 이동 불가면 애니 파라미터 0으로 감쇠 후 종료
        if (!gate.Can(BlockAct.Move))
        {
            AnimController.MoveAnim(Vector3.zero);
            return;
        }

        // 3) 애니메이터 파라미터(감쇠 적용)
        AnimController.MoveAnim(mv);
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
