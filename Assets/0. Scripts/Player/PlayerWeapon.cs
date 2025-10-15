using System;
using System.Collections;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class PlayerWeapon : MonoBehaviour
{

    [SerializeField] Transform firePos;

    Transform currentModel;
    WeaponItemData currentData;
    new AudioSource audio;
    MeshRenderer muzzleFlash;

    float fireTimer = 0;
    float fireDelay = 0;
    float fireHeat = 0;


    void Awake()
    {
        audio = GetComponent<AudioSource>();
        muzzleFlash = firePos.GetComponentInChildren<MeshRenderer>();
        muzzleFlash.enabled = false;
    }
    void Update()
    {
        fireTimer += Time.deltaTime;
        //연발하지 않는 동안은 총알 spread 줄이기
        if (fireTimer > fireDelay) fireHeat = Math.Clamp(fireHeat - Time.deltaTime, 0, 1);
    }

    void Equip(WeaponItemData data)
    {
        if (data == null) { Debug.LogWarning("weaponitemdata null"); return; }
        if (currentData == data) return;
        Clear();

        GameObject go = Instantiate(data.modelPrefab, transform);
        currentModel = go.transform;
        currentData = data;

        ApplyOffsets(data);
        DisablePhysicsAndScripts(currentModel);
    }

    void Clear()
    {
        if (currentModel != null)
        {
            Destroy(currentModel.gameObject);
            currentModel = null;
        }
        currentData = null;
    }

    public bool CanFire()
    {
        if (currentData == null) return false;
        if (currentModel == null) return false;
        return true;
    }

    void ApplyOffsets(WeaponItemData data)
    {
        if (currentModel == null) return;

        currentModel.localPosition = Vector3.zero;
        currentModel.localRotation = Quaternion.Euler(Vector3.zero);
        currentModel.localScale = Vector3.zero;
    }

    //장착시 rigidbody, collier 끄기
    void DisablePhysicsAndScripts(Transform root)
    {
        Collider[] cols = root.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < cols.Length; i++)
            cols[i].enabled = false;

        Rigidbody[] bodies = root.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < bodies.Length; i++)
        {
            bodies[i].isKinematic = true;
            bodies[i].detectCollisions = false;
        }
    }

    //바닥에 버릴 때 다시 rigidbody, collier 복구
    void enablePhysicsAndScripts(Transform root)
    {
        
    }

    public void Fire(int moveFactor, float fireFactor)
    {
        Camera cam = Camera.main;

        // 1. 카메라 중앙에서 레이 발사
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        Vector3 targetPoint;

        if (Physics.Raycast(ray, out hit, 1000f))
        {
            // 맞은 지점
            targetPoint = hit.point;
        }
        else
        {
            // 안 맞으면, 카메라 전방 일정 거리(예: 100m)
            targetPoint = ray.GetPoint(1000f);
        }

        // 2. 총구에서 목표 지점을 향하는 방향 계산
        Vector3 dir = (targetPoint - firePos.position).normalized;
        float finalSpread = (float)(spread + moveFactor * 0.02 + fireFactor * 0.02);
        // 3. 퍼짐 적용
        Vector2 rand = Random.insideUnitCircle * finalSpread;
        dir += cam.transform.right * rand.x;  // 좌우 흔들림
        dir += cam.transform.up * rand.y;     // 상하 흔들림
        dir.Normalize();

        // 3. 총알 생성 (총구 위치에서, 목표 방향으로)
        GameObject bulletObj = Instantiate(bullet, firePos.position, Quaternion.LookRotation(dir));


        audio.PlayOneShot(fireSfx, 1.0f);
        StartCoroutine(ShowMuzzleFlash());
    }

    IEnumerator ShowMuzzleFlash()
    {
        muzzleFlash.material.mainTextureOffset = new Vector2(Random.Range(0, 2), Random.Range(0, 2)) * 0.5f;
        muzzleFlash.transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        muzzleFlash.transform.localScale = Vector3.one * Random.Range(0.5f, 1.0f);
        muzzleFlash.enabled = true;
        yield return new WaitForSeconds(0.2f);
        muzzleFlash.enabled = false;
    }
}
