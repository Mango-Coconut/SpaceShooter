using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    bool ispick = false;
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float turnSpeed = 360;

    float firetimer = 0;
    [SerializeField] float delay = 0.1f;
    Animator animator;
    FireBullet fireBullet;
    void Awake()
    {
        animator = GetComponent<Animator>();
        fireBullet = GetComponent<FireBullet>();
    }

    IEnumerator Start()
    {
        turnSpeed = 0f;
        yield return new WaitForSeconds(0.3f);
        turnSpeed = 360.0f;
    }


    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float r = Input.GetAxis("Mouse X");
        animator.SetFloat("MoveX", h);
        animator.SetFloat("MoveY", v);
        if (ispick) return;
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
        gameObject.transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);
        gameObject.transform.Rotate(Vector3.up * r * turnSpeed * Time.deltaTime);

        firetimer += Time.deltaTime;
        if (firetimer > delay && Input.GetMouseButton(0))
        {
            firetimer = 0;
            fireBullet.Fire();
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MonsterAttack"))
        {
            Debug.Log($"맞음");
        }
    }
}
