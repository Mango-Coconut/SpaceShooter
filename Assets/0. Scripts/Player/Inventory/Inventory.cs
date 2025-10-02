using System;
using UnityEngine;

public class Inventory : Container
{
    public event Action OnInventoryChanged;

    void OnEnable()  => Changed += Forward;
    void OnDisable() => Changed -= Forward;
    void Forward()   => OnInventoryChanged?.Invoke();

    public bool HasItem(ItemData data, int amount = 1)
    {
        var slot = slots.Find(s => s.itemdata == data);
        return slot != null && slot.count >= amount;
    }

    public void UseItem(ItemData data)
    {
        var slot = slots.Find(s => s.itemdata == data);
        if (slot == null) return;

        slot.count--;
        slot.useCount++;
        slot.lastUsed = System.DateTime.Now;

        if (slot.count <= 0) slots.Remove(slot);

        RaiseChanged(); // ← 공통 이벤트 호출 → 위에서 포워딩됨
    }
}