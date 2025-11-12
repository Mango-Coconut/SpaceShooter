using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlotUIHandler : MonoBehaviour,
IPointerEnterHandler,
IPointerExitHandler
{
    public Func<StoredItem> GetItem;
    public Func<RectTransform> GetRect;


    #region 이벤트 발행
    public event Action<StoredItem, RectTransform> PointerEntered;
    public event Action PointerExited;


    // 툴팁 띄우기
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        StoredItem item = GetItem();
        if (item == null || item.itemData == null) return;

        RectTransform rect = GetRect();
        if (rect == null) return;

        PointerEntered?.Invoke(item, rect);
    }

    // 툴팁 숨기기
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        PointerExited?.Invoke();
    }
    #endregion
}
