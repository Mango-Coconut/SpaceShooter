using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestSlot : MonoBehaviour
{
    QuestInstance questInstance;
    public QuestInstance QuestInstance => questInstance;
    [SerializeField] TMP_Text questTitle;
    [SerializeField] TMP_Text questState;

    public void Set(QuestInstance instance)
    {        
        if(questInstance == null) questInstance = instance;
        else if(questInstance.data != instance.data) return;

        questTitle.SetText(questInstance.data.title);
        if(instance.state == QuestState.Locked || instance.state == QuestState.CanAccept)
        {
            questState.SetText("받기 전인데 뜨면 안되지");
        }
        else if(instance.state == QuestState.Active)
        {
            questState.SetText("진행 중");
        }
        else if (instance.state == QuestState.ReadyToTurnIn)
        {
            questState.SetText("완료 가능");
        }
        else if (instance.state == QuestState.Completed)
        {
            questState.SetText("완료됨");
        }
    }
}
