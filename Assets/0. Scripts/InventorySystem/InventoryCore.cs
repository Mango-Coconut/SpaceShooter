using System;
using System.Collections.Generic;

public class InventoryCore : IItemSource, IItemSink
{
    int capacity = 20;
    public int Capacity => capacity;

    List<StoredItem> slots = new List<StoredItem>();

    public Action OnChanged;

    public IReadOnlyList<StoredItem> Slots => slots;

    public InventoryCore(int capacity)
    {
        this.capacity = capacity;
    }

    void RaiseChanged() => OnChanged?.Invoke();

    // 메인 진입점
    public bool TryAddItem(StoredItem incoming)
    {
        if (incoming == null || incoming.itemData == null) return false;
        if (incoming.count <= 0) return true;

        // 스택 가능한 경우 (un-unique)
        if (!incoming.IsUniqueInstance())
        {
            return TryAddItem(incoming.itemData, incoming.count);
        }

        // 유니크(강화, 파츠, 내구도 등) → 인스턴스 단위로 추가
        int toAdd = Math.Max(1, incoming.count);
        int added = 0;

        for (int i = 0; i < toAdd; i++)
        {
            if (slots.Count >= capacity) break;

            StoredItem copy = CloneAsSingle(incoming); // 상태 복제
            copy.count = 1;
            slots.Add(copy);
            added++;
        }

        if (added == 0) return false;

        // 통계(선택)
        // ItemStatistics.Instance.OnItemGet(incoming.itemdata, added);

        RaiseChanged();
        return added == toAdd;
    }

    // 스택형 전용 (ItemData + count)
    public bool TryAddItem(ItemData data, int amount = 1)
    {
        if (amount == 0) amount = 1;
        if (data == null || amount <= 0) return false;

        int remaining = amount;
        int actuallyAdded = 0;

        // 1. 기존 스택 채우기
        for (int i = 0; i < slots.Count; i++)
        {
            StoredItem s = slots[i];
            if (s.itemData != data) continue;
            if (s.count >= data.maxStack) continue;

            int space = data.maxStack - s.count;
            int add = remaining < space ? remaining : space;

            s.count += add;
            remaining -= add;
            actuallyAdded += add;

            if (remaining <= 0)
                break;
        }

        // 2. 새 스택 생성
        while (remaining > 0 && slots.Count < capacity)
        {
            int stackCount = remaining < data.maxStack ? remaining : data.maxStack;
            slots.Add(new StoredItem(data, stackCount));
            remaining -= stackCount;
            actuallyAdded += stackCount;
        }

        if (actuallyAdded == 0) return false;

        // 통계(선택)
        // ItemStatistics.Instance.OnItemGet(data, actuallyAdded);

        RaiseChanged();
        return remaining == 0;
    }

    // ✅ StoredItem 기반 제거
    //   - 유니크(장비)는 instanceId로 정확히 제거
    //   - 스택형은 같은 데이터에서 수량 제거 (전량/부분)
    public bool TryRemoveItem(StoredItem target)
    {
        if (target == null || target.itemData == null)
        {
            Log.Info($"storeditem or itemdata is null");
            return false;
        }

        // 유니크/개체 제거: instanceId로 정확히 찾기
        if (target.IsUniqueInstance())
        {
            int idx = slots.FindIndex(s => s.instanceId == target.instanceId);
            if (idx < 0)
            {
                Log.Info($"idx < 0");
                return false;
            }

            slots.RemoveAt(idx);
            // ItemStatistics.Instance.OnItemUse(target.itemdata);
            RaiseChanged();
            return true;
        }

        // 스택형 제거: 해당 데이터에서 count 만큼 제거
        return TryRemoveItem(target.itemData, target.count);
    }

