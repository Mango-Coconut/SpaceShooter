using System;
using UnityEngine;

public class NpcUIPresence : MonoBehaviour
{
    public static event Action<int> OnStateChanged;
    
    void OnEnable() 
    {
        OnStateChanged?.Invoke(1);
    }
    void OnDisable() 
    {
        OnStateChanged?.Invoke(-1);
    }
}
