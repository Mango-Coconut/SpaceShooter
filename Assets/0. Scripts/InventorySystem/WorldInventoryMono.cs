using UnityEngine;

public class WorldInventoryMono : MonoBehaviour
{
    [SerializeField] DroppedItem droppedItemPrefab;
    // 플레이어가 바닥에 버렸을 때 만들 기본 프리팹
    // (Mesh, Collider, Renderer(발광), DroppedItem 컴포넌트 포함)

    public WorldInventoryCore Core { get; private set; }

    void Awake()
    {
        Core = new WorldInventoryCore();

        // Core -> Mono 이벤트 구독
        Core.OnSpawnRequest += HandleSpawnRequest;
        Core.OnSpawnRequest_fromPlayer += HandleSpawnRequest_FromPlayer;
        Core.OnDespawnRequest += HandleDespawnRequest;
    }

    void OnDestroy()
    {
        if (Core != null)
        {
            Core.OnSpawnRequest -= HandleSpawnRequest;
            Core.OnDespawnRequest -= HandleDespawnRequest;
        }
    }

    void Start()
    {
        // 기존 WorldInventory.Start() 동작 복원:
        DroppedItem[] droppedItems = FindObjectsOfType<DroppedItem>();
        for (int i = 0; i < droppedItems.Length; i++)
        {
            DroppedItem di = droppedItems[i];
            if (di != null && di.Item != null && di.Item.itemData != null)
            {
                di.SetWorldInventory(this); // 기존 WorldInventory.SetWorldInventory 호출과 동일 의도 
                Core.RegisterExistingDrop(di);
            }
        }

        Debug.Log($"WorldInventoryMono 초기화: {droppedItems.Length}개 아이템 등록됨");
    }

    // ================= Core 이벤트 처리 ================

    // Core.TryAddItem() 에 의해 호출됨 = "버린 아이템 월드에 스폰해줘"
    void HandleSpawnRequest(StoredItem item)
    {
        HandleSpawn(item, null);
    }
    void HandleSpawnRequest_FromPlayer(StoredItem item, Transform dropper)
    {
        HandleSpawn(item, dropper);
    }

    void HandleSpawn(StoredItem item, Transform dropper)
    {
        if (item == null || item.itemData == null) return;
        if (droppedItemPrefab == null)
        {
            Debug.LogWarning("WorldInventoryMono.HandleSpawn: droppedItemPrefab is null");
            return;
        }

        Vector3 spawnPos = GetDropPosition(dropper);

        DroppedItem newDrop = Instantiate(droppedItemPrefab, spawnPos, Quaternion.identity);
        newDrop.Bind(item);
        newDrop.SetWorldInventory(this);

        Core.RegisterExistingDrop(newDrop);
    }

    // Core.TryRemoveItem() 에 의해 호출됨 = "줍힌 아이템 씬에서 없애줘"
    void HandleDespawnRequest(DroppedItem di)
    {
        if (di == null) return;
        Destroy(di.gameObject);
    }

    // ================= 편의 함수 ================

    // DroppedItem이 자기 자신이 주워졌다고 알리거나 할 때 쓸 수도 있음
    public void NotifyPickedUp(DroppedItem di)
    {
        // 예전 WorldInventory.NotifyPickedUp()은 단순히 리스트에서 제거만 했음. :contentReference[oaicite:15]{index=15}
        // Core 쪽에서는 TryRemoveItem()이 worldItems.Remove까지 하므로,
        // DroppedItem 쪽에서 직접 이걸 호출할 필요 없을 수도 있다.
        Core.UnregisterDrop(di);
    }

    Vector3 GetDropPosition(Transform t)
    {
        Vector3 basePos = transform.position;

        if (t != null)
        {
            basePos = t.position + t.forward * 1.2f;
        }

        return basePos + Vector3.up * 0.5f;
    }
}
