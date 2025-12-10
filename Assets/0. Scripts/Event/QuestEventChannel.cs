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

    // 보상 미리보기 요청
    public event Action<QuestData> OnRequestRewardPreview;
    public void RaiseRequestRewardPreview(QuestData data)
    {
        if (data != null) OnRequestRewardPreview?.Invoke(data);
    }

    // 보상 미리보기 숨기기 요청
    public event Action OnRequestRewardPreviewHide; public void RaiseRequestRewardPreviewHide()
    {
        OnRequestRewardPreviewHide?.Invoke();
    }
}