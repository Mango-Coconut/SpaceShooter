
using UnityEngine;

public class PickBehaviour : StateMachineBehaviour
{
    //Animator - Pickup 노드에 있음
    PlayerController pc;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (pc == null)
        {
            pc = animator.GetComponent<PlayerController>();
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        pc.InteractExit();
    }
}
