using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestSlot : MonoBehaviour
{
    QuestData data;
    public QuestData Data => data;
    [SerializeField] TMP_Text questTitle;
    [SerializeField] TMP_Text questState;

    public void Set(QuestInstance instance)
    {        
        if(data == null) data = instance.data;
        else if(data != instance.data) return;

        questTitle.SetText(data.title);
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
