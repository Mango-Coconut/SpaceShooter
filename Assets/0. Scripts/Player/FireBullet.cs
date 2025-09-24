using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class FireBullet : MonoBehaviour
{
    [SerializeField] private GameObject Bullet;
    [SerializeField] private Transform firePos;
    [SerializeField] AudioClip fireSfx;
    private new AudioSource audio;
    MeshRenderer muzzleFlash;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
        muzzleFlash = firePos.GetComponentInChildren<MeshRenderer>();
        muzzleFlash.enabled = false;
    }
    public void Fire()
    {
        Instantiate(Bullet, firePos.position, firePos.rotation);
        audio.PlayOneShot(fireSfx, 1.0f);
        
        StartCoroutine(ShowMuzzleFlash());
    }

    IEnumerator ShowMuzzleFlash()
    {
        muzzleFlash.material.mainTextureOffset = new Vector2(Random.Range(0,2), Random.Range(0,2))*0.5f;
        muzzleFlash.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        muzzleFlash.transform.localScale = Vector3.one * Random.Range(0.5f, 1.0f);
        muzzleFlash.enabled = true;
        yield return new WaitForSeconds(0.2f);
        muzzleFlash.enabled = false;
    }
}
