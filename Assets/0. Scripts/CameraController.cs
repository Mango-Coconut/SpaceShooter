using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform rifle;
    [SerializeField] Transform playerBody; // Player의 Transform
    [SerializeField] float mouseSensitivity = 200f;
    [SerializeField] float xRotationMin = -80f;
    [SerializeField] float xRotationMax = 80f;

    private float xRotation = 0f; // 현재 카메라 상하 회전값 누적

    IEnumerator Start()
    {
        xRotation = 0;
        yield return new WaitForSeconds(0.3f);
        xRotation = 0;
    }
    private void Update()
    {
        float mouseY = Cursor.visible == false ? Input.GetAxis("Mouse Y") : 0;

        // 상하 회전 누적 (마우스 Y는 반전)
        xRotation -= mouseY * mouseSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, xRotationMin, xRotationMax);

        // 카메라는 X축만 회전
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        rifle.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        // 플레이어 몸통의 좌우 회전은 PlayerController에서 처리
    }
}
