using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    [SerializeField] PlayerController Player;
    [SerializeField] float forawrdOffset;
    [SerializeField] float overlapRadius;
    [SerializeField] LayerMask itemLayer;
    public bool gizmoEnable = true;
    Items prevHighlighted;
    static readonly Collider[] buffer = new Collider[32];

    float timer = 0;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.05f)
        {
            timer = 0;
            SelectItems();
        }
        
        if (Input.GetKeyDown(KeyCode.F))
        {
            PickItems();
        }
        
    }
    public void PickItems()
    {
        if (prevHighlighted == null) return;
        Destroy(prevHighlighted.gameObject);
        prevHighlighted = null;
        //Player.
    }
    public void SelectItems()
    {
        Vector3 center  = transform.position + transform.forward * forawrdOffset;
        int count = Physics.OverlapSphereNonAlloc(center, overlapRadius, buffer, itemLayer);
        if (count == 0)
        {
            if (prevHighlighted != null)
            {
                prevHighlighted.Shining(false);
                prevHighlighted = null;
            }
            return;
        }
        Collider nearest = null;
        float minDist = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var c = buffer[i];
            if (c == null) continue;

            float d = (c.transform.position - center).sqrMagnitude;
            if (d < minDist) { minDist = d; nearest = c; }
        }

        Items item = nearest ? nearest.GetComponent<Items>() : null;

        if (prevHighlighted != null && prevHighlighted != item)
            prevHighlighted.Shining(false);

        if (item != null && prevHighlighted != item)
        {
            item.Shining(true);
            prevHighlighted = item;
        }
        else if (item == null)
        {
            prevHighlighted = null;
        }
    }

    void OnDrawGizmos()
    {
        if (!gizmoEnable) return;
        // 씬 뷰에서만 보임 (게임 실행/빌드에는 안 보임)
        Gizmos.color = Color.yellow;
        Vector3 overlapOffset = transform.position + transform.forward * forawrdOffset;
        Gizmos.DrawWireSphere(overlapOffset, overlapRadius);
    }
}
