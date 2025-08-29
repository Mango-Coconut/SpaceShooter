using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float turnSpeed = 180;
    Animation anim;
    void Awake()
    {
        anim = GetComponent<Animation>();
    }

    void Start()
    {
        anim.Play("Idle");
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float r = Input.GetAxis("Mouse X");
        PlayerMoveAnim(h, v);
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
        gameObject.transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);
        gameObject.transform.Rotate(Vector3.up * r * turnSpeed * Time.deltaTime);
    }
    void PlayerMoveAnim(float h, float v)
    {
        if (v >= 0.1f)
        {
            anim.CrossFade("RunF", 0.25f);
        }
        else if (v < -0.1f)
        {
            anim.CrossFade("RunB", 0.25f);
        }
        else if (h >= 0.1f)
        {
            anim.CrossFade("RunR", 0.25f);
        }
        else if (h < -0.1f)
        {
            anim.CrossFade("RunL", 0.25f);
        }
        else
        {
            anim.CrossFade("Idle", 0.25f);
        }
    }
}
