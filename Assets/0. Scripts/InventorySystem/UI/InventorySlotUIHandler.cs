using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(InventorySlotUI))]
public class InventorySlotUIHandler : MonoBehaviour,
IPointerEnterHandler,
IPointerExitHandler,
IPointerClickHandler,
IBeginDragHandler,
IDragHandler,
IEndDragHandler
{
    InventorySlotUI mySlot;

    void Awake()
    {
        mySlot = gameObject.GetComponent<InventorySlotUI>();
    }

    #region 이벤트 발행
    public event Action<InventorySlotUI> PointerEnter;
    public event Action PointerExit;
    public event Action<InventorySlotUI> LeftClick;
    public event Action<StoredItem> RightClick;
    public event Action<StoredItem, PointerEventData> BeginDrag;
    public event Action<PointerEventData> Dragging;
    public event Action<StoredItem, PointerEventData> EndDrag;


    // 툴팁 띄우기
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        PointerEnter?.Invoke(mySlot);
    }

    // 툴팁 숨기기
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        PointerExit?.Invoke();
    }

    // 아이템 사용, 장착(우클릭)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            LeftClick?.Invoke(mySlot);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            Log.Info($"SlotUIHandier -> rightClick{mySlot.EnterItem.itemdata.name}");
            if (RightClick != null)
            {
                foreach (var d in RightClick.GetInvocationList())
                    Debug.Log($"[RightClick] {d.Target} -> {d.Method}");
            }

            RightClick?.Invoke(mySlot.EnterItem);

        }
    }

    //아이템 옮기기 1
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        mySlot.Invisible();
        BeginDrag?.Invoke(mySlot.EnterItem, eventData);
    }

    //아이템 옮기기 2
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        Dragging?.Invoke(eventData);
    }

    //아이템 옮기기 3
    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        mySlot.Visible();
        EndDrag?.Invoke(mySlot.EnterItem, eventData);
    }
    #endregion
}
