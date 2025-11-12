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
    [SerializeField] Button buyButton;
    #endregion

    public SlotPointerHandler PointerHandler { get; private set; }
    public SlotClickHandler ClickHandler => null; // 상점은 안 씀
    public SlotDragHandler DragHandler => null; // 상점은 안 씀
    public RectTransform Rect { get; private set; }
    public GameObject GO => gameObject;

    StoredItem enterItem;
    int playerCoin;

    void Awake()
    {
        Rect = GetComponent<RectTransform>();
        PointerHandler = GetComponent<SlotPointerHandler>();
        PointerHandler.GetItem = () => enterItem;
        PointerHandler.GetRect = () => Rect;
        Rect = GetComponent<RectTransform>();
    }

    public void Bind(StoredItem item, int? playerCoin = null)
    {
        if (item == null || item.itemData == null) { Clear(); return; }

        enterItem = item;
        this.playerCoin = playerCoin ?? this.playerCoin;

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
    }

    public void Refresh()
    {
        if (enterItem == null || enterItem.itemData == null) return;

        // slider Handle을 따라다니는 숫자
        // if (slider.value == 0 || slider.value == enterItem.count)
        // {
        //     sliderHandleCountText.enabled = false;
        // }
        // else
        // {
        //     sliderHandleCountText.enabled =       true;
        //     sliderHandleCountText.text = slider.value.ToString();
        // }
        sliderHandleCountText.text = slider.value.ToString();

        int totalValue = (int)(enterItem.itemData.price * slider.value);
        totalCoin.text = totalValue.ToString();

        // 구매 가능 여부
        bool canBuy = (enterItem.count > 0) && (playerCoin >= totalValue);
        buyButton.interactable = canBuy;
    }
}