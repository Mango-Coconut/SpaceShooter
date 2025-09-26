using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SortPanel : MonoBehaviour
{
    [SerializeField] GameObject content;
    [SerializeField] GameObject blocker;

    void Start()
    {
        content.SetActive(false);
    }

    public void ContentToggle()
    {
        content.SetActive(!content.activeSelf);
        blocker.SetActive(!blocker.activeSelf);
    }
}
