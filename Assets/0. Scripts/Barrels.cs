using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Barrels : MonoBehaviour
{
    int hp = 10;
    [SerializeField] GameObject explosionVFX;
    CapsuleCollider capsuleCollider;
    Rigidbody rb;

    void Awake()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet") && --hp == 0)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
            rb.mass = 10;

            rb.AddForce(Vector3.up * 100f, ForceMode.Impulse);
            rb.AddTorque(Random.onUnitSphere * 10f, ForceMode.Impulse);
        }
    }
}
