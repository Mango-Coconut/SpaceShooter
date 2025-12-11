using UnityEngine;

public class EquipSlotPanel : MonoBehaviour
{
    [SerializeField] EquipInventoryMono equipInventory;
    public EquipInventoryMono EquipInventory => equipInventory;

    SlotEventAggregator forwarder;

    [SerializeField] private InventorySlot[] fixedSlots;

    private enum EquipIndex
    {
        Weapon = 0,
        Helmet = 1,
        ChestArmor = 2
    }
    [Tooltip("0: Weapon, 1: Helmet, 2: ChestArmor")]
    InventorySlot weaponSlot => fixedSlots[(int)EquipIndex.Weapon];
    InventorySlot helmetSlot => fixedSlots[(int)EquipIndex.Helmet];
    InventorySlot chestArmorSlot => fixedSlots[(int)EquipIndex.ChestArmor];

    void Awake()
    {
        forwarder = GetComponent<SlotEventAggregator>();
    }


    void OnEnable()
    {
        SubscribeInventory();
        RefreshAll();
    }
    void OnDisable()
    {
        UnSubscribeInventory();
    }

    public void RefreshAll()
    {
        // 무기
        weaponSlot.Bind(equipInventory.GetEquipped(EquipType.Weapon));

        // 헬멧
        helmetSlot.Bind(equipInventory.GetEquipped(EquipType.Helmet));

        // 갑옷
        chestArmorSlot.Bind(equipInventory.GetEquipped(EquipType.ChestArmor));
    }


    void SubscribeInventory()
    {
        UnSubscribeInventory();
        equipInventory.OnChanged += RefreshAll;
    }
    void UnSubscribeInventory()
    {
        equipInventory.OnChanged -= RefreshAll;
    }

    public StorageTarget GetSource()
    {
        return StorageTarget.Equip;
    }
}
