using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(CinemachineVirtualCamera))]
public class BackWallOnlyCollision : CinemachineExtension
{
    [SerializeField] Transform player;        // 플레이어 피벗
    [SerializeField] float radius = 0.3f;     // 카메라 충돌 반경
    [SerializeField] float recoverSpeed = 10f; // 막힌 후 풀릴 때 따라오는 속도
    [SerializeField] LayerMask wallMask;      // 벽 레이어
    [SerializeField] float skin = 0.05f;      // 벽에서 살짝 띄우기

    bool wasBlocked = false;
    Vector3 lastSafePos;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body) return;
        if (player == null) return;

        Vector3 desiredPos = state.RawPosition;  // 시네머신이 원래 두고 싶어하는 위치
        Vector3 playerPos = player.position;

        Vector3 dir = desiredPos - playerPos;
        float dist = dir.magnitude;
        if (dist < 0.001f)
        {
            wasBlocked = false;
            lastSafePos = desiredPos;
            return;
        }

        dir /= dist;

        // 카메라가 플레이어 "뒤쪽"에 있을 때만 처리 (앞쪽이면 무시)
        float dot = Vector3.Dot(dir, player.forward);
        if (dot > 0f)
        {
            wasBlocked = false;
            lastSafePos = desiredPos;
            return;
        }

        RaycastHit hit;
        bool blocked = Physics.SphereCast(
            playerPos,
            radius,
            dir,
            out hit,
            dist,
            wallMask,
            QueryTriggerInteraction.Ignore);

        if (blocked)
        {
            // 벽에 막힌 상태 → 벽 바로 앞까지 당기고 거기 기억
            Vector3 newPos = hit.point - dir * skin;
            state.RawPosition = newPos;
            lastSafePos = newPos;
            wasBlocked = true;
        }
        else
        {
            if (wasBlocked)
            {
                // 더 이상 안 막히는데, 이전에는 막혀있었음
                // → lastSafePos에서 desiredPos로 서서히 따라오게
                Vector3 toDesired = desiredPos - lastSafePos;
                float distanceToDesired = toDesired.magnitude;

                if (distanceToDesired < 0.001f)
                {
                    state.RawPosition = desiredPos;
                    lastSafePos = desiredPos;
                    wasBlocked = false; // 완전히 회복
                }
                else
                {
                    float maxStep = recoverSpeed * deltaTime;
                    Vector3 step;

                    if (distanceToDesired <= maxStep)
                    {
                        step = toDesired;
                        wasBlocked = false; // 이번 프레임에 바로 회복 완료
                    }
                    else
                    {
                        step = toDesired.normalized * maxStep;
                    }

                    Vector3 newPos = lastSafePos + step;
                    state.RawPosition = newPos;
                    lastSafePos = newPos;
                }
            }
            else
            {
                // 완전 자유 상태
                state.RawPosition = desiredPos;
                lastSafePos = desiredPos;
            }
        }
    }
}
