using UnityEngine;

public class EquipSlotPanel : SlotPanelBase
{
    [SerializeField] EquipInventoryMono equipInventory;
    public EquipInventoryMono EquipInventory => equipInventory;
    protected override IItemSource GetSource() => equipInventory.Core;

    [SerializeField] private InventorySlotUI[] fixedSlots;

    private enum EquipIndex
    {
        Weapon = 0,
        Helmet = 1,
        ChestArmor = 2
    }
    [Tooltip("0: Weapon, 1: Helmet, 2: ChestArmor")]
    InventorySlotUI weaponSlot => uiSlots[(int)EquipIndex.Weapon];
    InventorySlotUI helmetSlot => uiSlots[(int)EquipIndex.Helmet];
    InventorySlotUI chestArmorSlot => uiSlots[(int)EquipIndex.ChestArmor];
    void Awake()
    {
        uiSlots.Clear();
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
}
