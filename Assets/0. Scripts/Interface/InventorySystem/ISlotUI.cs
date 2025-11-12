using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISlotUI
{
    // 핸들러 (없으면 null 리턴)
    SlotPointerHandler PointerHandler { get; }
    SlotClickHandler ClickHandler { get; }
    SlotDragHandler DragHandler { get; }

    RectTransform Rect { get; }
    
    GameObject GO { get; }   // ← 추가

    // 공통 바인딩: 코인은 상점 슬롯만 사용, 인벤토리는 무시
    void Bind(StoredItem item, int? playerCoin = null);

    public void Clear();
}
