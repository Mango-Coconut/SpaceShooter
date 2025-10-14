using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(InventorySlotUIHandler))]
public class InventorySlotUI : MonoBehaviour
{
    public InventorySlotUIHandler handler;
    [SerializeField] Image frame;
    [SerializeField] Image itemImage;

    [SerializeField] TMP_Text itemAmount;

    RectTransform rect;
    public RectTransform Rect => rect;

    StoredItem enterItem;
    public StoredItem EnterItem => enterItem;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        handler = GetComponent<InventorySlotUIHandler>();
    }
    public void Initialize()
    {
        Clear();
    }

    public void Bind(StoredItem item)
    {
        enterItem = item;

        itemImage.sprite = item.itemdata.icon;
        itemAmount.text = item.count.ToString();

        itemAmount.enabled = true;
        itemImage.enabled = true;
    }

    public void Clear()
    {
        enterItem = null;
        itemImage.enabled = false;
        itemAmount.enabled = false;
    }
    public void Invisible()
    {
        itemImage.enabled = false;
        itemAmount.enabled = false;
    }
        public void Visible()
    {
        itemImage.enabled = false;
        itemAmount.enabled = false;
    }
}