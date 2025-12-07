
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/ItemEffects/HealHp")]
public class TestItemEffect : ItemUseEffect
{
    public override bool Apply(ItemUseContext ctx)
    {
        Debug.Log($"테스트 아이템 효과 적용 됨");
        return true;
    }
}