using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SlotPointerHandler))]
public class ShopSlot : MonoBehaviour, ISlotUI
{
    #region UI Child Components
    //아이템 이미지와 이미지 우측아래 개수
    [SerializeField] Image itemImage;
    [SerializeField] TMP_Text itemImageCount;

    //슬라이더와 거기 붙은 숫자
    [SerializeField] Slider slider;
    [SerializeField] TMP_Text sliderHandleCountText;
    [SerializeField] TMP_Text sliderMaxCountText;
    // [SerializeField] TMP_Text sliderMinCountText; 는 항상 0

    [SerializeField] TMP_Text totalCoin;
    #endregion

    #region ISlotUI
    public SlotPointerHandler PointerHandler { get; private set; }
    public SlotClickHandler ClickHandler => null; // 상점은 안 씀
    public SlotDragHandler DragHandler => null; // 상점은 안 씀
    public RectTransform Rect { get; private set; }
    public GameObject GO => gameObject;
    #endregion

    CanvasGroup canvasGroup;
    StoredItem enterItem;

    public event Action<StoredItem> OnCartedItem;

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
        PointerHandler = GetComponent<SlotPointerHandler>();
        PointerHandler.GetItem = () => enterItem;
        PointerHandler.GetRect = () => Rect;
        Rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
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
    }


    public void Clear()
    {
        enterItem = null;
        Invisible();
    }

    public void Refresh()
    {
        if (enterItem == null || enterItem.itemData == null) return;

        sliderHandleCountText.SetText($"{slider.value}");
        int totalValue = (int)(enterItem.itemData.price * slider.value);
        totalCoin.SetText($"{totalValue}");

        if (enterItem.IsUniqueInstance())
        {
            OnCartedItem?.Invoke(enterItem);
        }
        else
        {
            OnCartedItem?.Invoke(new StoredItem(enterItem.itemData, (int)slider.value));
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
}