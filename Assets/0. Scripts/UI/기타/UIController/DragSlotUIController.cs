using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragSlotUIController : MonoBehaviour
{
    [SerializeField] DragSlot dragSlot;

    public void Show(StoredItem item)
    {
        dragSlot.gameObject.SetActive(true);
        dragSlot.Bind(item);
    }
    public void Move(ItemUIEventArgs args)
    {
        if (args.Pointer != null)
        {
            dragSlot.transform.position = args.Pointer.position;
        }
    }
    public void Hide()
    {
        dragSlot.gameObject.SetActive(false);
    }
}
