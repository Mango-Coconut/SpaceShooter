using UnityEngine;
using UnityEngine.EventSystems;

public class SortPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject content;
    [SerializeField] GameObject blocker;

    bool isPinned;

    void Start() => Show(false);
    void Show(bool on)
    {
        if (content)  content.SetActive(on);
        if (blocker)  blocker.SetActive(on);
    }

    // ToggleButton
    public void OnButtonClick()
    {
        isPinned = !isPinned;
        Show(isPinned);
    }

    // ToggleButton, Blocker
    public void OnClickOutside()
    {
        isPinned = false;
        Show(false);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        Show(true);
    }

    public void OnPointerExit(PointerEventData e)
    {
        if (!isPinned) Show(false);
    }
}