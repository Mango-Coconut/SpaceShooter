using UnityEngine;

public class SlotPanel : MonoBehaviour
{
    [SerializeField] GameObject slotPrefab;
    [SerializeField] int slotCount = 100;

    void Start()
    {
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, transform);
        }
    }
}