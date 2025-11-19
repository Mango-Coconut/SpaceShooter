using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamController : MonoBehaviour
{
    public static CamController Instance { get; private set; }

    public CinemachineVirtualCamera vcamMain;
    public CinemachineVirtualCamera vcamCutscene;
    //public CinemachineVirtualCamera vcamAim;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // 씬 넘어가도 유지할 거면:
        // DontDestroyOnLoad(gameObject);
    }

    public void SetCutsceneCam(Transform t)
    {
        vcamCutscene.transform.position = t.position;
        vcamCutscene.transform.rotation = t.rotation;

        SetCam("Cutscene");
    }

    public void SetCam(string id)
    {
        vcamMain.Priority = id == "Main" ? 20 : 10;
        vcamCutscene.Priority = id == "Cutscene" ? 20 : 10;
        //vcamAim.Priority = id == "Aim" ? 20 : 10;
    }
}