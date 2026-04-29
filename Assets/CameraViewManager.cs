using UnityEngine;

public class CameraViewManager : MonoBehaviour
{
    public static CameraViewManager Instance { get; private set; }

    [Header("Камера")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float smoothSpeed = 5f;

    private Transform viewpoint_R;
    private Transform viewpoint_T;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private bool isSpecialViewActive;
    private enum ViewType { None, R, T }
    private ViewType currentView = ViewType.None;

    private mouse playerMouse;

    public bool IsSpecialViewActive => isSpecialViewActive;
    public bool IsViewR => currentView == ViewType.R;
    public bool IsViewT => currentView == ViewType.T;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        playerMouse = FindObjectOfType<mouse>();

        if (mainCamera != null)
        {
            originalPosition = mainCamera.transform.position;
            originalRotation = mainCamera.transform.rotation;
        }
    }

    private void Update()
    {
        if (isSpecialViewActive && mainCamera != null)
        {
            mainCamera.transform.position = Vector3.Lerp(
                mainCamera.transform.position,
                targetPosition,
                Time.unscaledDeltaTime * smoothSpeed);

            mainCamera.transform.rotation = Quaternion.Lerp(
                mainCamera.transform.rotation,
                targetRotation,
                Time.unscaledDeltaTime * smoothSpeed);
        }

        UpdateCursorState();

        if (Input.GetKeyDown(KeyCode.Escape) && isSpecialViewActive)
            ExitSpecialView();
    }

    private void UpdateCursorState()
    {
        if (isSpecialViewActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void SetView(string viewType, int servId, int rackId)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        if (viewType == "R")
        {
            viewpoint_R = GameObject.Find($"ServerRack_{rackId}/ServerBox_{servId}/ViewPoint_R").transform;
            if (viewpoint_R == null) return;
            currentView = ViewType.R;
            targetPosition = viewpoint_R.position;
            targetRotation = viewpoint_R.rotation;
        }
        else if (viewType == "T")
        {
            viewpoint_T = GameObject.Find($"ServerRack_{rackId}/ServerBox_{servId}/ViewPoint_T").transform;
            if (viewpoint_T == null) return;
            currentView = ViewType.T;
            targetPosition = viewpoint_T.position;
            targetRotation = viewpoint_T.rotation;
        }
        else
        {
            return;
        }

        isSpecialViewActive = true;

        if (playerMouse != null)
            playerMouse.SetSpecialView(true);

        if (UIManager.Instance != null)
            UIManager.Instance.HideMenu();

        Time.timeScale = 1f;
        Debug.Log($"[CameraViewManager] Активирован режим {viewType}");
    }

    public void ExitSpecialView()
    {
        isSpecialViewActive = false;
        currentView = ViewType.None;

        if (playerMouse != null)
            playerMouse.SetSpecialView(false);

        if (mainCamera != null)
        {
            mainCamera.transform.position = originalPosition;
            mainCamera.transform.rotation = originalRotation;
        }

        UpdateCursorState();
        Debug.Log("[CameraViewManager] Выход из спецрежима");
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