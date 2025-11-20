using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour, IInteractable
{
    [SerializeField] Sprite ladderSprite;
    public Transform startPos;
    public Transform startCamPos;
    public Transform topEndPos;
    public Transform bottomEndPos;
    public Transform topEndCamPos;
    public Transform bottomEndCamPos;
    [SerializeField] LayerMask obstacleMask;
    bool isUsing = false;
    PlayerController curPc;

    [SerializeField] GameObject topcollider;
    [SerializeField] GameObject bottomcollider;

    public void Interact(PlayerController pc)
    {
        if(curPc != null) {
            Debug.Log("이미 점유 중인 플레이어가 있음. 혹은 끝날때 널처리 안함");
            return;
        }

        curPc = pc;

        if (Physics.CheckSphere(startPos.position, 0.2f, obstacleMask))
            return;

        pc.StartLadderClimb(this);
        SetActiveChildCollider(true);
    }

    public bool IsAvailable()
    {
        return !isUsing;
    }

    public void OnFocus()
    {
        
    }

    public void OnUnfocus()
    {
        
    }
        public Sprite GetIcon() => ladderSprite;

    public (string inputKeyText, string behaviorText) GetPrompt() => ("F", "올라타기");

    public void SetActiveChildCollider(bool b){
        topcollider.SetActive(b);
        bottomcollider.SetActive(b);
    }
    public void Clear(){
        SetActiveChildCollider(false);
        curPc = null;
    }
}
