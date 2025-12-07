using UnityEngine;

public abstract class ItemUseEffect : ScriptableObject
{
    public abstract bool Apply(ItemUseContext ctx);
}

public class ItemUseContext
{
    public PlayerController player;

    public ItemUseContext(PlayerController p)
    {
        player = p;
    }
}