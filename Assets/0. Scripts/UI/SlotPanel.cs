using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SlotPanel : MonoBehaviour
{
    [SerializeField] InventoryUI inventoryUI;
    [SerializeField] Container container;
    [SerializeField] GameObject slotPrefab;

    ItemType categoryFilter;

    List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    private void Awake()
    {
        if (container != null) SetContainer(container);
        container.Changed += Refresh;
    }

    public void SetContainer(Container c)
    {
        container = c;
        for (int i = uiSlots.Count; i < c.maxSlotNum; i++)
        {
            InventorySlotUI child = Instantiate(slotPrefab, transform).GetComponent<InventorySlotUI>();
            child.Initialize(inventoryUI);
            uiSlots.Add(child);
        }
        Refresh();
    }

    private void OnEnable()
    {
        container.Changed += Refresh;
    }

    void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        container.Changed -= Refresh;
    }

    public void ChangeCategory(int index)
    {
        categoryFilter = (ItemType)index;
        Refresh();
    }

    void Refresh()
    {
        if (container == null) return;

        int uiIndex = 0;
        foreach (StoredItem slot in container.slots)
        {
            if (categoryFilter == ItemType.All || categoryFilter == slot.itemdata.type)
            {
                if (uiIndex < uiSlots.Count)
                    uiSlots[uiIndex].Bind(slot);
                uiIndex++;
            }
        }

        // 남은 슬롯 Clear
        for (int i = uiIndex; i < uiSlots.Count; i++)
        {
            uiSlots[i].Clear();
        }
    }
}