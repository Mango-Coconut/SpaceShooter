using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;

public class ParallelPractice : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Parallel.For(0, 1000, (i) =>
        {
            Debug.Log($"{Thread.CurrentThread.ManagedThreadId} : {i}");
        }) ;
    }
}
