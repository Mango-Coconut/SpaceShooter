using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(AudioSource))]
public class FireBullet : MonoBehaviour
{
    [SerializeField] private GameObject Bullet;
    [SerializeField] private Transform firePos;
    [SerializeField] AudioClip fireSfx;
    private new AudioSource audio;
    MeshRenderer muzzleFlash;

    public float spread = 0.05f;


    void Awake()
    {
        audio = GetComponent<AudioSource>();
        muzzleFlash = firePos.GetComponentInChildren<MeshRenderer>();
        muzzleFlash.enabled = false;
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
        Debug.Log(finalSpread);
        // 3. 퍼짐 적용
        Vector2 rand = Random.insideUnitCircle * finalSpread;
        dir += cam.transform.right * rand.x;  // 좌우 흔들림
        dir += cam.transform.up * rand.y;     // 상하 흔들림
        dir.Normalize();

        // 3. 총알 생성 (총구 위치에서, 목표 방향으로)
        GameObject bulletObj = Instantiate(Bullet, firePos.position, Quaternion.LookRotation(dir));


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
