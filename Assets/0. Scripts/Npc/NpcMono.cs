using System;
using System.Runtime.InteropServices;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;

public class NpcMono : MonoBehaviour, IInteractable
{
    [Header("Chest Identification")]
    [SerializeField] string instanceId; // 고유 식별자
    public string InstanceId => instanceId;

    public NpcCore Core { get; private set; }
    
    [SerializeField] Sprite icon;
    [SerializeField] DialogueAsset dialogueAsset;

    ShopInventory shopInventory;
    public ShopInventory ShopInventory => shopInventory;
    
    Animator animator;
    PlayerController player;
    
    // Interact시 발송할 이벤트
    [SerializeField] GameEventHub hub;

    void Awake()
    {
        Core = new NpcCore(gameObject.name);
        shopInventory = GetComponent<ShopInventory>();

        // 자식의 Animator 찾아오기
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);

            Animator found;
            if (child.TryGetComponent<Animator>(out found))
            {
                animator = found;
                break;
            }
        }

        if (animator == null)
        {
            Log.Warn($"{name}: Animator not found among direct children.");
        }
    }

    #region 이벤트
    public event Action OpenShop;
    void OnEnable()
    {
        Core.dialogueCore.OnCommand -= HandleCommand;
        Core.dialogueCore.OnEnded -= HandleDialogueEnded;
        Core.dialogueCore.OnCommand += HandleCommand;
        Core.dialogueCore.OnEnded += HandleDialogueEnded;
       }

    void OnDisable()
    {
        Core.dialogueCore.OnCommand -= HandleCommand;
        Core.dialogueCore.OnEnded -= HandleDialogueEnded;
    }



    void HandleDialogueEnded()
    {
        Exit();
    }
    void HandleCommand(DialogueCommand command)
    {
        switch (command.type)
        {
            case DialogueCommandType.None:
                break;

            case DialogueCommandType.OpenShop:
                if (shopInventory == null)
                {
                    Debug.Log("shopInventory null"); return;
                }
                OpenShop?.Invoke();
                break;
            case DialogueCommandType.StartQuest:
                if (command.questData == null)
                {
                    Debug.Log("DialogueCommand.questdata null"); return;
                }
                if (hub == null && hub.quest == null)
                {
                    Debug.Log("hub or hub.quest null"); return;
                }
                hub.quest.RaiseQuestStartRequested(command.questData, this);

                break;
            case DialogueCommandType.CompleteQuest:
                if (command.questData == null)
                {
                    Debug.Log("DialogueCommand.questdata null"); return;
                }
                if (hub == null && hub.quest == null)
                {
                    Debug.Log("hub or hub.quest null"); return;
                }
                hub.quest.RaiseQuestCompleteRequested(command.questData, this);

                break;

        }
    }

    #endregion
    bool isEnter = false;
    public void Interact(PlayerController pc)
    {
        //추후 네트워크 환경 등에서 널가드 추가
        // if (pc == null) Log.Error("NpcMono : PlayerController is null");
        if (isEnter == false)
        {
            Enter(pc);
        }
        else
        {
            Exit();
        }
    }
    public void Enter(PlayerController pc)
    {
        if (isEnter == true) return;
        if (pc == null || pc.gate == null) return;
        if (hub == null || hub.npc == null) return;

        isEnter = true;

        player = pc;
        player.gate.PushUI();

        hub.npc.RaiseEnter(this);
        Core.Initialize(dialogueAsset);
    }
    public void Exit()
    {
        if (isEnter == false) return;
        if (player == null || player.gate == null) return;
        if (hub == null || hub.npc == null) return;

        isEnter = false;

        player.gate.PopUI();
        player = null;

        hub.npc.RaiseExit(this);
    }

    public bool IsAvailable()
    {
        return Core.CanTalk;
    }

    public void OnFocus()
    {
        //animator.SetTrigger("Scanned");
    }

    public void OnUnfocus()
    {
        Exit();
    }
    
    public Sprite GetIcon() => icon;

    public (string inputKeyText, string behaviorText) GetPrompt() => ("F", "대화하기");

    public NpcData SaveData()
    {
        NpcData data = new NpcData();
        data.instanceId = this.instanceId;
        //InventoryCore 재사용
        if(shopInventory != null && shopInventory.Core != null)
        {
            data.inventory = shopInventory.Core.SaveData();
        }

        return data;
    }

}