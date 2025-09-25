using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventorySortDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;
    [SerializeField] private Inventory inventory;

    void Start()
    {
        // 1. enum → 문자열 리스트 변환
        var names = Enum.GetNames(typeof(SortType));
        var options = new List<string>(names);

        // 2. Dropdown 옵션 세팅
        dropdown.ClearOptions();
        dropdown.AddOptions(options);

        // 3. 이벤트 연결
        dropdown.onValueChanged.AddListener(OnChanged);

        // 4. 초기 선택값
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    void OnChanged(int index)
    {
        // index → enum 변환
        SortType type = (SortType)index;

        Debug.Log($"선택된 정렬 기준: {type}");

        // Inventory 정렬 실행
        inventory.Sort(type);
    }
}
