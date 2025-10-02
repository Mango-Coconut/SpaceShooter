
using UnityEngine;

public class PickBehaviour : StateMachineBehaviour
{
    PlayerController pc;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null)
        {
            pc = animator.GetComponent<PlayerController>();
        }
        pc.gate.PushInteract();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        pc.gate.PopInteract();
    }
}
