
using UnityEngine;

[CreateAssetMenu(menuName = "Item/Effects/HealHp")]
public class HealHpEffect : ItemUseEffect
{
    [SerializeField] int healAmount;

    public override bool Apply(ItemUseContext ctx)
    {
        if (ctx.player == null) return false;
        return ctx.player.Heal(healAmount);
    }
}