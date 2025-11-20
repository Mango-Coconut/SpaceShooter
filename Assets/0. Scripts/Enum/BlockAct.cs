[System.Flags]
public enum BlockAct
{
    None = 0,
    Move = 1 << 0,
    Rotate = 1 << 1,
    Jump = 1 << 2,
    Fire = 1 << 4,
    Interact = 1 << 5,
    All = ~0
}