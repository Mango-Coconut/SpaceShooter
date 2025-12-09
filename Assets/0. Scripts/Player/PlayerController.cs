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

    [SerializeField]
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

    [SerializeField] float hardLandingSpeed = 7f; // 값은 나중에 감으로 조절
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
        curHP = maxHP-5;
    }

    static readonly int MoveXHash = Animator.StringToHash("MoveX");
    static readonly int MoveYHash = Animator.StringToHash("MoveY");
    void Update()
    {
        //개발자 도구
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TakeDamage(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Heal(1);
        }

        switch (state)
        {
            case PlayerState.Cutscene:
                // 컷신 중에는 움직임 정지
                playerMove.Tick(PlayerState.Cutscene);
                break;

            case PlayerState.Normal:
                playerMove.Tick(PlayerState.Normal);
                break;

            case PlayerState.Climb:
                playerMove.Tick(PlayerState.Climb);
                break;

            case PlayerState.Air:
                playerMove.Tick(PlayerState.Air);
                break;
        }

        switch (state)
        {
            case PlayerState.Cutscene:
                // 별도 처리 필요시 여기
                break;

            case PlayerState.Normal:
                UpdateMoveAnim();

                // 땅에서 떨어지면 Air로 전환
                if (!playerMove.IsGrounded)
                {
                    state = PlayerState.Air;
                }
                break;

            case PlayerState.Climb:
                UpdateMoveAnim();
                break;

            case PlayerState.Air:
                // 착지 체크
                if (playerMove.IsGrounded)
                {
                    float fallSpeed = playerMove.VerticalSpeed; // 음수면 아래로 떨어진 값
                    state = PlayerState.Normal;

                    // 충분히 빠르게 떨어졌으면 하드 랜딩
                    if (fallSpeed <= -hardLandingSpeed)
                    {
                        gate.PushAll();                          // 애니 동안 입력 막기
                        animator.SetTrigger("FallingToGround");  // Landing 애니 트리거
                    }
                    // 느리게 떨어진 경우는 그냥 Normal 복귀
                }
                break;
        }


    }
    void UpdateMoveAnim()
    {
        animator.SetFloat(MoveXHash, playerMove.LastMoveInput.x, 0.05f, Time.deltaTime);
        animator.SetFloat(MoveYHash, playerMove.LastMoveInput.y, 0.05f, Time.deltaTime);
    }
    public void LandingEnd()
    {
        gate.PopAll();
        state = PlayerState.Normal;
        CamController.Instance.SetCam("Main");
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

    public void InteractExit()
    {
        interactor.InteractExit();
    }

    //equipInventory.OnChanged += HandleEquipChanged
    void HandleEquipChanged()
    {
        StoredItem item;
        bool isWeaponEquip = equipInventory.TryGetEquipped(EquipType.Weapon, out item);
        if(isWeaponEquip) playerWeapon.Equip(item);
        animator.SetBool("IsEquip", isWeaponEquip);

        //playerArmor.HelmetEquip(equipInventory.GetEquipped(EquipType.Helmet, out item));
        //playerArmor.ChestArmorEquip(equipInventory.GetEquipped(EquipType.ChestArmor, out item));
    }


    // 외부에서 플레이어 Animator 조작
    public void PlayAnimToTrigger(int triggerHash)
    {
        animator.SetTrigger(triggerHash);
    }

    public void TakeDamage(int amount)
    {
        curHP -= amount;
        if (curHP <= 0)
        {
            Die();
            return;
        }
        curHP = Mathf.Clamp(curHP, 0, maxHP);
        Debug.Log($"takedamage {amount}, curhp : {curHP}");
    }
    public bool Heal(int amount)
    {
        if(curHP == maxHP) return false;
        curHP += amount;
        curHP = Mathf.Clamp(curHP, 0, maxHP);
        Debug.Log($"heal {amount}, curhp : {curHP}");
        return true;
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
            TakeDamage(1);
        }
    }

    #region Ladder Methods
    public Ladder curLadder;
    //Ladder에서 다시 호출
    public void StartLadderClimb(Ladder newLadder, Transform startPos, Transform startCamPos)
    {
        curLadder = newLadder;

        playerMove.SnapTo(startPos.position, startPos.rotation);
        CamController.Instance.SetCutsceneCam(startCamPos);

        gate.PushAll();
        animator.SetTrigger("ClimbStart");
        state = PlayerState.Cutscene;
    }
    // 사다기 타기 시작모션 끝날경우
    public void OnClimbStartExit()
    {
        gate.PopAll();
        gate.PushClimb();
        state = PlayerState.Climb;
        CamController.Instance.SetCam("Main");
    }
    //사다리 맨위 도착 애니메이션 시작
    public void OnClimbEndTopEnter()
    {
        gate.PopClimb();
        gate.PushAll();
        CamController.Instance.SetCutsceneCam(curLadder.topCamPos);
        state = PlayerState.Cutscene;
        animator.SetTrigger("ClimbOnTop");
    }
    //사다리 맨아래 도착 애니메이션 시작
    public void OnClimbEndBottomEnter()
    {
        gate.PopClimb();
        gate.PushAll();
        CamController.Instance.SetCutsceneCam(curLadder.bottomCamPos);
        state = PlayerState.Cutscene;
        animator.SetTrigger("ClimbOnBottom");
    }
    //사다리 맨위 도착 애니메이션 완료
    public void OnClimbEndTopExit()
    {
        playerMove.SnapTo(curLadder.topEndPos.position, curLadder.topEndPos.rotation);
        gate.PopAll();
        state = PlayerState.Normal;
        CamController.Instance.SetCam("Main");
        curLadder.Clear();
        curLadder = null;
    }
    //사다리 맨아래 도착 애니메이션 완료
    public void OnClimbEndBottomExit()
    {
        playerMove.SnapTo(curLadder.bottomEndPos.position, curLadder.bottomEndPos.rotation);
        gate.PopAll();
        state = PlayerState.Normal;
        CamController.Instance.SetCam("Main");
        curLadder.Clear();
        curLadder = null;
    }
    #endregion
}
