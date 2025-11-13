using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SlotPointerHandler))]
public class ShopSlot : MonoBehaviour, ISlotUI
{
    #region UI Child Components
    [SerializeField] CanvasGroup canvasGroup;

    //아이템 이미지와 이미지 우측아래 개수
    [SerializeField] Image itemImage;
    [SerializeField] TMP_Text itemImageCount;

    //슬라이더와 거기 붙은 숫자
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text sliderHandleCountText;
    [SerializeField] TMP_Text sliderMaxCountText;
    // [SerializeField] TMP_Text sliderMinCountText; 는 항상 0

    [SerializeField] TMP_Text totalCoin;
    [SerializeField] Button buyButton;
    [SerializeField] TMP_Text buyButtonText;
    #endregion

    #region ISlotUI
    public SlotPointerHandler PointerHandler { get; private set; }
    public SlotClickHandler ClickHandler => null; // 상점은 안 씀
    public SlotDragHandler DragHandler => null; // 상점은 안 씀
    public RectTransform Rect { get; private set; }
    public GameObject GO => gameObject;
    #endregion


    //구매 이벤트
    public event Action<StoredItem, int> BoughtItem;
    
    StoredItem enterItem;
    int playerCoin;


    void Awake()
    {
        PointerHandler = GetComponent<SlotPointerHandler>();
        PointerHandler.GetItem = () => enterItem;
        PointerHandler.GetRect = () => Rect;
        Rect = GetComponent<RectTransform>();
    }

    public void Bind(StoredItem item)
    {
        if (item == null || item.itemData == null) { Clear(); return; }

        enterItem = item;

        slider.value = 0;
        totalCoin.text = "0";

        itemImage.sprite = enterItem.itemData.icon;
        itemImageCount.text = enterItem.count.ToString();
        slider.maxValue = enterItem.count;
        sliderMaxCountText.text = enterItem.count.ToString();
        Refresh();
    }
    
    public void SetPlayerCoin(int coin)
    {
        playerCoin = coin;
    }


    public void Clear()
    {
        enterItem = null;
        Invisible();
    }

    public void Refresh()
    {
        if (enterItem == null || enterItem.itemData == null) return;

        sliderHandleCountText.enabled = true;
        sliderHandleCountText.SetText($"{slider.value}");

        int totalValue = (int)(enterItem.itemData.price * slider.value);
        totalCoin.SetText($"{totalValue}");

        if(slider.value == 0)
        {
            sliderHandleCountText.enabled = false;
            buyButton.interactable = false;
            buyButtonText.color = Color.black;
        }
        
        if(playerCoin >= totalValue)
        {
            buyButton.interactable = true;
            buyButtonText.color = Color.white;
        }
        else
        {
            buyButton.interactable = false;
            buyButtonText.color = Color.red;
        }
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

    public void BuyButtonClick()
    {
        BoughtItem?.Invoke(enterItem, (int)slider.value);
    }
}