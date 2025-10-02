using System;
using UnityEngine;

public class Interactor : MonoBehaviour
{

    [SerializeField] float maxDistance;
    [SerializeField] float forwardOffset;
    [SerializeField] float overlapRadius;
    [SerializeField] LayerMask interactableLayer;
    public bool gizmoEnable = true;

    IInteractable current;
    static readonly Collider[] buffer = new Collider[32];

    float timer = 0;

    public event Action<IInteractable> TargetChanged;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 0.1f) { timer = 0f; Scan(); }
    }

    void Scan()
    {
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
                var cand = col.GetComponentInParent<IInteractable>(); if (cand == null) continue;

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

        if (!ReferenceEquals(current, best))
        {
            current?.OnUnfocus();
            current = best;
            current?.OnFocus();
            TargetChanged?.Invoke(current);
        }
    }

    public void OnInteractInput(PlayerController player)
    {
        if (current == null) return;
        if (!current.IsAvailable()) return;
        current.Interact(player);
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
