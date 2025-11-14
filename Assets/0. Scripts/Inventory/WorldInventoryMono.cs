using UnityEngine;

public class WorldInventoryMono : MonoBehaviour
{
    [SerializeField] DroppedItem droppedItemPrefab;
    // 플레이어가 바닥에 버렸을 때 만들 기본 프리팹
    // (Mesh, Collider, Renderer(발광), DroppedItem 컴포넌트 포함)

    public WorldInventoryCore Core { get; private set; }

    void Awake()
    {
        if (droppedItemPrefab == null)
        {
            Log.Warn("SpawnFromSave: droppedItemPrefab is null");
            return;
        }

        Core = new WorldInventoryCore();

        // Core -> Mono 이벤트 구독
        Core.OnSpawnRequest += HandleSpawnItem;
        Core.OnSpawnRequest_fromPlayer += HandleSpawnItem;
        Core.OnSpawnRequest_fromLoad += HandleSpawnItem;
        Core.OnDespawnRequest += HandleDespawnRequest;
    }

    void OnDestroy()
    {
        if (Core != null)   
        {
            Core.OnSpawnRequest -= HandleSpawnItem;
            Core.OnSpawnRequest_fromPlayer -= HandleSpawnItem;
            Core.OnSpawnRequest_fromLoad -= HandleSpawnItem;
            Core.OnDespawnRequest -= HandleDespawnRequest;
        }
    }

    void Start()
    {
        DroppedItem[] droppedItems = FindObjectsOfType<DroppedItem>();
        for (int i = 0; i < droppedItems.Length; i++)
        {
            DroppedItem di = droppedItems[i];
            if (di != null && di.Item != null && di.Item.itemData != null)
            {
                di.SetWorldInventory(this); 
                Core.RegisterExistingDrop(di);
            }
        }
    }

    #region 필드 아이템 생성
    void HandleSpawnItem(StoredItem item)
    {
        HandleSpawnItem(item, Vector3.zero, Vector3.zero);
    }

    public void HandleSpawnItem(StoredItem item, Vector3 position, Vector3 rotationEuler)
    {
        if (item == null || item.itemData == null)
        {
            Log.Warn("spawn item is null");
            return;
        }
        DroppedItem newDrop = Instantiate(droppedItemPrefab, position, Quaternion.Euler(rotationEuler));
        newDrop.Bind(item);
        newDrop.SetWorldInventory(this);
        Core.RegisterExistingDrop(newDrop);
    }
    public void HandleSpawnItem(StoredItem item, Transform dropper)
    {
        //Dropper 앞에다 스폰
        HandleSpawnItem(item, GetDropPosition(dropper), Vector3.zero);
    }
    Vector3 GetDropPosition(Transform t)
    {
        Vector3 basePos = transform.position;
        if (t != null) basePos = t.position + t.forward * 1.2f;

        return basePos + Vector3.up * 0.5f;
    }

    #endregion

    #region 필드 아이템 제거
    // Core.TryRemoveItem() 에 의해 호출됨 = "줍힌 아이템 씬에서 없애줘"
    void HandleDespawnRequest(DroppedItem di)
    {
        if (di == null) return;
        Destroy(di.gameObject);
    }
    #endregion

    public void ClearAllDrops()
    {
        // 자신의 관리 하위(트랜스폼 자식)만 정리하여 범위를 좁힘
        DroppedItem[] all = GetComponentsInChildren<DroppedItem>(true);
        for (int i = 0; i < all.Length; i++)
        {
            DroppedItem di = all[i];
            if (di == null) continue;
            if (Core != null)
            {
                Core.UnregisterDrop(di); // 코어 목록에서 제거 + OnChanged
            }
            Destroy(di.gameObject);       // 씬에서 제거
        }
    }
}
