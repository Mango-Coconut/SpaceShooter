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


    protected virtual void Awake()
    {
        Core = new InventoryCore(capacity);
    }
    
    public bool TryAddItem(StoredItem item) => Core.TryAddItem(item);
    public bool TryRemoveItem(StoredItem item) => Core.TryRemoveItem(item);
}