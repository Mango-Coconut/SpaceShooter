using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum Block
{
    None = 0,
    Move = 1 << 0,
    Fire = 1 << 1,
    Pick = 1 << 2
}

public class PlayerActionGate
{
    private readonly int[] _cnt = new int[32];

    public Block Active { get; private set; }

    public bool Can(Block mask) => (Active & mask) == 0;


    //Pick Block Behaviors
    private static readonly Block PickMask = Block.Move | Block.Fire | Block.Pick;

    public void PushPick() => Push(PickMask);
    public void PopPick() => Pop(PickMask);




    public void Push(Block mask)
    {
        int v = (int)mask;
        for (int i = 0; i < 32; i++)
        {
            if ((v & (1 << i)) == 0) continue;

            if (_cnt[i] == 0) Active |= (Block)(1 << i);
            _cnt[i]++;
        }
    }

    public void Pop(Block mask)
    {
        int v = (int)mask;
        for (int i = 0; i < 32; i++)
        {
            if ((v & (1 << i)) == 0) continue;

            if (_cnt[i] > 0)
            {
                _cnt[i]--;
                if (_cnt[i] == 0) Active &= ~(Block)(1 << i);
            }
        }
    }

    public void ClearAll()
    {
        Array.Clear(_cnt, 0, _cnt.Length);
        Active = Block.None;
    }
}