using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldInventoryCore : IItemSource, IItemSink
{
    readonly List<DroppedItem> worldItems = new List<DroppedItem>();

    public event Action OnChanged;

    // Mono에게 실제 씬 조작을 부탁하는 이벤트
    public event Action<StoredItem> OnSpawnRequest;     //기본 인터페이스 함스
    public event Action<StoredItem, Transform> OnSpawnRequest_fromPlayer; // 플레이어가 버림 (→ 새 DroppedItem 필요)
    public event Action<DroppedItem> OnDespawnRequest;  // 플레이어가 주움 (→ 해당 DroppedItem 파괴)

    void RaiseChanged()
    {
        OnChanged?.Invoke();
    }

    public void RegisterExistingDrop(DroppedItem di)
    {
        // 씬 시작 시나 Spawn 후 Mono가 호출
        if (di == null) return;
        if (di.Item == null || di.Item.itemData == null) return;

        if (!worldItems.Contains(di))
        {
            worldItems.Add(di);
            RaiseChanged();
        }
    }

    public void UnregisterDrop(DroppedItem di)
    {
        if (di == null) return;
        if (worldItems.Remove(di))
        {
            RaiseChanged();
        }
    }

    DroppedItem FindDropByInstanceId(string instanceId)
    {
        // StoredItem은 instanceId 고유값을 들고 있음. :contentReference[oaicite:10]{index=10}
        for (int i = 0; i < worldItems.Count; i++)
        {
            DroppedItem d = worldItems[i];
            if (d != null && d.Item != null && d.Item.instanceId == instanceId)
            {
                return d;
            }
        }
        return null;
    }

    // ===== IItemSource (줍기) =====
    public bool CanRemoveItem(StoredItem item)
    {
        if (item == null) return false;
        if (item.itemData == null) return false;
        return FindDropByInstanceId(item.instanceId) != null;
    }

    public bool TryRemoveItem(StoredItem item)
    {
        if (!CanRemoveItem(item)) return false;

        DroppedItem target = FindDropByInstanceId(item.instanceId);
        if (target == null) return false;

        // 목록에서 제거
        worldItems.Remove(target);

        // 실제 GameObject 제거는 Mono에게 부탁
        OnDespawnRequest?.Invoke(target);

        RaiseChanged();
        return true;
    }

    // ===== IItemSink (버리기) =====
    public bool CanAddItem(StoredItem item)
    {
        if (item == null) return false;
        if (item.itemData == null) return false;
        return true;
        // 바닥은 무한. 나중에 '금지 구역이면 false' 같은 룰 추가 가능
    }
    public bool TryAddItem(StoredItem item)
    {
        if (!CanAddItem(item)) return false;

        // Core는 "드랍 오브젝트 만들어줘" 요청만 보냄.
        // 실제 DroppedItem 인스턴스 생성 -> RegisterExistingDrop까지는 Mono가 후속 처리.
        OnSpawnRequest?.Invoke(item);

        // worldItems에는 아직 안 넣었는데,
        // Mono가 Instantiate 한 직후 RegisterExistingDrop()으로 넣어줄 거라 일단 여기선 끝.
        return true;
    }
    public bool TryAddItem_PlayerDrop(StoredItem item, Transform dropper)
    {
        if (!CanAddItem(item)) return false;

        OnSpawnRequest_fromPlayer?.Invoke(item, dropper);
        return true;
    }

    public WorldDropData SaveData()
    {
        WorldDropData data = new WorldDropData();
        data.drops = new List<WorldDropEntry>();

        // 내부 보유 목록: worldItems(List<DroppedItem>) :contentReference[oaicite:12]{index=12}
        System.Reflection.FieldInfo fi = typeof(WorldInventoryCore).GetField("worldItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        List<DroppedItem> list = (List<DroppedItem>)fi.GetValue(this);

        for (int i = 0; i < list.Count; i++)
        {
            DroppedItem di = list[i];
            if (di == null || di.Item == null || di.Item.itemData == null)
            {
                continue;
            }

            WorldDropEntry entry = new WorldDropEntry();
            entry.storedItem = di.Item.SaveData();

            Vec3Data pos = new Vec3Data();
            pos.x = di.transform.position.x;
            pos.y = di.transform.position.y;
            pos.z = di.transform.position.z;
            entry.position = pos;

            Vec3Data rot = new Vec3Data();
            Vector3 e = di.transform.rotation.eulerAngles;
            rot.x = e.x;
            rot.y = e.y;
            rot.z = e.z;
            entry.rotationEuler = rot;

            data.drops.Add(entry);
        }

        return data;
    }
}