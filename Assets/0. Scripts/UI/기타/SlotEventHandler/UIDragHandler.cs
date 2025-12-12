using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragHandler : MonoBehaviour,
IBeginDragHandler,
IDragHandler,
IEndDragHandler
{
    public Func<StoredItem> GetItem;
    
    public event Action<StoredItem, PointerEventData> DragBegan;
    public event Action<PointerEventData> Dragging;
    public event Action<StoredItem, PointerEventData> DragEnded;

    // InventorySlot에서 드래그 시작시 해당슬롯 숨기기
    public Action SetGhostInvisible;
    // InventorySlot에서 드래그 종료시 해당슬롯 보이기
    public Action SetGhostVisible;

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        StoredItem item = GetItem();
        if (item == null || item.itemData == null) return;

        SetGhostInvisible?.Invoke();
        DragBegan?.Invoke(item, eventData);
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        Dragging?.Invoke(eventData);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        StoredItem item = GetItem();
        if (item == null || item.itemData == null)
        {
            SetGhostVisible?.Invoke(); return;
        }

        SetGhostVisible?.Invoke();
        DragEnded?.Invoke(item, eventData); ;
    }
}
