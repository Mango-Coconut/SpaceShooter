using UnityEngine;

public sealed class InventoryManager : MonoBehaviour
{
    [SerializeField] InteractionHub hub;
    [SerializeField] PlayerController player;

    [SerializeField] InventoryMono playerInventoryMono;
    [SerializeField] EquipInventoryMono equipInventoryMono;
    [SerializeField] WorldInventoryMono worldInventoryMono;

    public InventoryMono PlayerInventoryMono => playerInventoryMono;
    public EquipInventoryMono EquipInventoryMono => equipInventoryMono;
    public WorldInventoryMono WorldInventoryMono => worldInventoryMono;

    InventoryMono chestInventoryMono;
    public InventoryMono ChestInventoryMono => chestInventoryMono;

    InventoryMono shopInventoryMono;
    public InventoryMono ShopInventoryMono => shopInventoryMono;

    PanelManager panel;

    #region Singleton
    static InventoryManager instance;
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

    [SerializeField] bool dontDestroyOnLoad = true;

    void Awake()
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

        if (dontDestroyOnLoad)
        {
            DontDestroyOnLoad(gameObject);
        }

        if (player == null)
        {
            Log.Warn("PlayerController is null.");
        }

        panel = FindObjectOfType<PanelManager>();
        if (panel == null)
        {
            Log.Warn("PanelManager is null.");
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
    #endregion

    void OnEnable()
    {
        Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    void Subscribe()
    {
        Unsubscribe();

        if (panel != null)
        {
            panel.OnItemDropped += HandleItemDropped;
            panel.OnItemRightClicked += HandleRightClick;
        }

        if (hub != null)
        {
            if (hub.chest != null)
            {
                hub.chest.OnOpen += HandleSetChestInventory;
                hub.chest.OnClose += HandleClearChestInventory;
            }
            if (hub.npc != null)
            {
                hub.npc.OnEnter += HandleSetNpcInventory;
                hub.npc.OnExit += HandleClearNpcInventory;
            }
        }
    }

    void Unsubscribe()
    {
        if (panel != null)
        {
            panel.OnItemDropped -= HandleItemDropped;
            panel.OnItemRightClicked -= HandleRightClick;
        }

        if (hub != null && hub.chest != null)
        {
            hub.chest.OnOpen -= HandleSetChestInventory;
            hub.chest.OnClose -= HandleClearChestInventory;
        }
    }

    #region Chest inventory set

    void HandleSetChestInventory(InventoryMono chestMono)
    {
        if (chestMono == null)
        {
            Log.Error("InventoryManager -> Chest is null on open.");
            return;
        }
        chestInventoryMono = chestMono;
    }

    void HandleClearChestInventory(InventoryMono chestMono)
    {
        if (chestMono == null)
        {
            Log.Error("InventoryManager -> Chest is null on close.");
            return;
        }
        if (ReferenceEquals(chestInventoryMono, chestMono))
        {
            chestInventoryMono = null;
        }
    }
    #endregion

    #region Npc inventory set
    void HandleSetNpcInventory(NpcMono npc)
    {
        if(npc == null || npc.ShopInventory == null)
        {
            Log.Error($"NPC is null on Open");
            return;
        }
        shopInventoryMono = npc.ShopInventory;
    }

    void HandleClearNpcInventory(NpcMono npc)
    {
        if (npc == null || npc.ShopInventory == null)
        {
            Log.Error($"NPC is null on close");
            return;
        }
        if (ReferenceEquals(shopInventoryMono, npc.ShopInventory))
        {
            shopInventoryMono = null;
        }
    }
    #endregion

    #region  Drag & Drop / Right Click
    void HandleItemDropped(StorageTarget from, StorageTarget to, StoredItem item)
    {
        IItemSource fromSource = GetSource(from);
        IItemSink toSink = GetSink(to);

        TryDeliver(fromSource, toSink, item);
    }

    public void HandleRightClick(StoredItem item, StorageTarget from)
    {
        TryAutoDeliver(item, from);
    }
    #endregion

    #region 유틸 함수
    IItemSource GetSource(StorageTarget target)
    {
        switch (target)
        {
            case StorageTarget.Player:
                return playerInventoryMono != null ? playerInventoryMono.Core : null;
            case StorageTarget.Chest:
                return chestInventoryMono != null ? chestInventoryMono.Core : null;
            case StorageTarget.Equip:
                return equipInventoryMono != null ? equipInventoryMono.Core : null;
            case StorageTarget.World:
                return worldInventoryMono != null ? worldInventoryMono.Core : null;
            default:
                return null;
        }
    }

    IItemSink GetSink(StorageTarget target)
    {
        return (IItemSink)GetSource(target);
    }
    #endregion
    
    // ----- Auto Deliver -----

    public bool TryAutoDeliver(StoredItem item, StorageTarget from)
    {
        if (item == null)
        {
            return false;
        }

        IItemSource fromSource = GetSource(from);
        if (fromSource == null)
        {
            return false;
        }

        IItemSink playerInv = GetSink(StorageTarget.Player);
        IItemSink chestInv = GetSink(StorageTarget.Chest);
        IItemSink equipInv = GetSink(StorageTarget.Equip);
        IItemSink worldInv = GetSink(StorageTarget.World);

        bool fromEquip = ReferenceEquals(fromSource, equipInv);
        bool fromChest = ReferenceEquals(fromSource, chestInv);
        bool fromPlayer = ReferenceEquals(fromSource, playerInv);

        if (fromEquip)
        {
            // 장비 해제 → 인벤토리 우선, 안 되면 체스트
            return TryDeliverWithFallbacks(fromSource, item, playerInv, chestInv);
        }

        if (fromChest)
        {
            // 체스트 → 인벤토리 우선, 안 되면 장비
            return TryDeliverWithFallbacks(fromSource, item, playerInv, equipInv);
        }

        if (fromPlayer)
        {
            // 인벤토리 → 장비 가능하면 장비, 아니면 체스트 (체스트 열려있을 때)
            IItemSink maybeEquip = (item.itemData != null && item.itemData.type == ItemType.Equip) ? equipInv : null;
            IItemSink maybeChest = chestInv;
            return TryDeliverWithFallbacks(fromSource, item, maybeEquip, maybeChest);
        }

        // 그 외 (월드 등) → 플레이어 인벤토리로
        return TryDeliver(fromSource, playerInv, item);
    }

    
    public bool TryAddItem(IItemSink sink, ItemData data, int amount = 1)
    {
        if (sink == null || data == null || amount <= 0)
        {
            return false;
        }
        StoredItem item = new StoredItem(data, amount);
        return sink.TryAddItem(item);
    }

    public bool TryAddItem(IItemSink sink, StoredItem item)
    {
        if (sink == null || item == null)
        {
            return false;
        }
        return sink.TryAddItem(item);
    }

    public bool TryRemoveItem(IItemSource source, ItemData data, int amount = 1)
    {
        if (source == null || data == null || amount <= 0)
        {
            return false;
        }
        StoredItem temp = new StoredItem(data, amount);
        return source.TryRemoveItem(temp);
    }

    public bool TryRemoveItem(IItemSource source, StoredItem item)
    {
        if (source == null || item == null)
        {
            return false;
        }
        return source.TryRemoveItem(item);
    }

    // IItemSink 여러개에 순서대로 TryDeliver
    public bool TryDeliverWithFallbacks(IItemSource from, StoredItem item, params IItemSink[] sinks)
    {
        if (from == null || item == null || sinks == null)
        {
            return false;
        }

        for (int i = 0; i < sinks.Length; i++)
        {
            IItemSink sink = sinks[i];
            if (sink == null)
            {
                continue;
            }

            if (TryDeliver(from, sink, item))
            {
                return true;
            }
        }

        return false;
    }

    public bool TryDeliver(IItemSource from, IItemSink to, StoredItem item)
    {
        if (from == null || to == null || item == null) return false;

        // 월드 드랍이면 월드 드랍하기
        if (ReferenceEquals(to, worldInventoryMono != null ? worldInventoryMono.Core : null))
        {
            return DropToWorld(from, item);
        }

        // 장비창이면 스왑으로
        ISwapSink swap = to as ISwapSink;
        if (swap != null)
        {
            return TryDeliverSwap(from, swap, item, (IItemSink)from);
        }

        //둘 다 아니면 그냥 옮기기
        return TryDeliverBasic(from, to, item);
    }

    bool DropToWorld(IItemSource from, StoredItem item)
    {
        if (from == null || item == null) return false;
        if (worldInventoryMono == null || worldInventoryMono.Core == null) return false;

        // 1. 원본에서 빼기
        if (!from.TryRemoveItem(item))
        {
            return false;
        }

        // 2. 월드 인벤토리에 드랍 (실패하면 롤백)
        if (!worldInventoryMono.Core.TryAddItem_PlayerDrop(item, player.transform))
        {
            TryRestoreToSource(from, item, "DropToWorld");
            return false;
        }

        return true;
    }

    public bool TryBuyItem(StoredItem item, int amount)
    {
        StoredItem transfer;
        if (item.IsUniqueInstance())
        {
            transfer = item;
        }
        else
        {
            transfer = new StoredItem(item.itemData, amount);
        }
        return TryDeliverBasic(shopInventoryMono.Core, playerInventoryMono.Core, transfer);
    }

    public bool TryDeliverBasic(IItemSource from, IItemSink to, StoredItem item)
    {
        if (from == null || to == null || item == null)
        {
            return false;
        }

        // 1. 원본에서 제거 시도
        if (!from.TryRemoveItem(item))
        {
            return false;
        }

        // 2. 대상에 추가 시도
        if (!to.TryAddItem(item))
        {
            // 실패하면 롤백
            TryRestoreToSource(from, item, "TryDeliverBasic");
            return false;
        }

        return true;
    }

    public bool TryDeliverSwap(IItemSource from, ISwapSink to, StoredItem item, IItemSink originSink)
    {
        if (from == null || to == null || originSink == null || item == null) return false;

        // 1. 원본에서 빼기
        if (!from.TryRemoveItem(item))
        {
            return false;
        }

        // 2. 장비창에 스왑 시도
        StoredItem swapped;
        if (!to.TryAddItemSwap(item, out swapped))
        {
            // 장비 실패 → 원래대로 롤백
            TryRestoreToSink(originSink, item, "TryDeliverSwap: equip failed");
            return false;
        }

        // 3. 기존 장비를 originSink로 돌려보내기 (있다면)
        if (swapped != null)
        {
            if (!originSink.TryAddItem(swapped))
            {
                // 되돌릴 공간 없음 → 전체 롤백 시도
                bool revert = to.TryAddItemSwap(swapped, out var backToOrigin);
                StoredItem toRestore = backToOrigin ?? item;

                if (!originSink.TryAddItem(toRestore) || !revert)
                {
                    Log.Error("TryDeliverSwap rollback failed.");
                }

                return false;
            }
        }

        return true;
    }

    bool TryRestoreToSource(IItemSource source, StoredItem item, string context)
    {
        IItemSink sink = source as IItemSink;
        if (sink != null)
        {
            return TryRestoreToSink(sink, item, context);
        }

        Log.Error(context + " rollback failed: source is not IItemSink.");
        return false;
    }

    bool TryRestoreToSink(IItemSink sink, StoredItem item, string context)
    {
        if (sink == null)
        {
            Log.Error(context + " rollback failed: sink is null.");
            return false;
        }

        if (sink.TryAddItem(item))
        {
            return true;
        }

        Log.Error(context + " rollback failed: cannot restore item.");
        return false;
    }
}