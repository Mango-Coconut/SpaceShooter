using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestReward
{
    public int coin;
    //public int bitcoin;

    [Header("아이템 보상")]
    public List<QuestItemReward> items;
}