using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] TooltipUI tooltipUI;

    RectTransform uiRect;
    Canvas canvas;
    Camera cam;

    void Start()
    {
        uiRect = (RectTransform)transform;
        canvas = GetComponentInParent<Canvas>();
        cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

        HideTooltip();
    }
    public void ShowTooltip(StoredItem item, RectTransform slotRect)
    {
        //툴팁 위치를 슬롯 옆에 붙도록 변경

        Vector3[] corners = new Vector3[4];
        slotRect.GetWorldCorners(corners);
        Vector3 worldTopRight = corners[2];

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiRect,
            RectTransformUtility.WorldToScreenPoint(cam, worldTopRight),
            cam,
            out Vector2 localPos
        );

        RectTransform ttRect = (RectTransform)tooltipUI.transform;
        ttRect.anchoredPosition = localPos;

        // 내용 세팅 및 표시
        tooltipUI.Set(item);
        tooltipUI.gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        tooltipUI.gameObject.SetActive(false);
    }

}
