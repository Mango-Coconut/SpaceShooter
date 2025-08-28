using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float turnSpeed = 180;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        float r = Input.GetAxis("Mouse X");
        Vector3 moveDir = (Vector3.forward * v) + (Vector3.right * h);
        gameObject.transform.Translate(moveDir.normalized * moveSpeed * Time.deltaTime);
        gameObject.transform.Rotate(Vector3.up * r * turnSpeed * Time.deltaTime);
    }
}
