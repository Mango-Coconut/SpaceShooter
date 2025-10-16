using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory : MonoBehaviour, IStorable
{
    List<StoredItem> slots = new List<StoredItem>();
    public List<StoredItem> Slots => slots;
    public int maxSlotNum = 10;

    public event Action Changed;

    protected void RaiseChanged()
    {
        Changed?.Invoke();
    }

    protected StoredItem FindSlot(ItemData data)
    {
        return slots.Find(s => s.itemdata == data);
    }

    public int CountOf(ItemData data)
    {
        StoredItem slot = FindSlot(data);
        return slot != null ? slot.count : 0;
    }

    public bool HasItem(ItemData data, int amount = 1)
    {
        return CountOf(data) >= amount;
    }

    public bool TryAddItem(ItemData data, int amount = 1)
    {
        if (data == null) return false;
        if (amount <= 0) return true; // 넣을 게 없음

        int remaining = amount;
        int actuallyAdded = 0;

        // 1) 기존 동일 아이템 슬롯들에 먼저 채우기 (maxStack까지)
        for (int i = 0; i < slots.Count; i++)
        {
            StoredItem s = slots[i];
            if (s.itemdata != data) continue;
            if (s.count >= data.maxStack) continue;

            int space = data.maxStack - s.count;
            int add = remaining < space ? remaining : space;

            s.count += add;
            remaining -= add;
            actuallyAdded += add;

            if (remaining <= 0)
                break;
        }

        // 2) 아직 남았고, 새 슬롯을 만들 수 있으면 새 슬롯들로 나눠 담기
        while (remaining > 0 && slots.Count < maxSlotNum)
        {
            int stackCount = remaining < data.maxStack ? remaining : data.maxStack;
            slots.Add(new StoredItem(data, stackCount));
            remaining -= stackCount;
            actuallyAdded += stackCount;
        }

        // 3) 아무것도 못 넣었다면 실패
        if (actuallyAdded == 0) return false;

        Debug.Log($"Inventory에 {data.name}을 {actuallyAdded} 만큼 추가");
        // 4) 이벤트 (성공한 만큼이라도 변화가 있었으니 갱신)
        RaiseChanged();

        // 5) 통계 반영 (나중에 구현할 때 여기서 actuallyAdded만큼 누적)
        // ItemStatistics.Instance.OnItemGet(data, actuallyAdded);

        // 전량 들어갔는지 여부 반환
        return remaining == 0;
    }

    public bool TryRemoveItem(ItemData data, int amount = 1)
    {
        if (data == null) return false;
        if (amount <= 0) return true;

        int remaining = amount;
        int actuallyRemoved = 0;

        // 1. 같은 아이템 슬롯들에서 순차적으로 제거
        for (int i = slots.Count - 1; i >= 0; i--) // 뒤에서부터 제거 (Remove 안전)
        {
            StoredItem s = slots[i];
            if (s.itemdata != data) continue;

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

            if (remaining <= 0)
                break;
        }

        Debug.Log($"Inventory에서 {data.name}을 {actuallyRemoved} 만큼 제거");
        if (actuallyRemoved > 0)
        {
            RaiseChanged();

            // 🔹 통계 반영 (나중에 구현)
            // ItemStatistics.Instance.OnItemUse(data);
        }

        // 전량 제거 성공 여부 반환
        return remaining == 0;
    }


    // 사용 로직: 기록 + 차감
    public bool UseItem(ItemData data, int useCount = 1)
    {
        StoredItem slot = FindSlot(data);

        if (slot == null) return false;
        // 보유한 아이템이 충분하지 않습니다! (그럴일 없겠지만)
        if (slot.count < useCount) return false;

        // (아이템 고유의 사용 로직이 있다면 여기서 호출)
        // 예: data.Use(this);

        slot.useCount++;
        slot.lastUsed = DateTime.Now;

        return TryRemoveItem(slot.itemdata, 1);
    }

    public void Sort(int index)
    {
        SortType key = (SortType)index;

        Comparison<StoredItem> comparison = (a, b) =>
        {
            switch (key)
            {
                case SortType.Rarity: return b.itemdata.rarity.CompareTo(a.itemdata.rarity);
                case SortType.Price: return b.itemdata.price.CompareTo(a.itemdata.price);
                case SortType.Weight: return b.itemdata.weight.CompareTo(a.itemdata.weight);
                case SortType.Volume: return b.itemdata.volume.CompareTo(a.itemdata.volume);
                case SortType.LastGet: return b.lastGet.CompareTo(a.lastGet);
                case SortType.LastUsed: return b.lastUsed.CompareTo(a.lastUsed);
                case SortType.UseCount: return b.useCount.CompareTo(a.useCount);
                default: return 0;
            }
        };

        Slots.Sort(comparison);
        RaiseChanged();
    }

}