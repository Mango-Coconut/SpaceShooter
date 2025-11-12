using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour
{
    InventorySlotUI Core;
    //아이템 이미지와 이미지 우측아래 개수
    Image itemImage;
    TMP_Text itemImageCount;

    //슬라이더와 거기 붙은 숫자
    Slider slider;
    TMP_Text sliderMaxCountText;
    // TMP_Text sliderMinCountText; 는 항상 0

    //
    TMP_Text totalCoin;

    Button buyButton;
}
