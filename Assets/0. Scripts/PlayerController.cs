using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    bool ispick = false;
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float turnSpeed = 360;
    Animation anim;
    void Awake()
    {
        anim = GetComponent<Animation>();
    }

    IEnumerator Start()
    {
        anim.Play("Idle");

        turnSpeed = 0f;
        yield return new WaitForSeconds(0.3f);
        turnSpeed = 360.0f;
    }

    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float r = Input.GetAxis("Mouse X");
        PlayerMoveAnim(h, v);
        if (ispick) return;
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
        gameObject.transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);
        gameObject.transform.Rotate(Vector3.up * r * turnSpeed * Time.deltaTime);
    }
    void PlayerMoveAnim(float h, float v)
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ispick = true;
            anim.CrossFade("Picking Up", 0.25f);
        }
        if (ispick) return;
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
    public void disablePickUp()
    {
        ispick = false;
    }
}
