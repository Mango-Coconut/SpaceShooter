using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirePosGizmo : MonoBehaviour
{
    public Color c = Color.yellow;
    public float radius = 0.1f;

    void OnDrawGizmos()
    {
        Gizmos.color = c;
        Gizmos.DrawSphere(transform.position, radius);
    }
}
