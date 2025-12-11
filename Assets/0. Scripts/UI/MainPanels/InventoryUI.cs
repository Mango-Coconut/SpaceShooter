using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    //인벤토리 슬롯
    [SerializeField] SlotPanel slotPanel;
    public SlotPanel SlotPanel => slotPanel;

    //장비 슬롯
    [SerializeField] EquipSlotPanel equipSlotPanel;
    public EquipSlotPanel EquipSlotPanel => equipSlotPanel;

    //구독 편하게 하기 용
    protected SlotEventAggregator[] forwarders;
    SlotEventBridge slotEventBridge = new SlotEventBridge();
    public SlotEventBridge SlotEventBridge => slotEventBridge;

    void Awake()
    {
        forwarders = GetComponentsInChildren<SlotEventAggregator>(true);
    }

    void OnEnable()
    {
        foreach (var forwarder in forwarders)
        {
            slotEventBridge.Subscribe(forwarder);
        }
    }

    private void OnDisable()
    {
        foreach (var forwarder in forwarders)
        {
            slotEventBridge.UnSubscribe(forwarder);
        }
    }
}

