
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
        //PC에서 Interact시에 직접 처리했음
        //pc.gate.PushInteract();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        pc.gate.PopInteract();
    }
}
