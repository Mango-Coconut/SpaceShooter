using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Quest/QuestEventChannel")]
public class QuestEventChannel : ScriptableObject
{
    public event Action<QuestInstance> OnQuestStateChanged;
    public void RaiseQuestStateChanged(QuestInstance instance)
    {
        if (instance != null) OnQuestStateChanged?.Invoke(instance);
    }
}