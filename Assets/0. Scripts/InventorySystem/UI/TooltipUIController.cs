using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipUIController : MonoBehaviour
{
    readonly TooltipUI ui;

    // 필요 시: 화면 밖 클램핑 옵션 등 추가
    public TooltipUIController(TooltipUI tooltipUI)
    {
        ui = tooltipUI;
    }

    public void Show(StoredItem item, RectTransform anchor)
    {
        if (ui == null) return;
        if (item == null) { Hide(); return; }
        if (anchor == null) { Hide(); return; }

        ui.Show(item, anchor);
    }

    public void Hide()
    {
        if (ui == null) return;
        ui.Hide();
    }

    /// <summary>
    /// 마우스 이동에 따라 툴팁 위치를 옮겨야 할 때 사용.
    /// TooltipUI 내부가 screenPos를 받는 형태가 아니라면 필요에 맞게 고쳐서 사용.
    /// </summary>
    public void MoveTo(Vector2 screenPos)
    {
        if (ui == null) return;
        ui.MoveTo(screenPos);
    }
}
