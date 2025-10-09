using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    SlotPanel host;
    [SerializeField] Image frame;
    [SerializeField] Image itemImage;

    [SerializeField] TMP_Text itemAmount;

    RectTransform slotRect;

    StoredItem enterItem;

    void Awake()
    {
        slotRect = GetComponent<RectTransform>();
    }
    public void Initialize(SlotPanel parent)
    {
        host = parent;
        enterItem = null;
        Clear();
    }

    public void Bind(StoredItem item)
    {
        enterItem = item;

        itemImage.sprite = item.itemdata.icon;
        itemAmount.text = item.count.ToString();

        itemAmount.enabled = true;
        itemImage.enabled = true;
    }

    public void Clear()
    {
        enterItem = null;
        itemImage.enabled = false;
        itemAmount.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (enterItem == null) return;
        host.ShowTooltip(enterItem, slotRect);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (enterItem == null) return;
        host.HideTooltip();
    }
}