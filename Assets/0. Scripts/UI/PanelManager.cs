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

    [Header("InteractionPanels")]
    [SerializeField] InteractionPanel iiPanel;

    [Header("chestPanels")]
    [SerializeField] ChestPanel chestPanel;

    void OnEnable()
    {
        if (interactor) interactor.TargetChanged += IiPanelChange;
        Chest.OnChestOpened += ChestUIToggle;

        if (InputManager.Instance == null) return;
        InputManager.Instance.OnToggleInventory += InventoryUIToggle;
        InputManager.Instance.OnEsc += InventoryUIHandleEsc;
    }
    void OnDisable()
    {
        if (interactor) interactor.TargetChanged -= IiPanelChange;
        Chest.OnChestOpened -= ChestUIToggle;

        if (InputManager.Instance == null) return;
        InputManager.Instance.OnToggleInventory -= InventoryUIToggle;
        InputManager.Instance.OnEsc -= InventoryUIHandleEsc;
    }

    void Start()
    {
        inventoryUI.SetActive(false);
        iiPanel.gameObject.SetActive(false);
        chestPanel.gameObject.SetActive(false);
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

    void InventoryUIHandleEsc()
    {
        if (isInvenOpen || isChestOpen)
        {
            CloseInventoryUI();
            CloseChestUI();
        }
        else
        {
            // UI가 없을 때만 커서 토글 허용
            CursorController.Apply(!CursorController.LookEnabled);
        }
    }


    bool isInvenOpen;
    void InventoryUIToggle()
    {
        if (isInvenOpen == false)
        {
            OpenInventoryUI();
        }
        else
        {
            CloseInventoryUI();
            CloseChestUI();
        }
    }
    void OpenInventoryUI()
    {
        isInvenOpen = true;
        inventoryUI.SetActive(true);
        CursorController.Apply(false);
    }
    void CloseInventoryUI()
    {
        isInvenOpen = false;
        inventoryUI.SetActive(false);
        CursorController.Apply(true);
    }

    bool isChestOpen = false;
    void ChestUIToggle(Chest c)
    {
        if (isChestOpen == false)
        {
            OpenInventoryUI();
            OpenChestUI(c);
        }
        else
        {
            CloseInventoryUI();
            CloseChestUI();
        }
    }
    void OpenChestUI(Chest c)
    {
        isChestOpen = true;
        // 상자 열 때 UI에 Chest 전달
        chestPanel.deliverChest(c);
        chestPanel.gameObject.SetActive(true);
    }
    void CloseChestUI()
    {
        isChestOpen = false;
        chestPanel.gameObject.SetActive(false);
    }
}