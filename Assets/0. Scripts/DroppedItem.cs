
using UnityEngine;

public class DroppedItem : MonoBehaviour, IInteractable
{
    WorldInventoryMono worldInventory;

    // --- 에디터에서 세팅할 값들 ---
    [Header("Editor Setup")]
    public ItemData initialData;
    public int initialCount = 1;
    public int initialEnhancement = 0;

    StoredItem item;
    public StoredItem Item => item;

    [HideInInspector] public bool isOn = false;
    Renderer rd;
    MaterialPropertyBlock mpb;
    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    float onIntensity = 0.75f;
    static readonly int PickHash = Animator.StringToHash("Pick");

    void Awake()
    {
        // 1) 아직 item이 안 만들어져 있으면 여기서 자동 생성
        if (item == null)
        {
            if (initialData != null)
            {
                item = new StoredItem(initialData, Mathf.Max(initialCount, 1));
                item.enhancement = initialEnhancement;
            }
            else
            {
                Log.Info($"DroppedItem 에디터 초기화 값 필요");
            }
        }

        // 2) 외형 모델 적용
        ApplyVisualModel();

        // 3) 발광 준비
        mpb = new MaterialPropertyBlock();
        rd = GetComponentInChildren<Renderer>(true);
        if (rd != null)
        {
            foreach (var mat in rd.materials)
                mat.EnableKeyword("_EMISSION");
        }
        Shining(false);
    }
    public void Bind(StoredItem newItem)
    {
        item = newItem;
        ApplyVisualModel();

        // 여기서 시각적인 동기화도 같이 가능:
        // - 아이콘에 맞춘 머티리얼 색
        // - 무기 모델 프리팹 인스턴스 등
        // 지금은 단순히 현재 머티리얼 발광 세기만 초기화 정도로 충분
        Shining(false);
    }

    void ApplyVisualModel()
    {
        if (item == null || item.itemData == null) return;

        GameObject prefab = item.itemData.modelPrefab;
        if (prefab == null) return;

        // 현재 자식 비우고
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            Destroy(child.gameObject);
        }

        // 새 모델 생성
        GameObject modelInstance = GameObject.Instantiate(prefab, this.transform);
    }

    public void SetWorldInventory(WorldInventoryMono WI)
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

        bool picked = InventoryManager.Instance.TryHandleRightClick(item, worldInventory.Core);

        if (!picked)
        {
            // 못 주움 (인벤토리 풀 등). 시각 피드백만 주고 그대로 둔다.
            return;
        }

        player.PlayAnimToTrigger(PickHash);
        Shining(false);
        //worldInventory.NotifyPickedUp(this);
        //Destroy(gameObject);
    }

    public (string, string) GetPrompt() => ("F", "줍기");
    public Sprite GetIcon() => item.itemData ? item.itemData.icon : null;

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
