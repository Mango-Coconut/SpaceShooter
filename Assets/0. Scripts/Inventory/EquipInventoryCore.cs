using System;
using System.Collections.Generic;

public class EquipInventoryCore : IItemSink, IItemSource, ISwapSink
{
    readonly Dictionary<EquipType, StoredItem> equipped = new Dictionary<EquipType, StoredItem>();
    public IReadOnlyDictionary<EquipType, StoredItem> Equipped => equipped;
    public bool TryGetEquipped(EquipType slot, out StoredItem item)
    {
        return equipped.TryGetValue(slot, out item);
    }

    public event Action OnChanged;
    void RaiseChanged()
    {
        OnChanged?.Invoke();
    }

    #region  Item Add, Remove, Swap
    public bool CanAddItem(StoredItem item)
    {
        if (item == null) return false;
        if (item.itemData.equiptype == EquipType.None) return false;
        return true;
    }

    public bool TryAddItem(StoredItem item)
    {
        if (!CanAddItem(item)) return false;

        //해당 타입의 장비가 비어있을 경우만 장착 가능
        var slot = item.itemData.equiptype;
        if (equipped.ContainsKey(slot)) return false;

        equipped[slot] = item;
        RaiseChanged();
        return true;
    }

    public bool CanRemoveItem(StoredItem item)
    {
        if (item == null || item.itemData == null) return false;
        var slot = item.itemData.equiptype;
        if (!equipped.TryGetValue(slot, out var equippedItem) || equippedItem == null)
            return false;

        // 참조 동일성 대신 instanceId 기준으로 비교해도 일치로 인정
        if (equippedItem == item) return true;
        return !string.IsNullOrEmpty(equippedItem.instanceId) && equippedItem.instanceId == item.instanceId;
    }

    public bool TryRemoveItem(StoredItem item)
    {
        if (!CanRemoveItem(item)) return false;

        var slot = item.itemData.equiptype;
        equipped.Remove(slot);

        RaiseChanged();
        return true;
    }

    public bool CanAddItemSwap(StoredItem newItem, out StoredItem swappedOut)
    {
        swappedOut = null;
        if (!CanAddItem(newItem)) return false;

        var slot = newItem.itemData.equiptype;

        equipped.TryGetValue(slot, out swappedOut);
        return true;
    }
    public bool TryAddItemSwap(StoredItem newItem, out StoredItem swappedOut)
    {
        swappedOut = null;
        if (!CanAddItem(newItem)) return false;

        var slot = newItem.itemData.equiptype;

        // 슬롯에 이미 뭐가 있으면 그걸 뽑아냄
        equipped.TryGetValue(slot, out swappedOut);

        equipped[slot] = newItem;
        RaiseChanged();
        return true;
    }
    #endregion

    //세이브
    public EquipData SaveData()
    {
        EquipData data = new EquipData();
        data.equippedSlots = new List<EquippedSlotData>();

        // Equipped: IReadOnlyDictionary<EquipType, StoredItem> :contentReference[oaicite:8]{index=8}
        foreach (KeyValuePair<EquipType, StoredItem> kv in this.Equipped)
        {
            EquipType slotType = kv.Key;
            StoredItem item = kv.Value;
            if (item == null || item.itemData == null)
            {
                continue;
            }

            EquippedSlotData ed = new EquippedSlotData();
            ed.slot = slotType.ToString();
            ed.item = item.SaveData(); // ★ 핵심
            data.equippedSlots.Add(ed);
        }
        return data;
    }


    public void LoadData(EquipData data)
    {
        if (data == null)
        {
            RestoreExact(null);
            return;
        }
        RestoreExact(data.equippedSlots);
    }

    //로드
    public void RestoreExact(List<EquippedSlotData> equippedSlots)
    {
        equipped.Clear(); 

        if (equippedSlots != null)
        {
            for (int i = 0; i < equippedSlots.Count; i++)
            {
                EquippedSlotData es = equippedSlots[i];
                if (es == null || es.item == null) continue;

                EquipType slotEnum;
                bool ok = Enum.TryParse<EquipType>(es.slot, out slotEnum);
                if (!ok) continue;

                StoredItem rebuilt = StoredItem.RestoreFromData(es.item);
                if (rebuilt == null) continue;

                equipped[slotEnum] = rebuilt;
            }
        }

        RaiseChanged();
    }
}
