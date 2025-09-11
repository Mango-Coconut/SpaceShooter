using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Barrels : MonoBehaviour
{
    int hp = 10;
    public float exploreRadius = 10f;
    [SerializeField] GameObject explosionVFX;
    [SerializeField] Texture[] textures;
    CapsuleCollider capsuleCollider;
    Rigidbody rb;
    bool isExploded = false;

    void Awake()
    {
        //Physics
        capsuleCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();

        //Texture
        int idx = Random.Range(0, textures.Length);
        GetComponent<MeshRenderer>().material.mainTexture = textures[idx];
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet") && --hp == 0)
        {
            Explosion();
        }
    }
    void Explosion()
    {
        if (isExploded) return;
        isExploded = true;
        Instantiate(explosionVFX, transform.position, Quaternion.identity);
        rb.AddForce(Vector3.up * 700f, ForceMode.Impulse);
        rb.AddTorque(Random.onUnitSphere * 100f, ForceMode.Impulse);
        OtherExplosion(transform.position);
    }
    void OtherExplosion(Vector3 pos)
    {
        Collider[] colls = Physics.OverlapSphere(pos, exploreRadius, 1 << 3);
        foreach (var coll in colls)
        {
            if (coll.CompareTag("Barrel"))
            {
                coll.GetComponent<Rigidbody>().AddExplosionForce(25000f, pos, exploreRadius);
                coll.GetComponent<Barrels>().Explosion();
            }
        }
    }
}
