using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipUIController : MonoBehaviour
{
    [SerializeField] TooltipUI tooltipUI;

    void Awake()
    {
        if(tooltipUI == null) { Log.Warn("TooltipUIController에 tooltipUI 넣기"); }
        tooltipUI.gameObject.SetActive(true);
    }
    void Start()
    {
        tooltipUI.gameObject.SetActive(false);
    }

    public void Show(SlotPanelEventArgs args)
    {
        RectTransform slotRect = args.Slot.Rect;

        Vector3[] corners = new Vector3[4];
        slotRect.GetWorldCorners(corners);
        Vector3 worldTopRight = corners[2];

        Vector2 ttPos = RectTransformUtility.WorldToScreenPoint(null, worldTopRight);

        RectTransform ttRect = (RectTransform)tooltipUI.transform;
        ttRect.position = ttPos;

        tooltipUI.gameObject.SetActive(true);
        tooltipUI.Set(args.Item);
    }

    public void Hide()
    {
        tooltipUI.gameObject.SetActive(false);
    }
}
