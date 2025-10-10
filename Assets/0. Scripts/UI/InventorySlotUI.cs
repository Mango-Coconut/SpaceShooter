using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UIElements;
public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    InventoryUI inventoryUI;
    [SerializeField] UnityEngine.UI.Image frame;
    [SerializeField] UnityEngine.UI.Image itemImage;

    [SerializeField] TMP_Text itemAmount;

    RectTransform slotRect;

    StoredItem enterItem;

    void Awake()
    {
        raycaster = GetComponentInParent<Canvas>().GetComponent<GraphicRaycaster>();
        slotRect = GetComponent<RectTransform>();
    }
    public void Initialize(InventoryUI parent)
    {
        inventoryUI = parent;
        enterItem = null;
        Clear();
    }

    public void Bind(StoredItem item)
    {
        enterItem = item;

        itemImage.sprite = item.itemdata.icon;
        itemAmount.text = item.count.ToString();

        itemAmount.enabled = true;
        itemImage.enabled = true;
    }

    public void Clear()
    {
        enterItem = null;
        itemImage.enabled = false;
        itemAmount.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (enterItem == null) return;
        inventoryUI.ShowTooltip(enterItem, slotRect);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (enterItem == null) return;
        inventoryUI.HideTooltip();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (enterItem == null) return;
        inventoryUI.BeginDragSlot(enterItem);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (enterItem == null) return;
        inventoryUI.DragDragSlot(eventData.position);
    }

    GraphicRaycaster raycaster;
    public void OnEndDrag(PointerEventData eventData)
    {
        if (enterItem == null) return;
        

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(eventData, results);
        string tag = "";

        foreach (RaycastResult result in results)
        {
            if (result.gameObject.CompareTag("InventoryUI"))
            {
                tag = "InventoryUI";
                return;
            }
            if (result.gameObject.CompareTag("ChestUI"))
            {
                tag = "ChestUI";
                return;
            }
        }
        
        inventoryUI.EndDragSlot(tag);
    }
}