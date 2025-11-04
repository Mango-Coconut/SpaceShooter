using System;
using System.Collections.Generic;
using UnityEngine;

public class Chest : InventoryMono, IInteractable
{
    [SerializeField] InteractionHub hub;

    [Header("Chest Identification")]
    [SerializeField] string instanceId; // 고유 식별자

    [Header("Initial Items (for editor setup)")]
    [SerializeField] StoredItem[] chestitems;
    public string InstanceId => instanceId;
    public static event Action<Chest> OnChestOpened;
    public static event Action<Chest> OnChestClosed;

    PlayerController owner;
    bool isOpen = false;

    [SerializeField] Sprite chestSprite;

    #region 초기화
#if UNITY_EDITOR
    static HashSet<string> usedIds = new HashSet<string>();
#endif
    protected override void Awake()
    {
        base.Awake();
        // ID가 비어 있으면 자동 생성
        if (string.IsNullOrWhiteSpace(instanceId))
        {
#if UNITY_EDITOR
            Log.Warn($"Chest '{name}' 이름 지정 하셈");
#endif
        }

#if UNITY_EDITOR
        // 중복 감지 (정적 HashSet으로 한 번만)
        if (!usedIds.Add(instanceId))
        {
            Debug.LogWarning($"[Chest] Duplicate instanceId detected: {instanceId} ({name})");
        }
#endif
    }
    #endregion

    void Start()
    {
        // 초기 아이템 추가
        if (chestitems != null)
        {
            foreach (var item in chestitems)
            {
                if (item.itemData == null)
                {
                    Log.Error($"{InstanceId} Chest에 아이템 지정하기");
                    break;
                }
                TryAddItem(item.itemData, item.count);
            }
        }
    }


    void OnDisable()
    {
        if(isOpen) ForceCloseFromUI();
    }

    public void Interact(PlayerController pc)
    {
        if (isOpen == false)
        {
            OpenChest(pc);
        }
        else
        {
            CloseChest(pc);
        }
    }

    void OpenChest(PlayerController pc)
    {
        isOpen = true;
        owner = pc;

        pc.gate.PushUI();
        if (hub != null && hub.chest != null)
        {
            hub.chest.RaiseOpen(this);
        }
    }
    void CloseChest(PlayerController pc)
    {
        isOpen = false;

        var targetPc = pc != null ? pc : owner;
        if (targetPc != null)
        {
            targetPc.gate.PopUI();
        }
        owner = null;

        if (hub != null && hub.chest != null)
        {
            hub.chest.RaiseClose(this);
        }
    }
    
    public void ForceCloseFromUI()
    {
        CloseChest(owner);
    }


    public bool IsAvailable() => true;
    public void OnFocus() { }
    public void OnUnfocus() { }

    public Sprite GetIcon() => chestSprite;
    public (string inputKeyText, string behaviorText) GetPrompt() => ("F", "열기");

    public ChestData SaveData()
    {
        ChestData data = new ChestData();
        data.instanceId = this.InstanceId;

        //InventoryCore 재사용
        InventoryCore core = this.Core;
        if (core != null)
        {
            data.inventory = core.SaveData();
        }

        return data;
    }
}
