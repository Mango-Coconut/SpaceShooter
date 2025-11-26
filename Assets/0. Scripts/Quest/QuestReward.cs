using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestReward
{
    // [Header("기본 보상")]
    // public int exp;
    public int coin;

    [Header("아이템 보상")]
    public List<QuestItemReward> items;
}