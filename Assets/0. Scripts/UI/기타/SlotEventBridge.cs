using System;
using System.Collections.Generic;

public class SlotEventBridge
{
    public event Action<ItemUIEventArgs> MouseEntered;
    public event Action<ItemUIEventArgs> MouseExited;
    public event Action<ItemUIEventArgs> RightClicked;
    public event Action<ItemUIEventArgs> DragBegan;
    public event Action<ItemUIEventArgs> Dragging;
    public event Action<ItemUIEventArgs> DragEnded;

    readonly List<ItemPanelEventAggregator> sources = new List<ItemPanelEventAggregator>();

    public void Subscribe(ItemPanelEventAggregator newSource)
    {
        if (newSource == null) return;
        if (sources.Contains(newSource)) return;

        UnSubscribe(newSource);

        sources.Add(newSource);

        newSource.MouseEntered += HandleMouseEntered;
        newSource.MouseExited += HandleMouseExited;
        newSource.RightClicked += HandleRightClicked;
        newSource.DragBegan += HandleDragBegan;
        newSource.Dragging += HandleDragging;
        newSource.DragEnded += HandleDragEnded;
    }

    public void UnSubscribe(ItemPanelEventAggregator target)
    {
        if (target == null) return;
        if (!sources.Contains(target)) return;

        target.MouseEntered -= HandleMouseEntered;
        target.MouseExited -= HandleMouseExited;
        target.RightClicked -= HandleRightClicked;
        target.DragBegan -= HandleDragBegan;
        target.Dragging -= HandleDragging;
        target.DragEnded -= HandleDragEnded;

        sources.Remove(target);
    }

    public void UnSubscribeAll()
    {
        for (int i = sources.Count - 1; i >= 0; i--)
        {
            UnSubscribe(sources[i]);
        }
    }
    
    void HandleMouseEntered(ItemUIEventArgs e) => MouseEntered?.Invoke(e);
    void HandleMouseExited(ItemUIEventArgs e) => MouseExited?.Invoke(e);
    void HandleRightClicked(ItemUIEventArgs e) => RightClicked?.Invoke(e);
    void HandleDragBegan(ItemUIEventArgs e) => DragBegan?.Invoke(e);
    void HandleDragging(ItemUIEventArgs e) => Dragging?.Invoke(e);
    void HandleDragEnded(ItemUIEventArgs e) => DragEnded?.Invoke(e);

    // 이벤트 추적용
    // void HandleMouseEntered(ItemUIEventArgs e)
    // {
    //     Log.Info("MouseEnter");
    //     MouseEntered?.Invoke(e);
    // }

    // void HandleMouseExited(ItemUIEventArgs e)
    // {
    //     Log.Info("MouseExit");
    //     MouseExited?.Invoke(e);
    // }

    // void HandleRightClicked(ItemUIEventArgs e)
    // {
    //     Log.Info("RightClick");
    //     RightClicked?.Invoke(e);
    // }

    // void HandleDragBegan(ItemUIEventArgs e)
    // {
    //     Log.Info("DragBegin");
    //     DragBegan?.Invoke(e);
    // }

    // void HandleDragging(ItemUIEventArgs e)
    // {
    //     Log.Info("Dragging");
    //     Dragging?.Invoke(e);
    // }

    // void HandleDragEnded(ItemUIEventArgs e)
    // {
    //     Log.Info("DragEnd");
    //     DragEnded?.Invoke(e);
    // }


}
