using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SlotPanel : MonoBehaviour
{
    InventoryUI inventoryUI;
    [SerializeField] Inventory inventory;
    [SerializeField] GameObject slotPrefab;

    ItemType categoryFilter;

    List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    private void Awake()
    {
        inventoryUI = GetComponentInParent<InventoryUI>();
        for (int i = 0; i < inventory.maxSlotNum; i++)
        {
            InventorySlotUI child = Instantiate(slotPrefab, transform).GetComponent<InventorySlotUI>();
            child.Initialize(this);
            uiSlots.Add(child);
        }
    }

    private void OnEnable()
    {
        inventory.OnInventoryChanged += Refresh;
    }

    void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        inventory.OnInventoryChanged -= Refresh;
    }

    public void ChangeCategory(int index)
    {
        categoryFilter = (ItemType)index;
        Refresh();
    }

    void Refresh()
    {
        int uiIndex = 0;
        foreach (StoredItem slot in inventory.slots)
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

    public void ShowTooltip(StoredItem item, RectTransform slotRect)
    {
        inventoryUI.ShowTooltip(item, slotRect);
    }
    public void HideTooltip()
    {
        inventoryUI.HideTooltip();
    }
}