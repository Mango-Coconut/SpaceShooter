
using UnityEngine;

public class DroppedItem : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] Transform modelRoot;

    WorldInventoryMono worldInventory;

    [Header("Editor Setup (for pre-placed drops)")]
    public ItemData initialData;
    public int initialCount = 1;
    public int initialEnhancement = 0;

    // 실제 들고 있는 아이템 데이터(스택/강화/고유ID 등)
    StoredItem item;
    public StoredItem Item => item;

    // 하이라이트/발광 관련
    Renderer[] renderers;
    MaterialPropertyBlock mpb;
    bool isOn = false;

    static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");
    const float onIntensity = 0.75f;

    // 줍기 연출용
    static readonly int PickHash = Animator.StringToHash("Pick");

    void Awake()
    {
        mpb = new MaterialPropertyBlock();

        // 1) 에디터에서 씬에 직접 깔아둔 드랍만 초기화
        //    (= 프리팹이 아니라, 장면에 존재하는 상태로 시작하는 경우)
        if (item == null && initialData != null)
        {
            item = new StoredItem(
                initialData,
                Mathf.Max(initialCount, 1)
            );
            //item.enhancement = initialEnhancement;

            // 이제 이 아이템의 비주얼 반영
            ApplyVisualModel();
        }

        Shining(false);
    }

    // 월드 인벤토리가 런타임에 생성한 직후 호출하는 초기화 루틴
    // 저장된 StoredItem을 통째로 갖고 옴 (instanceId, count 등 포함)
    public void Bind(StoredItem newItem)
    {
        item = newItem;

        ApplyVisualModel();
        Shining(false);
    }

    PlayerController curPlayer;
    // 플레이어가 이 아이템을 줍으려고 했을 때 호출
    public void Interact(PlayerController pc)
    {
        if (pc == null || pc.inventory == null) return;
        if (worldInventory == null) return;
        if (item == null || item.itemData == null) return;

        InventoryManager IM = InventoryManager.Instance;
        bool picked = IM.TryDeliverBasic(IM.GetSource(StorageTarget.World), IM.GetSink(StorageTarget.Player), item);
        // 인벤 공간 없거나 등등 실패 -> 그냥 바닥에 남아있음.
        if (picked) return;

        // 성공적으로 플레이어 인벤으로 들어갔으면 줍는 애니메이션 실행과 하이라이트 종료
        curPlayer = pc;
        pc.PlayAnimToTrigger(PickHash);
        pc.gate.PushAll();
        Shining(false);

        // 실제 파괴는 WorldInventory 쪽에서 처리됨. 여기서는 안 없앰.
    }

    public void Exit()
    {
        if(curPlayer == null) return;
        curPlayer.gate.PopAll();
    }

    // 외형 모델링을 modelRoot 밑에 붙여준다.
    // 기존 모델은 제거하고 새 모델로 교체.
    void ApplyVisualModel()
    {
        // modelRoot 확인
        if (modelRoot == null)
        {
            Debug.LogWarning($"[{name}] modelRoot is not assigned — please set it in the DroppedItem prefab!");
            return;
        }

        // 기존 모델 제거
        for (int i = modelRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(modelRoot.GetChild(i).gameObject);
        }

        // 새 모델 생성
        if (item != null && item.itemData != null && item.itemData.modelPrefab != null)
        {
            GameObject modelInstance = Instantiate(item.itemData.modelPrefab, modelRoot);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Debug.LogWarning($"[{name}] modelPrefab missing in ItemData ({item?.itemData?.name ?? "null"})");
        }

        // 모델이 바뀔 때마다 렌더러, 발광 세팅 갱신
        RefreshRendererArray();
        EnableEmissionKeywordOnAll();
    }

    // 현재 모델에서 하이라이트 대상 Renderer들을 전부 모은다.
    void RefreshRendererArray()
    {
        renderers = modelRoot.GetComponentsInChildren<Renderer>(true);
    }

    // "_EMISSION" 키워드를 켠다. (Shining에서 색만 바꿀 수 있게)
    void EnableEmissionKeywordOnAll()
    {
        if (renderers == null) return;

        for (int r = 0; r < renderers.Length; r++)
        {
            var rd = renderers[r];
            if (rd == null) continue;

            var mats = rd.materials;
            for (int m = 0; m < mats.Length; m++)
            {
                // 머티리얼이 null일 수도 있으니 방어
                var mat = mats[m];
                if (mat != null)
                {
                    mat.EnableKeyword("_EMISSION");
                }
            }
        }
    }

    // 하이라이트 토글
    public void Shining(bool enable)
    {
        if (isOn == enable) return;
        isOn = enable;

        if (renderers == null || renderers.Length == 0) return;
        if (mpb == null) mpb = new MaterialPropertyBlock();

        for (int r = 0; r < renderers.Length; r++)
        {
            Renderer rd = renderers[r];

            // 이미 Destroy된 Renderer는 skip
            if (rd == null) continue;

            // sharedMaterials 접근 전에 rd가 여전히 유효한지 재확인
            var mats = rd.sharedMaterials;
            for (int m = 0; m < mats.Length; m++)
            {
                rd.GetPropertyBlock(mpb, m);
                mpb.SetColor(
                    EmissionColorID,
                    enable ? Color.white * onIntensity : Color.black
                );
                rd.SetPropertyBlock(mpb, m);
            }
        }
    }

    // 월드 인벤토리 연결
    public void SetWorldInventory(WorldInventoryMono wi)
    {
        worldInventory = wi;

        if (wi != null)
        {
            transform.SetParent(wi.transform);
        }
    }

    // IInteractable 구현들 ------------------------

    public bool IsAvailable() => gameObject.activeInHierarchy;

    public void OnFocus()
    {
        Shining(true);
    }

    public void OnUnfocus()
    {
        Shining(false);
    }

    public (string, string) GetPrompt() => ("F", "줍기");

    public Sprite GetIcon()
    {
        return (item != null && item.itemData != null) ? item.itemData.icon : null;
    }

    

    public bool CanInteract()
    {
        return true;
    }
}