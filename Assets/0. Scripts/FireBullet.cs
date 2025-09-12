using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class FireBullet : MonoBehaviour
{
    [SerializeField] private GameObject Bullet;
    [SerializeField] private Transform firePos;
    [SerializeField] private float delay = 0.1f;
    private float timer = 0;
    [SerializeField] AudioClip fireSfx;
    private new AudioSource audio;

    void Awake()
    {
        audio = GetComponent<AudioSource>();
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > delay && Input.GetMouseButton(0))
        {
            timer = 0;
            Fire();
        }
    }
    private void Fire()
    {
        Instantiate(Bullet, firePos.position, firePos.rotation);
        audio.PlayOneShot(fireSfx, 1.0f);
    }
}
