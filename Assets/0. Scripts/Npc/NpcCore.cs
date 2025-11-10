using System;
using System.Collections.Generic;

public class NpcCore
{
    //추후에 역할 나누기

    public string NpcName { get; }
    public bool CanTalk { get; private set; } = true;

    public DialogueCore dialogueCore { get; private set; }
    public NpcCore(string name)
    {
        NpcName = name;
        dialogueCore = new DialogueCore();
    }
    public void Initialize(DialogueAsset asset)
    {
        dialogueCore.Start(asset);
    }
}