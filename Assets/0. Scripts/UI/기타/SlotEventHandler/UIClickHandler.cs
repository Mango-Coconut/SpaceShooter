using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIClickHandler : MonoBehaviour,
IPointerClickHandler
{
    public Func<StoredItem> GetItem;

    public event Action<StoredItem> LeftClicked;
    public event Action<StoredItem> RightClicked;
    // Start is called before the first frame update

    // 아이템 사용, 장착(우클릭)
    public void OnPointerClick(PointerEventData eventData)
    {
        StoredItem item = GetItem();
        if (item == null || item.itemData == null) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            LeftClicked?.Invoke(item);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            RightClicked?.Invoke(item);
        }

    }
}