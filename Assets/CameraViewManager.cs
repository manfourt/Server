// CameraViewManager.cs
using UnityEngine;

public class CameraViewManager : MonoBehaviour
{
    [Header("Настройки камеры")]
    public Camera mainCamera;
    public Transform viewpoint_R;
    public Transform viewpoint_T;

    public float smoothSpeed = 5f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isSpecialViewActive = false;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private string currentViewType = "";

    private mouse playerMouse;

    // Публичное свойство для доступа из других скриптов
    public bool IsSpecialViewActive => isSpecialViewActive;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        playerMouse = FindObjectOfType<mouse>();

        originalPosition = mainCamera.transform.position;
        originalRotation = mainCamera.transform.rotation;
    }

    void Update()
    {
        if (Time.timeScale == 0)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SetView("R");
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                SetView("T");
            }
        }

        if (isSpecialViewActive)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * smoothSpeed);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);
        }
    }

    public void SetView(string viewType)
    {
        isSpecialViewActive = true;
        currentViewType = viewType;

        if (viewType == "R")
        {
            if (viewpoint_R == null) return;
            targetPosition = viewpoint_R.position;
            targetRotation = viewpoint_R.rotation;
        }
        else if (viewType == "T")
        {
            if (viewpoint_T == null) return;
            targetPosition = viewpoint_T.position;
            targetRotation = viewpoint_T.rotation;
        }

        if (playerMouse != null)
            playerMouse.SetSpecialView(true);

        if (UIManager.Instance != null)
            UIManager.Instance.HideMenu();

        Time.timeScale = 1;
    }

    public void ExitSpecialView()
    {
        isSpecialViewActive = false;
        currentViewType = "";

        if (playerMouse != null)
            playerMouse.SetSpecialView(false);

        mainCamera.transform.position = originalPosition;
        mainCamera.transform.rotation = originalRotation;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UpdateOriginalPosition(Vector3 newPos, Quaternion newRot)
    {
        if (!isSpecialViewActive)
        {
            originalPosition = newPos;
            originalRotation = newRot;
        }
    }
}