using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractUIController : MonoBehaviour
{
    [SerializeField] InteractionPanel interactionPanel;
    public bool IsOpen => interactionPanel != null && interactionPanel.gameObject.activeSelf;

    void Awake()
    {
        if (interactionPanel == null) { Log.Warn("InteractUIController에 interactionPanel 넣기"); }
    }

    public void Show()
    {
        interactionPanel.gameObject.SetActive(true);
    }

    public void Refresh(IInteractable target)
    {
        if (target == null)
        {
            Log.Error($"InteractUIController : IInteractable is null");
            return;
        }
        interactionPanel.Set(target);
    }
    
    public void Hide()
    {
        interactionPanel.gameObject.SetActive(false);
    }
}
