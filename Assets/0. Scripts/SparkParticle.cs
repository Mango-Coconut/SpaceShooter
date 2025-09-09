using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparkParticle : MonoBehaviour
{
    void Start()
    {
        Destroy(gameObject, 0.15f);
    }

}
