
using UnityEngine;

public abstract class ItemUseEffect : ScriptableObject
{
    public abstract bool Apply(ItemUseContext ctx);
}

public class ItemUseContext
{
    public PlayerController player;

    // 필요하면 더 추가 가능

    public ItemUseContext(PlayerController p)
    {
        player = p;
    }
}