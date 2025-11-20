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

    public PlayerState state;
    public PlayerActionGate gate;

    // Inventory
    [HideInInspector] public InventoryMono inventory;
    [HideInInspector] public EquipInventoryMono equipInventory;


    Interactor interactor;

    [SerializeField] PlayerWeapon playerWeapon;

    PlayerMove playerMove;

    Animator animator;


    public CinemachineVirtualCamera vcamCutscene;

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
        animator = GetComponent<Animator>();
        curHP = maxHP;
    }

    static readonly int MoveXHash = Animator.StringToHash("MoveX");
    static readonly int MoveYHash = Animator.StringToHash("MoveY");
    void Update()
    {
        switch (state)
        {
            case PlayerState.Cutscene:
                break;
            case PlayerState.Normal:
                playerMove.TickGround();
                animator.SetFloat(MoveXHash, playerMove.LastMoveInput.x, 0.05f, Time.deltaTime);
                animator.SetFloat(MoveYHash, playerMove.LastMoveInput.y, 0.05f, Time.deltaTime);
                break;
            case PlayerState.Climb:
                playerMove.TickLadder();
                animator.SetFloat(MoveXHash, playerMove.LastMoveInput.x, 0.05f, Time.deltaTime);
                animator.SetFloat(MoveYHash, playerMove.LastMoveInput.y, 0.05f, Time.deltaTime);
                break;
        }
    }

    void HandleJump()
    {
        if (playerMove.TryJump())
        {
            animator.SetTrigger("Jump");
        }
    }


    void HandleFire()
    {
        if (playerWeapon.TryFire(playerMove.isMoving))
        {
            animator.SetTrigger("Fire");
        }
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
        animator.SetTrigger(triggerHash);
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

    #region Ladder Methods
    public Ladder curLadder;
    //Ladder에서 다시 호출
    public void StartLadderClimb(Ladder newLadder)
    {
        if (curLadder != null) { Debug.Log("이미 타고 있는 Ladder가 있음 혹은 끝날때 널처리 안함"); return; }
        if (newLadder == null) { Debug.Log("타려는 Ladder가 null임"); return; }
        curLadder = newLadder;

        playerMove.SnapTo(curLadder.startPos.position, curLadder.startPos.rotation);
        CamController.Instance.SetCutsceneCam(curLadder.startCamPos);

        gate.PushAll();
        animator.SetTrigger("ClimbStart");
        state = PlayerState.Climb;
    }
    // 사다기 타기 시작모션 끝날경우
    public void OnClimbStartExit()
    {
        gate.PopAll();
        gate.PushClimb();
        state = PlayerState.Climb;
        CamController.Instance.SetCam("Main");
    }
    //사다리 맨위 도착 시작
    public void OnClimbEndTopEnter()
    {
        gate.PushAll();
        CamController.Instance.SetCutsceneCam(curLadder.topEndCamPos);
        animator.SetTrigger("ClimbOnTop");
    }
    //사다리 맨아래 도착 시작
    public void OnClimbEndBottomEnter()
    {
        gate.PushAll();
        CamController.Instance.SetCutsceneCam(curLadder.startCamPos);
        animator.SetTrigger("ClimbOnBottom");
    }
    //사다리 맨위 도착 완료
    public void OnClimbEndTopExit()
    {
        playerMove.SnapTo(curLadder.topEndPos.position, curLadder.topEndPos.rotation);
        gate.PopAll();
        state = PlayerState.Normal;
        CamController.Instance.SetCam("Main");
        curLadder.Clear();
        curLadder = null;
    }
    //사다리 맨아래 도착 완료
    public void OnClimbEndBottomExit()
    {
        playerMove.SnapTo(curLadder.bottomEndPos.position, curLadder.bottomEndPos.rotation);
        gate.PopAll();
        state = PlayerState.Normal;
        CamController.Instance.SetCam("Main");
        curLadder.Clear();
        curLadder = null;
    }
    //점프
    public void OnLadderFallEnter()
    {
        gate.PushAll();
    }
    public void OnLadderLandExit()
    {
        gate.PopAll();
        state = PlayerState.Normal;
        curLadder = null;
    }
    #endregion
}
