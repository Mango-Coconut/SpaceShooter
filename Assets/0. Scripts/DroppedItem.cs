
using UnityEngine;

public class DroppedItem : MonoBehaviour, IInteractable
{
    WorldInventory worldInventory;
    public StoredItem item;
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

    public void SetWorldInventory(WorldInventory WI)
    {
        worldInventory = WI;
        transform.SetParent(WI.gameObject.transform);
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

    public void Interact(PlayerController player)
    {
        if (player == null || player.inventory == null) return;
        if (worldInventory == null) return;
        if (item == null || item.itemData == null) return;

        bool picked = InventoryManager.Instance.TryHandleRightClick(item, worldInventory);

        if (!picked)
        {
            // 못 주움 (인벤토리 풀 등). 시각 피드백만 주고 그대로 둔다.
            return;
        }

        player.PlayAnimToTrigger(PickHash);
        Shining(false);
        worldInventory.NotifyPickedUp(this);
        Destroy(gameObject);
    }

    public (string , string) GetPrompt() => ("F", "줍기");
    public Sprite GetIcon()   => item.itemData ? item.itemData.icon : null;

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
