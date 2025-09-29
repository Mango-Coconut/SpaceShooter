using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject inventoryUI;
    bool opened;

    void Start()
    {
        inventoryUI.SetActive(false);
        InputManager.Instance.OnToggleInventory += Toggle;
        InputManager.Instance.OnEsc += HandleEsc;
    }
    void OnDestroy()
    {
        if (InputManager.Instance == null) return;
        InputManager.Instance.OnToggleInventory -= Toggle;
        InputManager.Instance.OnEsc -= HandleEsc;
    }

    void Toggle()
    {
        opened = !opened;
        inventoryUI.SetActive(opened);
        CursorController.Apply(!opened); // 열리면 Look 비활성(커서 보이게)
    }

    void HandleEsc()
    {
        if (opened)
        {
            Toggle(); // 인벤토리 열려 있으면 닫기 우선
        }
        else
        {
            // UI가 없을 때만 커서 토글 허용
            CursorController.Apply(!CursorController.LookEnabled);
        }
    }

    public bool IsOpen => opened;
}
