using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SlotPointerHandler))]
[RequireComponent(typeof(SlotClickHandler))]
[RequireComponent(typeof(SlotDragHandler))]
public class InventorySlotUI : MonoBehaviour, ISlotUI
{    
    [SerializeField] Image itemImage;
    [SerializeField] TMP_Text itemAmount;

    public SlotPointerHandler PointerHandler { get; private set; }
    public SlotClickHandler ClickHandler { get; private set; }
    public SlotDragHandler DragHandler { get; private set; }
    public RectTransform Rect { get; private set; }
    public GameObject GO => gameObject;

    StoredItem enterItem;
    public StoredItem EnterItem => enterItem;


    void Awake()
    {
        Rect = GetComponent<RectTransform>();
        PointerHandler = GetComponent<SlotPointerHandler>();
        ClickHandler = GetComponent<SlotClickHandler>();
        DragHandler = GetComponent<SlotDragHandler>();

        if (PointerHandler != null)
        {
            PointerHandler.GetItem = () => enterItem;
            PointerHandler.GetRect = () => Rect;
        }

        if (ClickHandler != null)
        {
            ClickHandler.GetItem = () => enterItem;
        }

        if (DragHandler != null)
        {
            DragHandler.GetItem = () => enterItem;
            DragHandler.SetGhostInvisible = Invisible;
            DragHandler.SetGhostVisible = Visible;
        }

        Clear();
    }

    public void Initialize()
    {
        Clear();
    }

    public void Bind(StoredItem item)
    {
        enterItem = item;
        if (item == null || item.itemData == null)
        {
            Clear();
            return;
        }

        itemImage.enabled = true;
        itemImage.sprite = item.itemData.icon;

        if (itemAmount != null)
        {
            if (item.count > 1) { itemAmount.enabled = true; itemAmount.text = item.count.ToString(); }
            else { itemAmount.enabled = false; }
        }
    }

    public void Clear()
    {
        enterItem = null;
        Invisible();
    }
    public void Invisible()
    {
        if (itemImage != null) itemImage.enabled = false;
        if (itemAmount != null) itemAmount.enabled = false;
    }
    public void Visible()
    {
        if (enterItem == null) return;
        if (itemImage != null) itemImage.enabled = true;
        if (itemAmount != null && enterItem.count > 1) itemAmount.enabled = true;
    }

    public void Bind(StoredItem item, int? playerCoin = null)
    {
        throw new System.NotImplementedException();
    }
}