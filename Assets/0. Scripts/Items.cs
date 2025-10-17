using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Items : MonoBehaviour, IInteractable
{
    public StoredItem item;
    public ItemData itemData;
    [SerializeField] int amount = 1;
    [HideInInspector] public bool isOn = false;
    Renderer rd;
    MaterialPropertyBlock mpb;
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    float onIntensity = 0.75f;
    static readonly int PickHash = Animator.StringToHash("Pick");

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        rd = GetComponent<Renderer>();
        foreach (var mat in rd.materials)
        {
            mat.EnableKeyword("_EMISSION");
        }
    }

    public bool IsAvailable()
    {
        return gameObject.activeInHierarchy;
    }

    public void OnFocus()
    {
        Shining(true);
    }

    public void OnUnfocus()
    {
        Shining(false);
    }

    /// <summary>
    /// Item의 Interact ---> PickUp 시스템
    /// </summary>
    /// <param name="player"></param>
    public void Interact(PlayerController player)
    {
        if (player == null || player.inventory == null) return;

        bool added = player.inventory.TryAddItem(itemData, amount);
        if (added)
        {
            player.PlayAnimToTrigger(PickHash);
            Shining(false);
            gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("주울 수 없음");
        }
    }

    public (string , string) GetPrompt() => ("F", "줍기");
    public Sprite GetIcon()   => itemData ? itemData.icon : null;

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
