using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponItemData", menuName = "ScriptableObjects/ItemData/WeaponItemData", order = 1)]
public class WeaponItemData : ItemData
{
    public GameObject modelPrefab;
    public GameObject bulletPrefab;
    public AudioClip fireSound;

    public float fireDelay;
    public float bulletSpread;

}
