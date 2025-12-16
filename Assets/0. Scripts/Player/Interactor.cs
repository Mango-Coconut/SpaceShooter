using System;
using UnityEngine;

public class Interactor : MonoBehaviour
{

    [SerializeField] float maxDistance;
    [SerializeField] float forwardOffset;
    [SerializeField] float overlapRadius;
    [SerializeField] LayerMask interactableLayer;
    public bool gizmoEnable = true;

    public IInteractable selected;
    public IInteractable current;
    static readonly Collider[] buffer = new Collider[32];


    public event Action<IInteractable> SelectedChanged;

    public void Scan()
    {
        if(current != null)
        {
            SelectedChanged?.Invoke(null);
            return;
        } 

        var cam = Camera.main; if (!cam) return;

        Vector3 center = transform.position + transform.forward * forwardOffset;
        int count = Physics.OverlapSphereNonAlloc(center, overlapRadius, buffer, interactableLayer);

        IInteractable best = null;
        if (count > 0)
        {
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            float bestPerpSqr = float.MaxValue;
            float bestAlong = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var col = buffer[i]; if (!col) continue;
                var cand = col.GetComponentInParent<IInteractable>(); 
                if (cand == null) continue;
                // 상호작용 로직 바꾸기
                if (cand == null || !cand.CanInteract()) continue;
                

                Vector3 toCenter = col.bounds.center - ray.origin;
                float t = Vector3.Dot(toCenter, ray.direction);
                if (t < 0f || t > maxDistance) continue;

                Vector3 r = ray.origin + ray.direction * t;
                Vector3 q = col.ClosestPoint(r);
                float perpSqr = (q - r).sqrMagnitude;

                if (perpSqr < bestPerpSqr || (Mathf.Approximately(perpSqr, bestPerpSqr) && t < bestAlong))
                {
                    bestPerpSqr = perpSqr;
                    bestAlong = t;
                    best = cand;
                }
            }
        }

        if (!ReferenceEquals(selected, best))
        {
            selected?.OnUnfocus();
            selected = best;
            selected?.OnFocus();
            SelectedChanged?.Invoke(selected);
        }
    }


    public bool Interact(PlayerController player)
    {
        // 사다리는 상호작용이 아닌 이동 상태로 취급, current에 넣지 않는다.
        Ladder ladder = selected as Ladder;
        if (ladder != null)
        {
            if (!ladder.CanInteract()) return false;   // 이미 타는 중이면 무시
            ladder.Interact(player);
            return true;
        }

        if (current != null)
        {
            InteractExit(); return true;
        }
        if (selected == null) return false;
        if (!selected.IsAvailable()) return false;
        current = selected;
        current.Interact(player);
        return true;
    }

    public void InteractExit()
    {
        if(current == null) return;
        current.Exit();
        current = null;
    }

    public void Clear()
    {
        selected = null;
        SelectedChanged?.Invoke(null);
    }

    void OnDrawGizmos()
    {
        if (!gizmoEnable) return;

        Gizmos.color = Color.cyan;

        // 플레이어 위치 + 전방 오프셋 기준
        Vector3 center = transform.position + transform.forward * forwardOffset;
        Gizmos.DrawWireSphere(center, overlapRadius);

        // 레이(크로스헤어 방향)도 참고용으로 그려줌
        Camera cam = Camera.main;
        if (cam)
        {
            Gizmos.color = Color.yellow;
            Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);
        }
    }
}
