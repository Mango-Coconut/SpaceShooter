using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SlotPanel : MonoBehaviour
{
    [SerializeField] Inventory inventory;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] int slotCount = 100;
    [SerializeField] RectTransform tooltip;  // Tooltip 루트(RectTransform)
    [SerializeField] TooltipUI tooltipUI;

    ItemType categoryFilter;

    List<InventorySlotUI> uiSlots = new List<InventorySlotUI>();

    private void Awake()
    {
        for (int i = 0; i < slotCount; i++)
        {
            InventorySlotUI child = Instantiate(slotPrefab, transform).GetComponent<InventorySlotUI>();
            child.Initialize(this);
            uiSlots.Add(child);
        }
    }

    private void OnEnable()
    {
        inventory.OnInventoryChanged += Refresh;
    }

    void Start()
    {
        Refresh();
        HideTooltip();
    }

    private void OnDisable() {
        inventory.OnInventoryChanged -= Refresh;
    }

    public void ChangeCategory(int index)
    {
        categoryFilter = (ItemType)index;
        Refresh();
    }

    void Refresh()
    {
        int uiIndex = 0;
        foreach (StoredItem slot in inventory.slots)
        {
            if (categoryFilter == ItemType.All || slot.itemdata.type == categoryFilter)
            {
                if (uiIndex < uiSlots.Count)
                    uiSlots[uiIndex].Bind(slot);
                uiIndex++;
            }
        }

        // 남은 슬롯 Clear
        for (int i = uiIndex; i < uiSlots.Count; i++)
        {
            uiSlots[i].Clear();
        }
    }

    public void ShowTooltip(StoredItem item, RectTransform slotRect)
    {
        //툴팁 위치 정하기
        // 슬롯 우상단 월드 좌표
        Vector3[] corners = new Vector3[4];
        slotRect.GetWorldCorners(corners);
        Vector3 worldTopRight = corners[2]; // 0BL, 1TL, 2TR, 3BR

        // 툴팁 부모 기준 좌표로 변환
        RectTransform parent = (RectTransform)tooltipUI.transform.parent;
        Canvas canvas = parent.GetComponentInParent<Canvas>();
        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            RectTransformUtility.WorldToScreenPoint(cam, worldTopRight),
            cam,
            out Vector2 localPos
        );

        // 툴팁 좌상단을 슬롯 우상단에 붙이기
        RectTransform ttRect = tooltipUI.GetComponent<RectTransform>();
        ttRect.pivot = new Vector2(0f, 1f);
        ttRect.anchoredPosition = localPos;


        tooltipUI.Set(item);

        tooltipUI.gameObject.SetActive(true);
    }
    public void HideTooltip()
    {
        tooltipUI.gameObject.SetActive(false);
    }
}