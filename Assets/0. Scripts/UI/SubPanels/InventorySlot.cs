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
    public UIPointerHandler<StoredItem> PointerHandler { get; private set; }
    public UIClickHandler ClickHandler { get; private set; } // TODO 제네릭화 하기
    public UIDragHandler DragHandler { get; private set; } // TODO 제네릭화 하기
    public RectTransform Rect { get; private set; }
    public GameObject GO => gameObject;
    #endregion

    StoredItem enterItem;
    public StoredItem EnterItem => enterItem;

    UIPointerHandler IUIInteraction.PointerHandler => PointerHandler;

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
        PointerHandler = GetComponent<UIPointerHandler<StoredItem>>();
        ClickHandler = GetComponent<UIClickHandler>();
        DragHandler = GetComponent<UIDragHandler>();

        if (PointerHandler != null)
        {
            PointerHandler.GetData = () => enterItem;
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