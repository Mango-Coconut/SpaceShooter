using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Cinemachine;


public class PlayerController : MonoBehaviour
{
    
    PlayerState state;
    public PlayerActionGate gate;

    // Inventory
    [HideInInspector] public InventoryMono inventory;
    [HideInInspector] public EquipInventoryMono equipInventory;

    
    Interactor interactor;
    
    [SerializeField] PlayerWeapon playerWeapon;

    PlayerMove playerMove;

    PlayerAnimController animController;

    
    public CinemachineVirtualCamera vcamCutscene;
    public Transform startCamPos;
    public Transform endCamPos;

    readonly int maxHP = 10;
    int curHP;


    public delegate void PlayerDieHandler();
    public static event PlayerDieHandler OnPlayerDie;

    


    void OnEnable()
    {
        InputManager.Instance.OnJump += HandleJump;
        InputManager.Instance.OnFire += HandleFire;
        InputManager.Instance.OnInteract += HandleInteract;
        equipInventory.OnChanged += HandleEquipChanged;
    }
    void OnDisable()
    {
        InputManager.Instance.OnJump -= HandleJump;
        InputManager.Instance.OnFire -= HandleFire;
        InputManager.Instance.OnInteract -= HandleInteract;
        equipInventory.OnChanged -= HandleEquipChanged;
    }
    void Awake()
    {
        gate = GetComponent<PlayerActionGate>();
        playerMove = GetComponent<PlayerMove>();
        inventory = GetComponent<InventoryMono>();
        equipInventory = GetComponent<EquipInventoryMono>();
        interactor = GetComponent<Interactor>();
        animController = GetComponent<PlayerAnimController>();
        curHP = maxHP;
    }

    void Update()
    {
        switch (state)
        {
            case PlayerState.Cutscene:
                break;
            case PlayerState.Normal:
                playerMove.TickGround();
                animController.Move(playerMove.LastMoveInput);
                break;
            case PlayerState.Climb:
                playerMove.TickLadder();
                animController.Move(playerMove.LastMoveInput);
                break;
        }
    }

    void HandleJump()
    {
        if (playerMove.TryJump())
        {
            animController.Jump();
        }
    }


    void HandleFire()
    {
        if (playerWeapon.TryFire(playerMove.isMoving))
        {
            animController.Fire();
        }
    }

    public Ladder curLadder;
    public void StartLadderClimb(Ladder newLadder)
    {
        if(curLadder != null){Debug.Log("이미 타고 있는 Ladder가 있음 혹은 끝날때 널처리 안함"); return;}
        if(newLadder == null) {Debug.Log("타려는 Ladder가 null임"); return;}
        curLadder = newLadder;

        playerMove.SnapTo(curLadder.startPos.position, curLadder.startPos.rotation);
        CamController.Instance.SetCutsceneCam(curLadder.startCamPos);

        gate.PushAll();
        animController.ClimbStart();
        state = PlayerState.Climb;
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
        animController.EquipToggle(isWeaponEquip);

        //playerArmor.HelmetEquip(equipInventory.GetEquipped(EquipType.Helmet, out item));
        //playerArmor.ChestArmorEquip(equipInventory.GetEquipped(EquipType.ChestArmor, out item));
    }


    // 외부에서 플레이어 Animator 조작
    public void PlayAnimToTrigger(int triggerHash)
    {
        animController.PlayAnimToTrigger(triggerHash);
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
