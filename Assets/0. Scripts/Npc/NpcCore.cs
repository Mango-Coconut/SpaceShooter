using System;
using System.Collections.Generic;

public class NpcCore
{
    public string NpcName { get; }
    public bool CanTalk { get; private set; } = true;

    public DialogueCore dialogueCore { get; private set; }
    public NpcCore(string name, DialogueAsset dialogueAsset)
    {
        NpcName = name;

        if (dialogueAsset != null)
        {
            dialogueCore = new DialogueCore();
            dialogueCore.Start(dialogueAsset);
        }
    }
}