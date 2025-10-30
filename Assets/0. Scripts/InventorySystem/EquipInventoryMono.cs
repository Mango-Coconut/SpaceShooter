
using System;
using UnityEngine;

public class EquipInventoryMono : MonoBehaviour
{
    public EquipInventoryCore Core { get; private set; }

    public event Action OnChanged;

    void Awake()
    {
        Core = new EquipInventoryCore();
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
        if (Core == null) return;
        Core.OnChanged -= HandleCoreChanged;
    }
    #endregion

    private void HandleCoreChanged()
    {
        OnChanged?.Invoke();
    }

    public bool TryGetEquipped(EquipType slot, out StoredItem item)
    {
        item = null;

        if (Core == null) return false;

        return Core.TryGetEquipped(slot, out item);
    }

    public StoredItem GetEquipped(EquipType slot)
    {
        if (Core == null) return null; 
        StoredItem item;
        Core.TryGetEquipped(slot, out item);
        return item;
    }
}