    // ✅ 기존 시그니처도 유지 (스택형 전용)
    public bool TryRemoveItem(ItemData data, int amount = 1)
    {
        if (data == null) return false;
        if (amount <= 0) return true;

        int remaining = amount;
        int actuallyRemoved = 0;

        // 뒤에서 앞으로(삭제 안전)
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            StoredItem s = slots[i];
            if (s.itemData != data) continue;

            if (s.IsUniqueInstance())
            {
                // 스택형 제거 경로에서는 유니크는 건드리지 않음
                continue;
            }

            if (s.count <= remaining)
            {
                remaining -= s.count;
                actuallyRemoved += s.count;
                slots.RemoveAt(i);
            }
            else
            {
                s.count -= remaining;
                actuallyRemoved += remaining;
                remaining = 0;
                break;
            }
            if (remaining <= 0) break;
        }

        if (actuallyRemoved > 0)
        {
            // ItemStatistics.Instance.OnItemUse(data);
            RaiseChanged();
        }

        return remaining == 0;
    }

    // 슬롯 여유 개수
    int GetFreeSlotCount()
    {
        int free = capacity - slots.Count;
        return free > 0 ? free : 0;
    }

    // 해당 아이템(Data)의 기존 스택에서 더 담을 수 있는 총 공간
    int GetStackableSpace(ItemData data)
    {
        if (data == null) return 0;
        int space = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            StoredItem s = slots[i];
            if (s.itemData != data) continue;
            if (s.count >= data.maxStack) continue;
            if (s.IsUniqueInstance()) continue; // 유니크는 스택 공간에 포함 X
            space += (data.maxStack - s.count);
        }
        return space;
    }

    // ======================= CanAdd =======================

    public bool CanAddItem(StoredItem incoming)
    {
        if (incoming == null || incoming.itemData == null) return false;
        if (incoming.count <= 0) return true;

        if (!incoming.IsUniqueInstance())
        {
            // 스택형: 기존 스택 공간 + (빈 슬롯 * maxStack) 로 수용 가능 여부
            int space = GetStackableSpace(incoming.itemData)
                        + GetFreeSlotCount() * incoming.itemData.maxStack;
            return space >= incoming.count;
        }

        // 유니크: 1개당 1슬롯 필요
        int needed = Math.Max(1, incoming.count);
        return GetFreeSlotCount() >= needed;
    }

    public bool CanAddItem(ItemData data, int amount = 1)
    {
        if (data == null) return false;
        if (amount <= 0) return true;

        // 스택형 전용 체크 (유니크는 StoredItem 기준으로 체크 권장)
        int space = GetStackableSpace(data)
                    + GetFreeSlotCount() * data.maxStack;
        return space >= amount;
    }

    // ======================= CanRemove =======================

    public bool CanRemoveItem(StoredItem target)
    {
        if (target == null || target.itemData == null) return false;
        if (target.count <= 0) return true;

        if (target.IsUniqueInstance())
        {
            // instanceId로 정확히 존재하는지
            int idx = slots.FindIndex(s => s.instanceId == target.instanceId);
            return idx >= 0;
        }

        // 스택형: 동일 데이터(비-유니크) 총합이 충분한지
        int total = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            StoredItem s = slots[i];
            if (s.itemData != target.itemData) continue;
            if (s.IsUniqueInstance()) continue;
            total += s.count;
            if (total >= target.count) return true;
        }
        return false;
    }

    public bool CanRemoveItem(ItemData data, int amount = 1)
    {
        if (data == null) return false;
        if (amount <= 0) return true;

        int total = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            StoredItem s = slots[i];
            if (s.itemData != data) continue;
            if (s.IsUniqueInstance()) continue; // 유니크는 이 경로에서 제거하지 않음
            total += s.count;
            if (total >= amount) return true;
        }
        return false;
    }

    public bool UseItem(ItemData itemData)
    {
        bool isUse = TryRemoveItem(itemData);
        if (isUse) Log.Info($"아이템 사용 성공");
        else Log.Info($"아이템 사용 실패");
        return isUse;
    }

    StoredItem CloneAsSingle(StoredItem src)
    {
        // 유니크를 “1개짜리 복제본”으로 만들어 새 슬롯에 넣기
        StoredItem c = new StoredItem(src.itemData, 1);
        c.enhancement = src.enhancement;
        return c;
    }
}