using System;
using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Event/EventChannel/QuestEventChannel")]
public class QuestEventChannel : ScriptableObject
{   
    // 퀘스트 상태 변화시마다
    public event Action<QuestInstance> OnQuestStateChanged;
    public void RaiseQuestStateChanged(QuestInstance instance)
    {
        if (instance != null) OnQuestStateChanged?.Invoke(instance);
    }
}