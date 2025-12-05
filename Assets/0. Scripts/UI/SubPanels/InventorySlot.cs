using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SlotPointerHandler))]
[RequireComponent(typeof(SlotClickHandler))]
[RequireComponent(typeof(SlotDragHandler))]
public class InventorySlot : MonoBehaviour, ISlotUI
{    
    #region UI Child Components
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Image itemImage;
    [SerializeField] TMP_Text itemAmount;
    
    #endregion 


    #region ISlotUI
    public SlotPointerHandler PointerHandler { get; private set; }
    public SlotClickHandler ClickHandler { get; private set; }
    public SlotDragHandler DragHandler { get; private set; }
    public RectTransform Rect { get; private set; }
    public GameObject GO => gameObject;
    #endregion

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
        if (item == null || item.itemData == null)
        {
            Invisible();
            return;
        }

        enterItem = item;

        Visible();

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
        canvasGroup.alpha = 0f;           
        canvasGroup.interactable = false; 
        canvasGroup.blocksRaycasts = false;
    }
    public void Visible()
    {
        canvasGroup.alpha = 1f; 
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true; 
    }
}