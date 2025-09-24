using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour
{
    [SerializeField] public ItemData itemData;
    [SerializeField] private int amount = 1;
    public bool isOn = true;
    Renderer rd;
    MaterialPropertyBlock mpb;
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    float onIntensity = 0.75f; 

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        rd = GetComponent<Renderer>();
        foreach (var mat in rd.materials)
        {
            mat.EnableKeyword("_EMISSION");
        }
    }

    public void Shining(bool enable)
    {
        if (isOn == enable) return;
        isOn = enable;
        
        for (int i = 0; i < rd.sharedMaterials.Length; i++)
        {
            rd.GetPropertyBlock(mpb, i);
            mpb.SetColor(EmissionColorID, enable ? Color.white * onIntensity : Color.black);
            rd.SetPropertyBlock(mpb, i);
        }
    }
}
