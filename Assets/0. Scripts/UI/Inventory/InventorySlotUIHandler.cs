using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InventorySlotUI))]
public class InventorySlotUIHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    InventorySlotUI mySlot;
    public event Action<InventorySlotUI> PointerEnter;
    public event Action<InventorySlotUI> PointerExit;
    public event Action<InventorySlotUI, PointerEventData> BeginDragSlot;
    public event Action<InventorySlotUI, PointerEventData> DragSlot;
    public event Action<InventorySlotUI, PointerEventData> EndDragSlot;

    void Awake()
    {
        mySlot = gameObject.GetComponent<InventorySlotUI>();
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        PointerEnter?.Invoke(mySlot);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        PointerExit?.Invoke(mySlot);
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        mySlot.Invisible();
        BeginDragSlot?.Invoke(mySlot, eventData);
    }
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        DragSlot?.Invoke(mySlot, eventData);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        mySlot.Visible();
        EndDragSlot?.Invoke(mySlot, eventData);
    }
}
