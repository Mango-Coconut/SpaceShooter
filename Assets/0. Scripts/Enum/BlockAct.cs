// 플레이어 행동 제한
[System.Flags]
public enum BlockAct
{
    None = 0,
    Move = 1 << 0,
    PlayerRotate = 1 << 1,
    SightRotate = 1 << 2,
    Jump = 1 << 3,
    Fire = 1 << 4,
    Interact = 1 << 5,
    All = ~0
}