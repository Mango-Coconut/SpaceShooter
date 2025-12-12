using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IInteractiveView<StoredItem>
{    
    #region UI Child Components
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Image itemImage;
    [SerializeField] TMP_Text itemAmount;
    
    #endregion 


    #region IInteractiveView
    public UIPointerHandler PointerHandler { get; private set; }
    public UIClickHandler ClickHandler { get; private set; }
    public UIDragHandler DragHandler { get; private set; }

    public RectTransform Rect { get; private set; }
    public GameObject GO => gameObject;
    #endregion

    StoredItem enterItem;
    public StoredItem EnterItem => enterItem;


    void Awake()
    {
        Rect = GetComponent<RectTransform>();
        UIPointerHandler<StoredItem> pointerT = GetComponent<UIPointerHandler<StoredItem>>();
        UIClickHandler<StoredItem> clickT = GetComponent<UIClickHandler<StoredItem>>();
        UIDragHandler<StoredItem> dragT = GetComponent<UIDragHandler<StoredItem>>();

        PointerHandler = pointerT;
        ClickHandler = clickT;
        DragHandler = dragT;

        if (pointerT != null)
        {
            pointerT.GetData = () => enterItem;
            pointerT.GetRect = () => Rect;
        }

        if (clickT != null)
        {
            clickT.GetData = () => enterItem;
        }

        if (dragT != null)
        {
            dragT.GetData = () => enterItem;
            dragT.SetGhostInvisible = Invisible;
            dragT.SetGhostVisible = Visible;
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
            Clear();
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