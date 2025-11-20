using UnityEngine;

public class PlayerAnimStateRelay : StateMachineBehaviour
{
    // 각 상태 이름 해시 캐싱
    static readonly int ClimbingStartHash = Animator.StringToHash("Climbing Start");
    static readonly int ClimbingToTopHash   = Animator.StringToHash("Climbing To Top");
    static readonly int ClimbingDownHash   = Animator.StringToHash("Climbing Down");
    static readonly int JoyfulJumpHash       = Animator.StringToHash("Joyful Jump");
    static readonly int LandingHash       = Animator.StringToHash("Landing");

    PlayerController pc;

    void EnsureInit(Animator animator)
    {
        if (pc == null)
            pc = animator.GetComponentInParent<PlayerController>();
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        EnsureInit(animator);

        int id = info.shortNameHash;

        if (id == ClimbingStartHash) {}
            //pc.OnClimbStartEnter();

        else if (id == ClimbingToTopHash) {}
            //pc.OnClimbLoopEnter();

        else if (id == ClimbingDownHash){}
            //pc.OnClimbEndEnter();

        else if (id == JoyfulJumpHash){}
            //pc.OnFallEnter();

        else if (id == LandingHash){}
            //pc.OnLadderLandExit();
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo info, int layerIndex)
    {
        EnsureInit(animator);

        int id = info.shortNameHash;

        if (id == ClimbingStartHash)
            pc.OnClimbStartExit();

        else if (id == ClimbingToTopHash)
            pc.OnClimbEndTopExit();

        else if (id == ClimbingDownHash)
            pc.OnClimbEndBottomExit();
    }
}
