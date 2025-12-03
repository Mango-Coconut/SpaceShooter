using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemUseManager : MonoBehaviour
{
    public static ItemUseManager Instance { get; private set; }

    PlayerController player;

    ItemUseContext ctx;

    void Awake()
    {
        Instance = this;
        player = FindFirstObjectByType<PlayerController>();
        ctx = new ItemUseContext(player);
    }

public bool TryUse(ItemData data)
{
    if (data == null || data.useEffects == null || data.useEffects.Length == 0)
        return false;

    bool used = false;

    foreach (ItemUseEffect effect in data.useEffects)
    {
        if (effect == null)
            continue;

        if (effect.Apply(ctx))
            used = true;
    }

    return used;
}
}
