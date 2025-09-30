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
        StartCoroutine(OtherExplosion(transform.position));
    }
    IEnumerator OtherExplosion(Vector3 pos)
    {
        yield return new WaitForSeconds(0.2f);
        Collider[] colls = Physics.OverlapSphere(pos, exploreRadius);
        foreach (var coll in colls)
        {
            Rigidbody rb = coll.attachedRigidbody;
            if (rb != null)
            {
                rb.AddExplosionForce(25000f, pos, exploreRadius);
                Barrels barrel = rb.GetComponent<Barrels>();
                if (barrel != null)
                {
                    barrel.Explosion();
                }
            }
        }
    }
}
