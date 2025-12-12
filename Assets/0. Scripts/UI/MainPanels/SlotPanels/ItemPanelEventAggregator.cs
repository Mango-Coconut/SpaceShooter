using System;
using UnityEngine;

public class ItemPanelEventAggregator : PanelEventAggregator<StoredItem>
{
    [Header("Source Info")]
    [SerializeField] StorageTarget source;

    public StorageTarget Source => source;

    public event Action<SlotPanelEventArgs> ItemMouseEntered;
    public event Action<SlotPanelEventArgs> ItemMouseExited;
    public event Action<SlotPanelEventArgs> ItemLeftClicked;
    public event Action<SlotPanelEventArgs> ItemRightClicked;
    public event Action<SlotPanelEventArgs> ItemDragBegan;
    public event Action<SlotPanelEventArgs> ItemDragging;
    public event Action<SlotPanelEventArgs> ItemDragEnded;

    protected override void OnEnable()
    {
        base.OnEnable();

        MouseEntered += HandleMouseEntered;
        MouseExited += HandleMouseExited;
        LeftClicked += HandleLeftClicked;
        RightClicked += HandleRightClicked;
        DragBegan += HandleDragBegan;
        Dragging += HandleDragging;
        DragEnded += HandleDragEnded;
    }

    protected override void OnDisable()
    {
        MouseEntered -= HandleMouseEntered;
        MouseExited -= HandleMouseExited;
        LeftClicked -= HandleLeftClicked;
        RightClicked -= HandleRightClicked;
        DragBegan -= HandleDragBegan;
        Dragging -= HandleDragging;
        DragEnded -= HandleDragEnded;

        base.OnDisable();
    }

    void HandleMouseEntered(SlotEventArgs<StoredItem> e)
    {
        if (ItemMouseEntered == null)
        {
            return;
        }

        SlotPanelEventArgs args = new SlotPanelEventArgs(e, source);
        ItemMouseEntered.Invoke(args);
    }

    void HandleMouseExited(SlotEventArgs<StoredItem> e)
    {
        if (ItemMouseExited == null)
        {
            return;
        }

        SlotPanelEventArgs args = new SlotPanelEventArgs(e, source);
        ItemMouseExited.Invoke(args);
    }

    void HandleLeftClicked(SlotEventArgs<StoredItem> e)
    {
        if (ItemLeftClicked == null)
        {
            return;
        }

        SlotPanelEventArgs args = new SlotPanelEventArgs(e, source);
        ItemLeftClicked.Invoke(args);
    }

    void HandleRightClicked(SlotEventArgs<StoredItem> e)
    {
        if (ItemRightClicked == null)
        {
            return;
        }

        SlotPanelEventArgs args = new SlotPanelEventArgs(e, source);
        ItemRightClicked.Invoke(args);
    }

    void HandleDragBegan(SlotEventArgs<StoredItem> e)
    {
        if (ItemDragBegan == null)
        {
            return;
        }

        SlotPanelEventArgs args = new SlotPanelEventArgs(e, source);
        ItemDragBegan.Invoke(args);
    }

    void HandleDragging(SlotEventArgs<StoredItem> e)
    {
        if (ItemDragging == null)
        {
            return;
        }

        SlotPanelEventArgs args = new SlotPanelEventArgs(e, source);
        ItemDragging.Invoke(args);
    }

    void HandleDragEnded(SlotEventArgs<StoredItem> e)
    {
        if (ItemDragEnded == null)
        {
            return;
        }

        SlotPanelEventArgs args = new SlotPanelEventArgs(e, source);
        ItemDragEnded.Invoke(args);
    }
}
