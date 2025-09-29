using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    [SerializeField] TMP_Text itemName;
    [SerializeField] Image itemImage;
    [SerializeField] TMP_Text itemDescript;
    public void Set(StoredItem item)
    {
        itemName.text = item.itemdata.name;
        itemImage.sprite = item.itemdata.icon;
        itemDescript.text = item.itemdata.description;
    }
}
