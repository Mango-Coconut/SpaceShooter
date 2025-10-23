using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(InventorySlotUIHandler))]
public class InventorySlotUI : MonoBehaviour
{
    [HideInInspector] public InventorySlotUIHandler handler;

    StoredItem enterItem;
    public StoredItem EnterItem => enterItem;
  
    //PanelManeger가 Tooltip 위치 변경시 사용
    RectTransform rect;
    public RectTransform Rect => rect;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        handler = GetComponent<InventorySlotUIHandler>();
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

        handler.enabled = true;

        enterItem = item;

        itemImage.sprite = item.itemdata.icon;
        itemImage.enabled = true;

        if (itemAmount == null || item.count == 1) return;
        itemAmount.text = item.count.ToString();
        itemAmount.enabled = true;
    }

    public void Clear()
    {
        handler.enabled = false;

        enterItem = null;

        Invisible();
    }
    public void Invisible()
    {
        itemImage.enabled = false;

        if (itemAmount == null) return;
        itemAmount.enabled = false;
    }
    public void Visible()
    {
        itemImage.enabled = false;

        if (itemAmount == null) return;
        itemAmount.enabled = false;
    }
}