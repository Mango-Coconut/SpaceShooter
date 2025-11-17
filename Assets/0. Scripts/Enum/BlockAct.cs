[System.Flags]
public enum BlockAct
{
    None = 0,
    Move = 1 << 0,
    Jump = 1 << 1,
    Climb = 1 << 2,
    Fire = 1 << 3,
    Interact = 1 << 4
}