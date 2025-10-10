using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DragSlot : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Image frame;
    [SerializeField] UnityEngine.UI.Image itemImage;

    [SerializeField] TMP_Text itemAmount;

    StoredItem item;

    public void Bind(StoredItem item)
    {
        this.item = item;

        itemImage.sprite = item.itemdata.icon;
        itemAmount.text = item.count.ToString();
    }

    public void Clear()
    {
        itemImage = null;
        itemAmount = null;
    }

}
