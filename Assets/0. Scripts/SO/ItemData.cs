using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemData", order = 1)]

public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    public string itemName;        // 이름
    public Sprite icon;            // UI 아이콘
    [TextArea] public string description; // 설명

    [Header("속성")]
    public ItemType type;          // 무기, 소모품, 재료 등

    public int rarity;    // 희귀도 (숫자가 클수록 희귀)
    public int price;
    public float weight;
    public float volume;

    public int maxStack = 1;       // 최대 겹침 수량

}

