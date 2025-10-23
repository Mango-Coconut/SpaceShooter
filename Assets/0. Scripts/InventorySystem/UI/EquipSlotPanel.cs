using UnityEngine;

public class EquipSlotPanel : SlotPanelBase
{
    [SerializeField] EquipInventory equipInventory;
    public EquipInventory EquipInventory => equipInventory;
    protected override IItemSource GetSource() => equipInventory;

    [SerializeField] private InventorySlotUI[] fixedSlots;

    private enum EquipIndex
    {
        Weapon = 0,
        Helmet = 1,
        ChestArmor = 2
    }
    [Tooltip("0: Weapon, 1: Helmet, 2: ChestArmor")]
    [SerializeField] InventorySlotUI weaponSlot => uiSlots[(int)EquipIndex.Weapon];
    //[SerializeField] InventorySlotUI helmetSlot => uiSlots[(int)EquipIndex.Helmet];
    //[SerializeField] InventorySlotUI chestArmorSlot => uiSlots[(int)EquipIndex.ChestArmor];
    void Awake()
    {
        uiSlots.Clear();
        uiSlots.AddRange(fixedSlots); // 수동 슬롯 연결
    }
    void OnEnable()
    {
        Log.Info($"{uiSlots.Count}");
        SubscribeInventory();
        SubscribeSlotUI();
    }
    void OnDisable()
    {
        UnSubscribeInventory();
        UnSubscribeSlotUI();
    }

    public void Refresh()
    {
        weaponSlot.Bind(equipInventory.Weapon);
    }


    void SubscribeInventory()
    {
        UnSubscribeInventory();
        equipInventory.OnChanged += Refresh;
    }
    void UnSubscribeInventory()
    {
        equipInventory.OnChanged -= Refresh;
    }
}
