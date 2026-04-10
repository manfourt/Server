using UnityEngine;

public class CameraViewManager : MonoBehaviour
{
    public static CameraViewManager Instance;

    [Header("Настройки камеры")]
    public Camera mainCamera;
    public Transform viewpoint_R;
    public Transform viewpoint_T;

    public float smoothSpeed = 5f;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isSpecialViewActive = false;
    private string currentViewType = "";  // "R" или "T"

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private mouse playerMouse;

    // Публичные свойства
    public bool IsSpecialViewActive => isSpecialViewActive;
    public bool IsViewR => currentViewType == "R";  // Проверка, активен ли вид R
    public bool IsViewT => currentViewType == "T";  // Проверка, активен ли вид T

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        playerMouse = FindObjectOfType<mouse>();

        // ВАЖНО: Добавляем PhysicsRaycaster на камеру для кликов
        if (mainCamera.GetComponent<UnityEngine.EventSystems.PhysicsRaycaster>() == null)
        {
            mainCamera.gameObject.AddComponent<UnityEngine.EventSystems.PhysicsRaycaster>();
            Debug.Log("Добавлен PhysicsRaycaster на камеру");
        }

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

        HandleCursorForClicking();
    }

    void HandleCursorForClicking()
    {
        // В режиме R показываем курсор для кликов по жёстким дискам
        if (isSpecialViewActive && IsViewR)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // В режиме T курсор скрываем (используем клавиши)
        else if (isSpecialViewActive && IsViewT)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        // В обычном режиме
        else if (!isSpecialViewActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void SetView(string viewType)
    {
        ClearAllOutlines();

        isSpecialViewActive = true;
        currentViewType = viewType;

        if (viewType == "R")
        {
            if (viewpoint_R == null) return;
            targetPosition = viewpoint_R.position;
            targetRotation = viewpoint_R.rotation;

            // Включаем курсор для кликов по жёстким дискам
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (viewType == "T")
        {
            if (viewpoint_T == null) return;
            targetPosition = viewpoint_T.position;
            targetRotation = viewpoint_T.rotation;

            // В режиме T курсор скрыт, удаление через клавиши
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (playerMouse != null)
            playerMouse.SetSpecialView(true);

        if (UIManager.Instance != null)
            UIManager.Instance.HideMenu();

        Time.timeScale = 1;

        Debug.Log($"Активирован режим {viewType}");
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

        Debug.Log("Выход из спецрежима");
    }

    public void UpdateOriginalPosition(Vector3 newPos, Quaternion newRot)
    {
        if (!isSpecialViewActive)
        {
            originalPosition = newPos;
            originalRotation = newRot;
        }
    }

    void ClearAllOutlines()
    {
        // Находим все объекты с Outline
        Outline[] allOutlines = FindObjectsOfType<Outline>();
        foreach (Outline outline in allOutlines)
        {
            outline.enabled = false;
        }

        // Также сбрасываем выделение у PlayerInteraction
        PlayerInteraction playerInteraction = FindObjectOfType<PlayerInteraction>();
        if (playerInteraction != null)
        {
            playerInteraction.ClearCurrentSelection();
        }

        Debug.Log("Все подсветки сброшены");
    }

    // Метод для включения/отключения кликабельности жёстких дисков
    void EnableHardDriveClickables(bool enable)
    {
        HardDriveClickable[] clickables = FindObjectsOfType<HardDriveClickable>();
        foreach (HardDriveClickable clickable in clickables)
        {
            clickable.enabled = enable;
            if (!enable)
            {
                // Отключаем подсветку
                Outline outline = clickable.GetComponent<Outline>();
                if (outline != null)
                    outline.enabled = false;
            }
        }
        Debug.Log($"Кликабельность жёстких дисков {(enable ? "включена" : "выключена")}");
    }
}