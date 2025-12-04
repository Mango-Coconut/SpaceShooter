using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUseManager : MonoBehaviour
{
    // 이 플레이어의 ItemUseManager라는 걸 명시적으로 보여주기 위한 참조
    public PlayerController Player { get; private set; }
    public ItemUseContext Context { get; private set; }

    void Awake()
    {
        Player = GetComponent<PlayerController>();
        if (Player == null)
        {
            Debug.LogError("ItemUseManager: 같은 오브젝트에 PlayerController가 필요합니다.");
        }

        Context = new ItemUseContext(Player);
    }

    public bool TryUse(ItemData data)
    {
        if (data == null || data.useEffects == null || data.useEffects.Length == 0)
            return false;

        if (data.type != ItemType.Consumable)
            return false;

        if (Context == null)
            return false;

        bool used = false;

        for (int i = 0; i < data.useEffects.Length; i++)
        {
            ItemUseEffect effect = data.useEffects[i];
            if (effect == null)
                continue;

            if (effect.Apply(Context))
            {
                used = true;
            }
        }

        return used;
    }
}
