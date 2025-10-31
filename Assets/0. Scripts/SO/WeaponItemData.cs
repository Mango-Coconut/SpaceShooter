using UnityEngine;

[CreateAssetMenu(fileName = "WeaponItemData", menuName = "ScriptableObjects/ItemDatafolder/WeaponItemData", order = 1)]
public class WeaponItemData : ItemData
{
    public GameObject bulletPrefab;
    public int PelletCount = 1;
    //연발시 반동 
    public float RecoilPerShot;
    //여러발 흩어짐
    public float SpreadAngle;
    public AudioClip fireSound;

    public float fireDelay;
    public float bulletSpread;

}
