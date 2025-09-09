using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    [SerializeField] private Transform target;
    private Transform cam;
    [Range(2.0f, 20.0f)]
    public float distance = 10.0f;

    [Range(0.0f, 10.0f)]
    public float height = 2.0f;

    public float damping = 10.0f;
    private Vector3 velocity = Vector3.zero;
    void Awake()
    {
        cam = GetComponent<Transform>();
    }

    void LateUpdate()
    {
        Vector3 pos = target.position
            + (-target.forward * distance)
            + (Vector3.up * height);

        cam.position = Vector3.SmoothDamp(cam.position, pos, ref velocity, Time.deltaTime*damping);

        cam.LookAt(target.position + Vector3.up * 1.5f);
    }
}
