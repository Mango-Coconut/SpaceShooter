using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public sealed class InventoryManager : MonoBehaviour
{
    [SerializeField] PlayerController player;

    // 인스펙터 할당
    [SerializeField] InventoryMono playerInventoryMono;
    [SerializeField] EquipInventoryMono equipInventoryMono;
    [SerializeField] WorldInventoryMono worldInventoryMono;

    public InventoryMono PlayerInventoryMono => playerInventoryMono;
    public EquipInventoryMono EquipInventoryMono => equipInventoryMono;
    public WorldInventoryMono WorldInventoryMono => worldInventoryMono;

    // 런타임 할당
    InventoryMono chestInventoryMono;
    public InventoryMono ChestInventoryMono => chestInventoryMono;


    IItemSource GetSource(StorageTarget target)
    {
        switch (target)
        {
            case StorageTarget.Player: return playerInventoryMono?.Core;
            case StorageTarget.Chest: return chestInventoryMono?.Core;
            case StorageTarget.Equip: return equipInventoryMono?.Core;
            case StorageTarget.World: return worldInventoryMono?.Core;
            default: return null;
        }
    }
    IItemSink GetSink(StorageTarget target)
    {
        switch (target)
        {
            case StorageTarget.Player: return playerInventoryMono?.Core;
            case StorageTarget.Chest: return chestInventoryMono?.Core;
            case StorageTarget.Equip: return equipInventoryMono?.Core;
            case StorageTarget.World: return worldInventoryMono?.Core;
            default: return null;
        }
    }

    PanelManager panel;

    #region 싱글톤
    private static InventoryManager instance;
    public static InventoryManager Instance
    {
        get
        {
            if (instance == null)
            {
#if UNITY_EDITOR
                Debug.LogError("[InventoryManager] Instance is null. Make sure a GameObject with InventoryManager exists in the scene.");
#endif
            }
            return instance;
        }
    }

    [SerializeField] private bool dontDestroyOnLoad = true;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
#if UNITY_EDITOR
            Debug.LogWarning("[InventoryManager] Duplicate detected. Destroying this component.", this);
#endif
            Destroy(gameObject);
            return;
        }

        instance = this;

        if (dontDestroyOnLoad == true)
        {
            DontDestroyOnLoad(gameObject);
        }

        // TODO: 초기화 필요 시 여기서 수행
        // Init();
        panel = FindObjectOfType<PanelManager>();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    #endregion

    void OnEnable()
    {
        SubsCribe();
    }
    void OnDisable()
    {
        UnSubsCribe();
    }
    void SubsCribe()
    {
        UnSubsCribe();
        panel.OnItemDropped += HandleItemDropped;
        panel.OnItemRightClicked += HandleRightClick;
        panel.OnChestClosed += ClearChestInventory;
        Chest.OnChestOpened += HandleSetChestInventory;
    }
    void UnSubsCribe()
    {
        panel.OnItemDropped -= HandleItemDropped;
        panel.OnItemRightClicked -= HandleRightClick;
        panel.OnChestClosed -= ClearChestInventory;
        Chest.OnChestOpened -= HandleSetChestInventory;
    }

    void HandleSetChestInventory(InventoryMono chestMono)
    {
        if (chestMono == null) Log.Error("InventoryManager -> Chest null");

        chestInventoryMono = chestMono;
    }
    void ClearChestInventory()
    {
        chestInventoryMono = null;
    }

    // 인벤토리에서 드래그 놓을 시 시작과 끝지점 이미 알고 있음
    void HandleItemDropped(StorageTarget from, StorageTarget to, StoredItem item)
    {
        TryDeliver(GetSource(from), GetSink(to), item);
    }

    // 인벤토리에서 우클릭시 시작 지점만 알고 있기에 옮길 Storage를 자동으로 설정
    public void HandleRightClick(StoredItem item, StorageTarget from)
    {
        TryAutoDeliver(item, from);
    }

    public bool TryAutoDeliver(StoredItem item, StorageTarget from)
    {
        if (item == null) return false;
        Log.Info($"InventoryManager -> TryAutoDeliver, {item.itemData.name}");

        var fromSrc = GetSource(from);
        var playerInv = playerInventoryMono?.Core;
        var chestInv = chestInventoryMono?.Core;
        var equipInv = equipInventoryMono?.Core;
        var worldInv = worldInventoryMono?.Core;

        bool fromEquip = ReferenceEquals(GetSource(from), equipInventoryMono?.Core);
        bool fromChest = ReferenceEquals(GetSource(from), chestInventoryMono?.Core);
        bool fromInv = ReferenceEquals(GetSource(from), playerInventoryMono?.Core);


        if (fromEquip)
        {
            return TryDeliverWithFallbacks(fromSrc, item, playerInv, chestInv);
        }
        else if (fromChest)
        {
            return TryDeliver(fromSrc, playerInv, item);
        }
        else if (fromInv)
        {
            IItemSink maybeChest = chestInv;
            IItemSink maybeEquip = (item.itemData?.type == ItemType.Equip) ? equipInv : null;

            return TryDeliverWithFallbacks(fromSrc, item, maybeChest, maybeEquip);
        }
        else // worldInventory → PlayerInventory
        {
            return TryDeliver(fromSrc, playerInv, item);
        }
    }


    public bool TryAddItem(IItemSink c, ItemData data, int amount = 1)
    {
        return c.TryAddItem(new StoredItem(data, amount));
    }
    public bool TryAddItem(IItemSink c, StoredItem item)
    {
        return c.TryAddItem(item);
    }
    public bool TryRemoveItem(IItemSource c, ItemData data, int amount = 1)
    {
        return c.TryRemoveItem(new StoredItem(data, amount));
    }
    public bool TryRemoveItem(IItemSource c, StoredItem item)
    {
        return c.TryRemoveItem(item);
    }

    // 여러 IItemSink에 순서대로 넣기 시도
    public bool TryDeliverWithFallbacks(IItemSource from, StoredItem item, params IItemSink[] sinks)
    {
        if (from == null || item == null || sinks == null) return false;

        for (int i = 0; i < sinks.Length; i++)
        {
            IItemSink sink = sinks[i];
            if (sink == null) continue;
            if (TryDeliver(from, sink, item))
                return true;
        }
        return false;
    }

    public bool TryDeliver(IItemSource from, IItemSink to, StoredItem item)
    {
        if (ReferenceEquals(to, worldInventoryMono.Core))
        {
            return DropToWorld(from, item);
        }
        if (to is ISwapSink swap)
        {
            return TryDeliverSwap(from, swap, item, (IItemSink)from);
        }
        else
        {
            return TryDeliverBasic(from, to, item);
        }
    }
    bool DropToWorld(IItemSource from, StoredItem item)
    {
        if (from == null || item == null) return false;
        if (worldInventoryMono == null || worldInventoryMono.Core == null) return false;

        // 1. from 인벤토리에서 뺄 수 있는지
        if (!from.CanRemoveItem(item)) return false;
        // 2. 월드가 받을 수 있는지
        if (!worldInventoryMono.Core.CanAddItem(item)) return false;

        // 3. 실제로 from에서 제거
        bool removed = from.TryRemoveItem(item);
        if (!removed) return false;
        // 4. 월드에 추가 (플레이어 Transform 넘겨줌)
        bool added = worldInventoryMono.Core.TryAddItem_PlayerDrop(item, player.transform);

        return added;
    }

    public bool TryDeliverBasic(IItemSource from, IItemSink to, StoredItem item)
    {
        if (from == null || to == null || item == null) return false;
        if (!from.CanRemoveItem(item)) return false;
        if (!to.CanAddItem(item)) return false;

        from.TryRemoveItem(item);
        to.TryAddItem(item);
        return true;
    }

    public bool TryDeliverSwap(IItemSource from, ISwapSink to, StoredItem item, IItemSink originSink)
    {
        if (from == null || to == null || originSink == null || item == null) return false;

        // 1) 사전검증: 꺼낼 수 있는가?
        if (!from.CanRemoveItem(item)) return false;

        // 2) 사전검증: 장비창이 이번 아이템을 받을 수 있는가? 그리고 무엇이 튀어나오는가?
        if (!to.CanAddItemSwap(item, out var willBeSwapped)) return false;

        // 3) 사전검증: 튀어나올 아이템이 있다면 원래 자리(or 지정된 곳)가 받을 수 있는가?
        if (willBeSwapped != null && !originSink.CanAddItem(willBeSwapped)) return false;

        // === 여기까지 오면 실패하지 않도록 보장됨 ===

        // 4) 실제 실행
        from.TryRemoveItem(item);                
        to.TryAddItemSwap(item, out var swapped);

        // 5) 기존 장비를 원래 자리로 복귀
        if (swapped != null)
        {
            originSink.TryAddItem(swapped);
        }

        return true;
    }

}