using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Interactor interactor;

    [Header("InventoryPanels")]
    [SerializeField] GameObject inventoryUI;
    bool opened;

    [Header("InteractionPanels")]
    [SerializeField] InteractionPanel iiPanel;

    void OnEnable()
    {
        if (interactor) interactor.TargetChanged += IiPanelChange;

        if (InputManager.Instance == null) return;
        InputManager.Instance.OnToggleInventory += InventoryUIToggle;
        InputManager.Instance.OnEsc += InventoryUIHandleEsc;
    }
    void OnDisable()
    {
        if (interactor) interactor.TargetChanged -= IiPanelChange;

        if (InputManager.Instance == null) return;
        InputManager.Instance.OnToggleInventory -= InventoryUIToggle;
        InputManager.Instance.OnEsc -= InventoryUIHandleEsc;
    }

    void Start()
    {
        inventoryUI.SetActive(false);
        iiPanel.gameObject.SetActive(false);
    }


    private void IiPanelChange(IInteractable interactable)
    {
        bool show = interactable != null;
        iiPanel.gameObject.SetActive(show);
        if (show)
        {
            iiPanel.OnTargetChange(interactable);
        }
    }

    void InventoryUIToggle()
    {
        opened = !opened;
        inventoryUI.SetActive(opened);
        CursorController.Apply(!opened); // 열리면 Look 비활성(커서 보이게)
    }

    void InventoryUIHandleEsc()
    {
        if (opened)
        {
            InventoryUIToggle(); // 인벤토리 열려 있으면 닫기 우선
        }
        else
        {
            // UI가 없을 때만 커서 토글 허용
            CursorController.Apply(!CursorController.LookEnabled);
        }
    }

}
