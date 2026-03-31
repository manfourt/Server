using UnityEngine;
using UnityEngine.InputSystem;

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

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        playerMouse = FindObjectOfType<mouse>();

        originalPosition = mainCamera.transform.position;
        originalRotation = mainCamera.transform.rotation;

        Debug.Log("CameraViewManager инициализирован");
    }

    void Update()
    {
        // ВАЖНО: Проверяем нажатия R и T ВСЕГДА, когда меню открыто
        // Для этого проверяем Time.timeScale == 0 (меню открыто)
        if (Time.timeScale == 0) // Меню ящика открыто
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                Debug.Log("Нажата клавиша R - активируем вид R");
                SetView("R");
            }
            else if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("Нажата клавиша T - активируем вид T");
                SetView("T");
            }
        }

        // Плавное движение камеры если активен специальный режим
        if (isSpecialViewActive)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, targetPosition, Time.deltaTime * smoothSpeed);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, targetRotation, Time.deltaTime * smoothSpeed);

            // Проверка на близость к цели (для отладки)
            if (Vector3.Distance(mainCamera.transform.position, targetPosition) < 0.1f)
            {
                Debug.Log($"Камера достигла позиции для вида {currentViewType}");
            }
        }
    }

    public void SetView(string viewType)
    {
        Debug.Log($"SetView вызван с параметром: {viewType}");

        isSpecialViewActive = true;
        currentViewType = viewType;

        if (viewType == "R")
        {
            if (viewpoint_R == null)
            {
                Debug.LogError("viewpoint_R не назначен в инспекторе!");
                return;
            }
            targetPosition = viewpoint_R.position;
            targetRotation = viewpoint_R.rotation;
            Debug.Log($"Установлена цель для вида R: позиция {targetPosition}, поворот {targetRotation.eulerAngles}");
        }
        else if (viewType == "T")
        {
            if (viewpoint_T == null)
            {
                Debug.LogError("viewpoint_T не назначен в инспекторе!");
                return;
            }
            targetPosition = viewpoint_T.position;
            targetRotation = viewpoint_T.rotation;
            Debug.Log($"Установлена цель для вида T: позиция {targetPosition}, поворот {targetRotation.eulerAngles}");
        }

        if (playerMouse != null)
        {
            playerMouse.SetSpecialView(true);
            Debug.Log("Mouse.SetSpecialView(true) вызван");
        }
        else
        {
            Debug.LogError("playerMouse не найден!");
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideMenu();
            Debug.Log("Меню скрыто");
        }

        // Возобновляем время после выбора вида
        Time.timeScale = 1;
        Debug.Log($"Время возобновлено, isSpecialViewActive = {isSpecialViewActive}");
    }

    public void ExitSpecialView()
    {
        Debug.Log("ExitSpecialView вызван");
        isSpecialViewActive = false;
        currentViewType = "";

        if (playerMouse != null)
            playerMouse.SetSpecialView(false);

        mainCamera.transform.position = originalPosition;
        mainCamera.transform.rotation = originalRotation;

        Debug.Log("Камера возвращена в исходное положение");
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