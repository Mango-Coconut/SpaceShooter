using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoicesPanel : MonoBehaviour
{
    [SerializeField] GameObject buttonPrefab;

    readonly List<GameObject> spawnedButtons = new List<GameObject>();

    public void Set(List<DialogueChoice> choices, System.Action<int> onClickChoice)
    {
        // 기존 버튼 정리
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            Destroy(spawnedButtons[i]);
        }
        spawnedButtons.Clear();

        if (choices == null || choices.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        for (int i = 0; i < choices.Count; i++)
        {
            DialogueChoice choice = choices[i];

            GameObject btnObj = Instantiate(buttonPrefab, transform);
            spawnedButtons.Add(btnObj);

            Button btn = btnObj.GetComponent<Button>();
            TMP_Text text = btnObj.GetComponentInChildren<TMP_Text>();
            if (text != null)
            {
                text.text = choice.text;
            }

            int capturedIndex = i; // 람다 캡처 주의
            btn.onClick.AddListener(() =>
            {
                onClickChoice?.Invoke(capturedIndex);
            });
        }
    }
}