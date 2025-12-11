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

    ShopInventory shopInventory;
    public ShopInventory ShopInventory => shopInventory;
    
    Animator animator;
    PlayerController curPlayer;
    
    // Interact시 발송할 이벤트
    [SerializeField] GameEventHub hub;

    // 대화 에셋
    [SerializeField] DialogueAsset dialogueAsset;

    [SerializeField] QuestData linkedQuest;   // 이 NPC가 주는 퀘스트 하나

    // 대화를 어디서부터 시작할 지
    [Header("Dialogue Start Nodes By QuestState")]
    [SerializeField] string nodeLocked;        // 조건 부족 (레벨/선행퀘 등)
    [SerializeField] string nodeCanAccept;     // 퀘스트 수락 가능한 상태
    [SerializeField] string nodeInProgress;    // 진행 중
    [SerializeField] string nodeReadyToTurnIn; // 완료 조건 충족 (보고만 하면 됨)
    [SerializeField] string nodeCompleted;     // 이미 완료 후

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
        if (curPlayer != null) curPlayer.InteractExit();
    }

    void HandleCommand(DialogueCommand command, DialogueAsset nowAsset)
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
            case DialogueCommandType.EnterNewDialogue:
                HandleEnterNewDialogue(command.newAsset);
                break;
            case DialogueCommandType.StartQuest:
                HandleStartQuest(nowAsset.questData);
                break;

            case DialogueCommandType.CompleteQuest:
                HandleCompleteQuest(nowAsset.questData);
                break;
        }
    }

    #endregion
    public void Interact(PlayerController pc)
    {
        if(!CanInteract()) return;
        Enter(pc);
    }
    public void Enter(PlayerController pc)
    {
        if (pc == null || pc.gate == null) return;
        if (hub == null || hub.npc == null) return;

        curPlayer = pc;
        curPlayer.gate.PushUI();

        hub.npc.RaiseEnter(this);
        Core.Initialize(dialogueAsset);
    }
    
    public void Exit()
    {
        if (curPlayer == null || curPlayer.gate == null) return;
        if (hub == null || hub.npc == null) return;

        curPlayer.gate.PopUI();
        curPlayer = null;

        hub.npc.RaiseExit(this);
    }
    
    void HandleEnterNewDialogue(DialogueAsset newAsset)
    {
        if (newAsset.questData == null)
        {
            // 커맨드에서 안 넘겨줬으면, 이 NPC의 linkedQuest 사용
            newAsset.questData = linkedQuest;
            if (newAsset.questData == null) return;
        }

        QuestState state = QuestManager.Instance.GetQuestState(newAsset.questData);

        // 최종 상태 기준으로 진입할 노드 고르기
        if (newAsset.questData.questDialogue == null)
            return;

        string nodeId = newAsset.questData.GetNodeIdByState(state); 
        Debug.Log($"state : {state}, nodeid : {nodeId}");

        // 퀘스트 전용 대화 에셋으로 갈아타서, 해당 상태 노드로 진입
        Core.Initialize(newAsset, nodeId);
    }
    void HandleStartQuest(QuestData quest)
    {
        bool started = QuestManager.Instance.TryStartQuest(quest, this);
        if (!started)
        {
            Debug.Log("StartQuest failed: " + quest.title);
        }
    }

    void HandleCompleteQuest(QuestData quest)
    {
        bool completed = QuestManager.Instance.TryCompleteQuest(quest, this, curPlayer);
        if (!completed)
        {
            // Core.dialogueCore.GotoQuestCompleteFailedNode();
            Debug.Log("CompleteQuest failed: " + quest.title);
        }
    }


    public void EnrollQuest(QuestData data)
    {
        linkedQuest = data;
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
        //Exit();
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

    public bool CanInteract()
    {
        if(curPlayer != null) return false;

        return true;
    }
}