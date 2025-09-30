using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform rifle;
    [SerializeField] Transform playerBody; // Player의 Transform
    [SerializeField] float mouseSensitivity = 200f;
    [SerializeField] float xRotationMin = -50f;
    [SerializeField] float xRotationMax = 50f;
    float xRotation = 0f;

    IEnumerator Start()
    {
        xRotation = 0;
        yield return new WaitForSeconds(0.3f);
        xRotation = 0;
    }
    private void Update()
    {
        float mouseY = 0;
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            mouseY = Input.GetAxis("Mouse Y");
        }

        xRotation -= mouseY * mouseSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation, xRotationMin, xRotationMax);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
