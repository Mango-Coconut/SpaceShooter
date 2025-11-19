using System;
using UnityEngine;

public class PlayerActionGate : MonoBehaviour
{
    private readonly int[] _cnt = new int[32];

    public BlockAct Active { get; private set; }

    public bool Can(BlockAct mask) => (Active & mask) == 0;

    #region All
    private static readonly BlockAct allMask =
    BlockAct.All;
    public void PushAll() => Push(allMask);
    public void PopAll() => Pop(allMask);
    #endregion

    //필요없어지는거같은데 나중에 폐기 고려
    #region Interaction
    private static readonly BlockAct interactMask =
    BlockAct.Move | BlockAct.Jump | BlockAct.Climb | BlockAct.Fire | BlockAct.Interact;
    public void PushInteract() => Push(interactMask);
    public void PopInteract() => Pop(interactMask);
    #endregion


    #region UIOnly
    private static readonly BlockAct uiMask =
    BlockAct.Fire | BlockAct.Climb;
    public void PushUI() => Push(uiMask);
    public void PopUI() => Pop(uiMask);
    #endregion

    public void Push(BlockAct mask)
    {
        int v = (int)mask;
        for (int i = 0; i < 32; i++)
        {
            if ((v & (1 << i)) == 0) continue;

            if (_cnt[i] == 0) Active |= (BlockAct)(1 << i);
            _cnt[i]++;
        }
    }

    public void Pop(BlockAct mask)
    {
        int v = (int)mask;
        for (int i = 0; i < 32; i++)
        {
            if ((v & (1 << i)) == 0) continue;

            if (_cnt[i] > 0)
            {
                _cnt[i]--;
                if (_cnt[i] == 0) Active &= ~(BlockAct)(1 << i);
            }
        }
    }

    public void ClearAll()
    {
        Array.Clear(_cnt, 0, _cnt.Length);
        Active = BlockAct.None;
    }
}
