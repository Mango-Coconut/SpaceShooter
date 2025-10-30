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
        if (item == null) return false;
        var slot = item.itemData.equiptype;
        return equipped.TryGetValue(slot, out var equippedItem) && equippedItem == item;
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

    //로드
    public void RestoreExact(List<EquippedSlotData> equippedSlots)
    {
        // 내부 딕셔너리 초기화(필요 시)
        // private readonly라도 Clear 호출은 가능하도록 선언되어 있음
        // 여기서는 안전하게 Clear
        // (원본 소스에서 equipped 접근 범위에 맞춰 같은 파일 내에서 처리)
        System.Reflection.FieldInfo fi = typeof(EquipInventoryCore).GetField("equipped", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Dictionary<EquipType, StoredItem> dict = (Dictionary<EquipType, StoredItem>)fi.GetValue(this);
        dict.Clear();

        if (equippedSlots != null)
        {
            for (int i = 0; i < equippedSlots.Count; i++)
            {
                EquippedSlotData es = equippedSlots[i];
                if (es == null || es.item == null)
                {
                    continue;
                }

                EquipType slotEnum;
                bool ok = Enum.TryParse<EquipType>(es.slot, out slotEnum);
                if (!ok)
                {
                    continue;
                }

                StoredItem rebuilt = StoredItem.RestoreFromData(es.item);
                if (rebuilt == null)
                {
                    continue;
                }

                dict[slotEnum] = rebuilt;
            }
        }

        OnChanged?.Invoke();
    }
}
