using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestReward
{
    public int coin;
    //public int bitcoin; 다른 재화 등

    [Header("아이템 보상")]
    public List<QuestItemReward> items;
}