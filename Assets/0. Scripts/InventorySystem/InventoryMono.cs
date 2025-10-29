using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMono : MonoBehaviour
{
    [SerializeField] int capacity = 20;


    public InventoryCore Core { get; private set; }
    public int Capacity => Core.Capacity;
    public IReadOnlyList<StoredItem> Slots => Core.Slots;


    public event Action OnChanged;

    protected virtual void Awake()
    {
        Core = new InventoryCore(capacity);
    }

    void OnEnable()
    {
        Subscribe();
    }

    void OnDisable()
    {
        UnSubscribe();
    }

    #region Core 이벤트 구독
    void Subscribe()
    {
        UnSubscribe();
        Core.OnChanged += HandleCoreChanged;
    }
    void UnSubscribe()
    {
        Core.OnChanged -= HandleCoreChanged;
    }
    #endregion

    void HandleCoreChanged()
    {
        OnChanged?.Invoke();
    }

    public bool TryAddItem(StoredItem item)
    {
        return Core.TryAddItem(item);
    }
    public bool TryAddItem(ItemData data, int amount = 1)
    {
        return Core.TryAddItem(data, amount);
    }


    public bool TryRemoveItem(StoredItem item)
    {
        return Core.TryRemoveItem(item);
    }
    public bool TryRemoveItem(ItemData data, int amount = 1)
    {
        return Core.TryRemoveItem(data, amount);
    }

}