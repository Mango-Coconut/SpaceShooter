using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour, IInteractable
{
    [SerializeField] Sprite ladderSprite;

    public Transform topStartPos;
    public Transform topEndPos;
    public Transform bottomStartPos;
    public Transform bottomEndPos;

    public Transform topCamPos;
    public Transform bottomCamPos;
    [SerializeField] LayerMask obstacleMask;
    bool isUsing = false;
    PlayerController curPlayer;

    [SerializeField] GameObject topcollider;
    [SerializeField] GameObject bottomcollider;

    void Awake()
    {
        Clear();
    }

    public void Interact(PlayerController pc)
    {
        // 이미 누가 사용 중이면 막기
        if (curPlayer != null)
        {
            Debug.Log("이미 점유 중인 플레이어가 있음. 혹은 끝날때 널처리 안함");
            return;
        }

        // 플레이어 기준으로 위/아래 시작점 중 더 가까운 쪽 고르기
        Vector3 pcPos = pc.transform.position;

        float distTop = (topStartPos.position - pcPos).sqrMagnitude;
        float distBottom = (bottomStartPos.position - pcPos).sqrMagnitude;

        bool startFromTop = distTop <= distBottom;

        Transform startPos = startFromTop ? topStartPos : bottomStartPos;
        Transform startCamPos = startFromTop ? topCamPos : bottomCamPos;

        // 선택된 시작점에 장애물 있는지 체크
        if (Physics.CheckSphere(startPos.position, 0.2f, obstacleMask))
        {
            // 여기서 막히면 curPlayer를 잡지 말아야 다음에 다시 시도 가능
            return;
        }

        curPlayer = pc;

        curPlayer.StartLadderClimb(this, startPos, startCamPos);
        SetActiveChildCollider(true);
    }
    public void Exit()
    {
        //이미 플레이어쪽에서 Clear 부름
        //Exit 불가. 끝까지 내려가던가 올라가야 끝나기 가능
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

    public void SetActiveChildCollider(bool b)
    {
        topcollider.SetActive(b);
        bottomcollider.SetActive(b);
    }
    public void Clear()
    {
        SetActiveChildCollider(false);
        curPlayer = null;
    }

    public bool CanInteract()
    {
        if (curPlayer != null) return false;

        return true;
    }
}
