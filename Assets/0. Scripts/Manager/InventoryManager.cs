using UnityEngine;

public sealed class InventoryManager : MonoBehaviour
{
    [SerializeField] PlayerController player;

    // 인스펙터에서 할당
    [SerializeField] InventoryMono playerInventoryMono;
    [SerializeField] EquipInventoryMono equipInventoryMono;
    [SerializeField] WorldInventoryMono worldInventoryMono;

    public InventoryMono PlayerInventoryMono => playerInventoryMono;
    public EquipInventoryMono EquipInventoryMono => equipInventoryMono;
    public WorldInventoryMono WorldInventoryMono => worldInventoryMono;

    // 현재 활성화된 상자 인벤토리
    InventoryMono chestInventoryMono;
    public InventoryMono ChestInventoryMono => chestInventoryMono;


    // StorageTarget 간 공통 접근 로직 (이동 처리 중복 최소화)
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

    #region Singleton
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

        // TODO: 추가 초기화가 필요하면 여기서 처리
        if (player == null) Log.Warn($"PlayerController null");

        panel = FindObjectOfType<PanelManager>();
        if (panel == null) Log.Warn($"PanelManager null");
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

        Chest.OnChestClosed += ClearChestInventory;
        Chest.OnChestOpened += HandleSetChestInventory;
    }
    void UnSubsCribe()
    {
        panel.OnItemDropped -= HandleItemDropped;
        panel.OnItemRightClicked -= HandleRightClick;

        Chest.OnChestClosed -= ClearChestInventory;
        Chest.OnChestOpened -= HandleSetChestInventory;
    }

    void HandleSetChestInventory(InventoryMono chestMono)
    {
        if (chestMono == null) Log.Error("InventoryManager -> Chest null");
        else chestInventoryMono = chestMono;

    }
    void ClearChestInventory(InventoryMono chestMono)
    {
        if (chestMono == null) Log.Error("InventoryManager -> Chest null");
        else chestInventoryMono = null;
    }

    // 인벤토리 드래그-드롭 이벤트 콜백
    void HandleItemDropped(StorageTarget from, StorageTarget to, StoredItem item)
    {
        TryDeliver(GetSource(from), GetSink(to), item);
    }

    // 인벤토리 우클릭 시 자동 이동 처리
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
            return TryDeliverWithFallbacks(fromSrc, item, playerInv, equipInv);
        }
        else if (fromInv)
        {
            IItemSink maybeChest = chestInv;
            IItemSink maybeEquip = (item.itemData?.type == ItemType.Equip) ? equipInv : null;

            return TryDeliverWithFallbacks(fromSrc, item, maybeChest, maybeEquip);
        }
        else // 월드 -> 플레이어 인벤토리 이동
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

    // 여러 IItemSink 대상에 순차적으로 전달 시도
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

        // 1. 원본 인벤토리에서 제거 가능한지 확인
        if (!from.CanRemoveItem(item)) return false;
        // 2. 월드 인벤토리가 아이템을 받을 수 있는지 확인
        if (!worldInventoryMono.Core.CanAddItem(item)) return false;

        // 3. 실제로 원본에서 아이템 제거
        bool removed = from.TryRemoveItem(item);
        if (!removed) return false;
        // 4. 월드에 드롭 (플레이어 Transform 전달)
        bool added = worldInventoryMono.Core.TryAddItem_PlayerDrop(item, player.transform);

        if (!added)
        {
            TryRestoreToSource(from, item, "DropToWorld");
            return false;
        }

        return true;
    }

    public bool TryDeliverBasic(IItemSource from, IItemSink to, StoredItem item)
    {
        if (from == null || to == null || item == null) return false;
        if (!from.CanRemoveItem(item)) return false;
        if (!to.CanAddItem(item)) return false;

        if (!from.TryRemoveItem(item)) return false;
        if (!to.TryAddItem(item))
        {
            TryRestoreToSource(from, item, "TryDeliverBasic");
            return false;
        }
        return true;
    }

    public bool TryDeliverSwap(IItemSource from, ISwapSink to, StoredItem item, IItemSink originSink)
    {
        if (from == null || to == null || originSink == null || item == null) return false;

        // 1) 원본에서 아이템을 꺼낼 수 있는지 확인
        if (!from.CanRemoveItem(item)) return false;

        // 2) 장비 창이 아이템을 수용할 수 있는지 및 스왑 대상 확보
        if (!to.CanAddItemSwap(item, out var willBeSwapped)) return false;

        // 3) 스왑될 아이템이 돌아갈 공간 확인 (같은 컨테이너라면 제거 과정에서 공간 확보)
        bool sameContainer = ReferenceEquals(originSink, from);
        if (willBeSwapped != null && !sameContainer && !originSink.CanAddItem(willBeSwapped)) return false;

        // 4) 실제 이동 수행
        if (!from.TryRemoveItem(item)) return false;                
        if (!to.TryAddItemSwap(item, out var swapped))
        {
            TryRestoreToSink(originSink, item, "TryDeliverSwap: 장착 실패 롤백");
            return false;
        }

        // 5) 기존 장비를 원래 인벤토리로 되돌리기
        if (swapped != null)
        {
            if (!TryRestoreToSink(originSink, swapped, "TryDeliverSwap: 기존 장비 반환"))
            {
                // 원래 인벤토리에 돌려놓지 못하면 전체 롤백
                bool revertEquip = to.TryAddItemSwap(swapped, out var backToOrigin);
                var itemToRestore = backToOrigin ?? item;
                bool restoredItem = TryRestoreToSink(originSink, itemToRestore, "TryDeliverSwap: 전체 롤백");

                if (!revertEquip || !restoredItem)
                {
                    Log.Error("TryDeliverSwap 롤백 실패: 스왑 복구 중 상태가 어긋났습니다.");
                }
                return false;
            }
        }

        return true;
    }

    private bool TryRestoreToSource(IItemSource source, StoredItem item, string context)
    {
        if (source is IItemSink sink)
        {
            return TryRestoreToSink(sink, item, context);
        }

        Log.Error($"{context} 롤백 불가: 원본이 IItemSink를 구현하지 않습니다.");
        return false;
    }

    private bool TryRestoreToSink(IItemSink sink, StoredItem item, string context)
    {
        if (sink == null)
        {
            Log.Error($"{context} 롤백 불가: 대상 싱크가 null입니다.");
            return false;
        }

        if (sink.TryAddItem(item))
        {
            return true;
        }

        Log.Error($"{context} 롤백 실패: 아이템을 되돌릴 수 없습니다.");
        return false;
    }

}
