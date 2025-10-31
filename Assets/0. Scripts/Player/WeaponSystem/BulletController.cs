using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public GameObject sparkEffect;
    public float damage;
    public float force;
    public float exploreRadius = 10f;
    private Rigidbody rb;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    // Start is called before the first frame update
    void Start()
    {
        rb.AddForce(transform.forward * force, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision other) {
        ContactPoint cp = other.GetContact(0);
        Quaternion rot = Quaternion.LookRotation(cp.normal);
        Instantiate(sparkEffect, cp.point, rot);
        Destroy(gameObject);
    }
}
