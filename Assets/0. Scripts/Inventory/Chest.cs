using System;
using System.Collections.Generic;
using UnityEngine;

public class Chest : InventoryMono, IInteractable
{
    // Interact시 발송할 이벤트
    [SerializeField] GameEventHub hub;

    [Header("Chest Identification")]
    [SerializeField] string instanceId; // 고유 식별자
    public string InstanceId => instanceId;

    [Header("Initial Items (for editor setup)")]
    [SerializeField] StoredItem[] chestitems;
    [SerializeField] int chestCoins;

    PlayerController curPlayer;
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
                    Log.Error($"{InstanceId} Chest에 아이템 지정하기 실패");
                    break;
                }
                TryAddItem(item);
            }
        }
        if(chestCoins > 0)
        {
            Core.TryAddCoin(chestCoins);
        }
    }


    void OnDisable()
    {
        if(isOpen) ForceCloseFromUI();
    }

    public void Interact(PlayerController pc)
    {
        if (isOpen) return;

        isOpen = true;

        curPlayer = pc;
        pc.gate.PushUI();

        hub.chest.RaiseOpen(this);
    }

    public void Exit()
    {
        if(!isOpen) return;

        isOpen = false;

        curPlayer.gate.PopUI();
        curPlayer = null;

        hub.chest.RaiseClose(this);
    }


    public void ForceCloseFromUI()
    {
        Exit();
    }


    public bool IsAvailable() => true;
    public void OnFocus() { }
    public void OnUnfocus() {}

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

    public bool CanInteract()
    {
        if(curPlayer != null) return false;

        return true;
    }
}
