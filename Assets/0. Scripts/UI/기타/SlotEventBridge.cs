using System;
using System.Collections.Generic;

public class SlotEventBridge
{
    public event Action<SlotPanelEventArgs> MouseEntered;
    public event Action<SlotPanelEventArgs> MouseExited;
    public event Action<SlotPanelEventArgs> RightClicked;
    public event Action<SlotPanelEventArgs> DragBegan;
    public event Action<SlotPanelEventArgs> Dragging;
    public event Action<SlotPanelEventArgs> DragEnded;

    readonly List<SlotEventAggregator> sources = new List<SlotEventAggregator>();

    public void Subscribe(SlotEventAggregator newSource)
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

    public void UnSubscribe(SlotEventAggregator target)
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
    
    void HandleMouseEntered(SlotPanelEventArgs e) => MouseEntered?.Invoke(e);
    void HandleMouseExited(SlotPanelEventArgs e) => MouseExited?.Invoke(e);
    void HandleRightClicked(SlotPanelEventArgs e) => RightClicked?.Invoke(e);
    void HandleDragBegan(SlotPanelEventArgs e) => DragBegan?.Invoke(e);
    void HandleDragging(SlotPanelEventArgs e) => Dragging?.Invoke(e);
    void HandleDragEnded(SlotPanelEventArgs e) => DragEnded?.Invoke(e);

    // 이벤트 추적용
    // void HandleMouseEntered(SlotPanelEventArgs e)
    // {
    //     Log.Info("MouseEnter");
    //     MouseEntered?.Invoke(e);
    // }

    // void HandleMouseExited(SlotPanelEventArgs e)
    // {
    //     Log.Info("MouseExit");
    //     MouseExited?.Invoke(e);
    // }

    // void HandleRightClicked(SlotPanelEventArgs e)
    // {
    //     Log.Info("RightClick");
    //     RightClicked?.Invoke(e);
    // }

    // void HandleDragBegan(SlotPanelEventArgs e)
    // {
    //     Log.Info("DragBegin");
    //     DragBegan?.Invoke(e);
    // }

    // void HandleDragging(SlotPanelEventArgs e)
    // {
    //     Log.Info("Dragging");
    //     Dragging?.Invoke(e);
    // }

    // void HandleDragEnded(SlotPanelEventArgs e)
    // {
    //     Log.Info("DragEnd");
    //     DragEnded?.Invoke(e);
    // }


}
