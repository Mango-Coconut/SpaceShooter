using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    [SerializeField] TMP_Text itemName;
    [SerializeField] Image itemImage;
    [SerializeField] TMP_Text itemDescript;

    void Awake()
    {
        ((RectTransform)transform).pivot = new Vector2(0f, 1f);
    }
    public void Set(StoredItem item)
    {
        itemName.text = item.itemData.name;
        itemImage.sprite = item.itemData.icon;
        itemDescript.text = item.itemData.description;
    }
}
