using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DragSlot : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Image frame;
    [SerializeField] UnityEngine.UI.Image itemImage;
    [SerializeField] TMP_Text itemAmount;

    public void Bind(StoredItem item)
    {
        itemImage.sprite = item.itemdata.icon;
        itemAmount.text = item.count.ToString();
    }
}
