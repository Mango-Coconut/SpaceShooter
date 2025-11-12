using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InventorySlotUIHandler))]
public class InventorySlotUI : MonoBehaviour
{
    [HideInInspector] public InventorySlotUIHandler pointerHandler;
    [HideInInspector] public SlotRClickHandler clickHandler;
    [HideInInspector] public SlotDragHandler dragHandler;

    StoredItem enterItem;
    public StoredItem EnterItem => enterItem;

    //PanelManeger가 Tooltip 위치 변경시 사용
    RectTransform rect;
    public RectTransform Rect => rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();

        //PointerHandler 초기화
        pointerHandler = GetComponent<InventorySlotUIHandler>();
        pointerHandler.GetItem = () => enterItem;
        pointerHandler.GetRect = () => rect;

        //ClickHandler 초기화
        clickHandler = GetComponent<SlotRClickHandler>();
        clickHandler.GetItem = () => enterItem;

        //DragHandler 초기화
        dragHandler = GetComponent<SlotDragHandler>();
        dragHandler.GetItem = () => enterItem;
        // 드래그 중에 슬롯 아이콘 숨김
        dragHandler.SetGhostInvisible = Invisible;
        dragHandler.SetGhostVisible = Visible;
    }
    public void Initialize()
    {
        Clear();
    }

    [SerializeField] Image itemImage;
    [SerializeField] TMP_Text itemAmount;
    public void Bind(StoredItem item)
    {
        if (item == null)
        {
            Clear();
            return;
        }

        enterItem = item;

        itemImage.sprite = item.itemData.icon;
        itemImage.enabled = true;

        if (itemAmount == null) return; //장비창은 항상 1개니까 TMP itemAmount 없음
        if (item.count == 1)
        {
            itemAmount.enabled = false;
            return;
        }
        itemAmount.text = item.count.ToString();
        itemAmount.enabled = true;
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
}