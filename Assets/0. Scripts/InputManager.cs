using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Vector2 Move { get; private set; }
    public Vector2 Look { get; private set; }

    public event System.Action OnFire;
    public event System.Action OnPick;
    public event System.Action OnToggleInventory;
    public event System.Action OnEsc;

    [SerializeField] float mouseSensitivity = 1f;

    bool LookEnabled => Cursor.lockState == CursorLockMode.Locked;

    public static InputManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 이미 있으면 자기 삭제
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 씬 전환에도 유지
    }
    void Update()
    {
        // 상태 폴링
        Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Look = LookEnabled ? new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSensitivity
                           : Vector2.zero;

        // 액션 이벤트
        if (Input.GetMouseButton(0)) OnFire?.Invoke();
        if (Input.GetKeyDown(KeyCode.F)) OnPick?.Invoke();
        if (Input.GetKeyDown(KeyCode.I)) OnToggleInventory?.Invoke();
        if (Input.GetKeyDown(KeyCode.Escape)) OnEsc?.Invoke();
    }
}
