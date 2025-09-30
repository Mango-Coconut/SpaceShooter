using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    [SerializeField] float forawrdOffset;   // (원문 유지)
    [SerializeField] float overlapRadius;
    [SerializeField] LayerMask itemLayer;
    public bool gizmoEnable = true;

    Items selectedItem;                     // prevHighlighted -> selectedItem 로 변경
    static readonly Collider[] buffer = new Collider[32];

    float timer = 0;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.1f)
        {
            timer = 0;
            SelectItems();
        }
    }

    public ItemData PickItems()
    {
        if (selectedItem == null) return null;

        ItemData id = selectedItem.itemData;
        GameObject go = selectedItem.gameObject;
        selectedItem = null;
        go.SetActive(false);
        return id;
    }

    // 화면 중앙(크로스헤어) 레이 기준으로 가장 가까운 아이템 선택
    public void SelectItems()
    {
        // 0) 후보 수집: 플레이어 전방 오프셋 중심의 구
        Vector3 center = transform.position + transform.forward * forawrdOffset;
        int count = Physics.OverlapSphereNonAlloc(center, overlapRadius, buffer, itemLayer);

        if (count == 0)
        {
            if (selectedItem != null)
            {
                selectedItem.Shining(false);
                selectedItem = null;
            }
            return;
        }

        // 1) 크로스헤어(화면 중앙)에서 나가는 레이
        Camera cam = Camera.main;
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // 2) 레이에 가장 가까운 콜라이더 고르기 (수직거리^2 최소, 동률 시 레이 앞쪽 t 최소)
        Collider best = null;
        float bestPerpSqr = float.MaxValue;
        float bestAlong   = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var c = buffer[i];
            if (c == null) continue;

            Vector3 toCenter = c.bounds.center - ray.origin;
            float t = Vector3.Dot(toCenter, ray.direction);
            if (t < 0f) continue; // 카메라 뒤쪽은 제외

            Vector3 r = ray.origin + ray.direction * t; // 레이 위 최근접점
            Vector3 q = c.ClosestPoint(r);              // 콜라이더에서 r에 가장 가까운 점

            float perpSqr = (q - r).sqrMagnitude;

            if (perpSqr < bestPerpSqr || (Mathf.Approximately(perpSqr, bestPerpSqr) && t < bestAlong))
            {
                bestPerpSqr = perpSqr;
                bestAlong   = t;
                best        = c;
            }
        }

        Items item = best ? best.GetComponent<Items>() : null;

        if (selectedItem != null && selectedItem != item)
        {
            selectedItem.Shining(false);
        }

        if (item != null && selectedItem != item)
        {
            item.Shining(true);
            selectedItem = item;
        }
        else if (item == null)
        {
            selectedItem = null;
        }
    }

    public bool CanPickUp()
    {
        return selectedItem != null;
    }

    void OnDrawGizmos()
    {
        if (!gizmoEnable) return;
        Gizmos.color = Color.yellow;
        Vector3 overlapOffset = transform.position + transform.forward * forawrdOffset;
        Gizmos.DrawWireSphere(overlapOffset, overlapRadius);
    }
}