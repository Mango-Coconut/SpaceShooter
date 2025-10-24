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

    [SerializeField] float fireTimer = 0;
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
        //연발하지 않는 동안은 총알 fireSpread 줄이기
        if (currentData != null && fireTimer > currentData.fireDelay)
        {
            fireHeat = Math.Clamp(fireHeat - Time.deltaTime, 0, 1);
        }
    }

    public bool Equip(StoredItem item)
    {
        if (item == null || item.itemData == null)
        {
            Debug.LogWarning("PlayerWeapon.Equip() -> StoredItem null");
            Clear();
            return false;
        }

        WeaponItemData data = item.itemData as WeaponItemData;
        if (data == null)
        {
            Debug.LogWarning("PlayerWeapon.Equip() -> WeaponItemData null");
            return false;
        }

        if (currentData == data) return false;

        UnEquip();
        if (data.modelPrefab == null)
        {
            Debug.LogWarning("PlayerWeapon.Equip() -> modelPrefab is null");
            return false;
        }

        currentData = data;


        GameObject go = Instantiate(data.modelPrefab, this.transform);

        currentModel = go.transform;


        ApplyOffsets(data);
        DisablePhysicsAndScripts(currentModel);
        return true;
    }


    public void UnEquip()
    {
        Clear();
    }

    public void Clear()
    {
        if (currentModel != null)
        {
            Destroy(currentModel.gameObject);
            currentModel = null;
        }
        currentData = null;
    }

    /// <summary> 이동 여부에 따라 bullet fireSpread 계수 적용
    /// </summary>
    /// <param name="moveFactor"></param>
public void Fire(int moveFactor)
{
    if (fireTimer < currentData.fireDelay) return;

    Camera cam = Camera.main;
    if (cam == null) return;

    // 1) 화면 중앙 조준선에서 Ray (불필요 레이어 제외)
    Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

    Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit, 1000f)
        ? hit.point
        : ray.GetPoint(1000f);

    // 2) 기준 방향(한 번만 계산)
    Vector3 baseDir = (targetPoint - firePos.position).normalized;

    // 3) 최종 퍼짐 각도(도 단위) 계산
    //    SpreadAngle는 SO에서 "기본 퍼짐"으로 관리하고,
    //    이동/열 등에 따른 보정은 '도' 단위로 더해줌.
    float baseSpreadDeg = (float)currentData.SpreadAngle;
    float moveBonusDeg  = (float)(moveFactor * 0.5f);   // 필요에 맞게 계수 조정
    float heatBonusDeg  = (float)(fireHeat   * 0.3f);   // 필요에 맞게 계수 조정
    float finalSpreadDeg = baseSpreadDeg + moveBonusDeg + heatBonusDeg;

    // 4) 탄환 스폰 (각 탄환은 독립적으로 퍼짐 적용)
    for (int i = 0; i < currentData.PelletCount; i++)
    {
        // insideUnitCircle: 반지름 1 원 내 임의 벡터 → 최종 도수로 스케일
        Vector2 r = UnityEngine.Random.insideUnitCircle * finalSpreadDeg;

        // 카메라 기준 좌우/상하 회전으로 퍼짐(각도 회전이 벡터 덧셈보다 확실하게 퍼짐이 보임)
        Quaternion qPitch = Quaternion.AngleAxis(-r.y, cam.transform.right); // 위/아래
        Quaternion qYaw   = Quaternion.AngleAxis( r.x, cam.transform.up);    // 좌/우

        Vector3 shotDir = (qYaw * qPitch) * baseDir;

        GameObject bulletObj = Instantiate(
            currentData.bulletPrefab,
            firePos.position,
            Quaternion.LookRotation(shotDir)
        );
    }

    // 5) 반동(샷당 1회 적용 권장; 펠릿 수와 무관하게)
    // 카메라/조준축에 RecoilPerShot(도 단위)만큼 올려주거나, 네가 쓰는 반동 시스템에 던져라.
    // ApplyRecoil(currentData.RecoilPerShot);

    // 6) 사운드/이펙트/쿨타임
    fireTimer = 0f;
    audio.PlayOneShot(currentData.fireSound, 1.0f);
    StartCoroutine(ShowMuzzleFlash());
}




    void ApplyOffsets(WeaponItemData data)
    {
        if (currentModel == null) return;

        currentModel.localPosition = Vector3.zero;
        currentModel.localRotation = Quaternion.Euler(Vector3.zero);
        currentModel.localScale = Vector3.one;
    }

    public bool CanFire()
    {
        if (currentData == null) return false;
        if (currentModel == null) return false;
        return true;
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

    IEnumerator ShowMuzzleFlash()
    {
        muzzleFlash.material.mainTextureOffset = new Vector2(UnityEngine.Random.Range(0, 2), UnityEngine.Random.Range(0, 2)) * 0.5f;
        muzzleFlash.transform.localRotation = Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360));
        muzzleFlash.transform.localScale = Vector3.one * UnityEngine.Random.Range(0.5f, 1.0f);
        muzzleFlash.enabled = true;
        yield return new WaitForSeconds(0.17f);
        muzzleFlash.enabled = false;
    }
}
