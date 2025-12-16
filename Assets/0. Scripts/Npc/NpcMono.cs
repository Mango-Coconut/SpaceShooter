using System;
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
        BindCore();
    }

    void OnDisable()
    {
        UnbindCore();
    }

    void BindCore()
    {
        Core.AttachDialogueEvents();
        Core.BindQuestStateProvider(GetQuestState);

        Core.OnDialogueEnded += HandleDialogueEndedFromCore;
        Core.OnOpenShopRequested += HandleOpenShopRequested;
        Core.OnStartQuestRequested += HandleStartQuestRequested;
        Core.OnCompleteQuestRequested += HandleCompleteQuestRequested;
        Core.OnEnterDialogueRequested += HandleEnterDialogueRequested;
    }

    void UnbindCore()
    {
        Core.DetachDialogueEvents();

        Core.OnDialogueEnded -= HandleDialogueEndedFromCore;
        Core.OnOpenShopRequested -= HandleOpenShopRequested;
        Core.OnStartQuestRequested -= HandleStartQuestRequested;
        Core.OnCompleteQuestRequested -= HandleCompleteQuestRequested;
        Core.OnEnterDialogueRequested -= HandleEnterDialogueRequested;
    }

    QuestState GetQuestState(QuestData questData)
    {
        return QuestManager.Instance.GetQuestState(questData);
    }

    void HandleDialogueEndedFromCore()
    {
        if (curPlayer != null) curPlayer.InteractExit();
    }

    void HandleOpenShopRequested()
    {
        if (shopInventory == null) return;
        OpenShop?.Invoke();
    }

    void HandleStartQuestRequested(QuestData quest)
    {
        bool started = QuestManager.Instance.TryStartQuest(quest, this);
        if (!started)
        {
            Log.Warn("StartQuest failed: " + quest.title);
        }
    }

    void HandleCompleteQuestRequested(QuestData quest)
    {
        bool completed = QuestManager.Instance.TryCompleteQuest(quest, this, curPlayer);
        if (!completed)
        {
            Log.Warn("CompleteQuest failed: " + quest.title);
        }
    }

    void HandleEnterDialogueRequested(DialogueAsset asset, string startNodeId)
    {
        if (asset.questData == null)
        {
            asset.questData = linkedQuest;
        }

        Core.Initialize(asset, startNodeId);
    }
    #endregion
    
    public void Interact(PlayerController pc)
    {
        if (!CanInteract()) return;
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
        if (shopInventory != null && shopInventory.Core != null)
        {
            data.inventory = shopInventory.Core.SaveData();
        }

        return data;
    }

    public bool CanInteract()
    {
        if (curPlayer != null) return false;

        return true;
    }
}