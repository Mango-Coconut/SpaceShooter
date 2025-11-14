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
    public void Move(SlotPanelEventArgs args)
    {
        if (args.Pointer != null)
        {
            Log.Info($"{args.Pointer.position}");
            dragSlot.transform.position = args.Pointer.position;
        }
    }
    public void Hide()
    {
        dragSlot.gameObject.SetActive(false);
    }
}
