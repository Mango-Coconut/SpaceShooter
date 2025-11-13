using UnityEngine;

public class EquipSlotPanel : SlotPanelBase
{
    [SerializeField] EquipInventoryMono equipInventory;
    public EquipInventoryMono EquipInventory => equipInventory;

    [SerializeField] private InventorySlot[] fixedSlots;

    private enum EquipIndex
    {
        Weapon = 0,
        Helmet = 1,
        ChestArmor = 2
    }
    [Tooltip("0: Weapon, 1: Helmet, 2: ChestArmor")]
    InventorySlot weaponSlot => uiSlots[(int)EquipIndex.Weapon] as InventorySlot;
    InventorySlot helmetSlot => uiSlots[(int)EquipIndex.Helmet] as InventorySlot;
    InventorySlot chestArmorSlot => uiSlots[(int)EquipIndex.ChestArmor] as InventorySlot;
    void Awake()
    {
        uiSlots.AddRange(fixedSlots); // 수동 슬롯 연결
    }
    void OnEnable()
    {
        SubscribeInventory();
        SubscribeSlotUI();
        RefreshAll();
    }
    void OnDisable()
    {
        UnSubscribeInventory();
        UnSubscribeSlotUI();
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

    protected override StorageTarget GetSource()
    {
        return StorageTarget.Equip;
    }
}
