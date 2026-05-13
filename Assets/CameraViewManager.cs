using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CameraViewManager : MonoBehaviour
{
    public static CameraViewManager Instance { get; private set; }

    [Header("Объекты")]
    [Tooltip("Перетащите сюда объект 'VRPlayer' (бывший XR Origin) из вашей сцены")]
    [SerializeField] private Transform xrOrigin;
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

    private int currentRackId;
    private int currentServId;

    public bool IsSpecialViewActive => isSpecialViewActive;
    public bool IsViewR => currentView == ViewType.R;
    public bool IsViewT => currentView == ViewType.T;

    // Ссылки на компоненты перемещения
    private TeleportationProvider teleportationProvider;
    private ActionBasedContinuousMoveProvider continuousMoveProvider;
    private ActionBasedSnapTurnProvider snapTurnProvider;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Автоматический поиск, если не задано в инспекторе
        if (xrOrigin == null)
        {
            var origin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
            if (origin != null) xrOrigin = origin.transform;
            else Debug.LogError("[CameraViewManager] Не найден XR Origin на сцене! Перетащите его в инспектор.");
        }

        if (xrOrigin != null)
        {
            // Получаем компоненты для управления передвижением
            teleportationProvider = xrOrigin.GetComponent<TeleportationProvider>();
            continuousMoveProvider = xrOrigin.GetComponent<ActionBasedContinuousMoveProvider>();
            snapTurnProvider = xrOrigin.GetComponent<ActionBasedSnapTurnProvider>();
        }
    }

    private void Update()
    {
        // Плавное перемещение к точке обзора
        if (isSpecialViewActive && xrOrigin != null)
        {
            xrOrigin.position = Vector3.Lerp(xrOrigin.position, targetPosition, Time.unscaledDeltaTime * smoothSpeed);
            xrOrigin.rotation = Quaternion.Slerp(xrOrigin.rotation, targetRotation, Time.unscaledDeltaTime * smoothSpeed);
        }
    }

    public void SetView(string viewType, int servId, int rackId)
    {
        if (xrOrigin == null) return;

        // Сохраняем позицию, куда нужно будет вернуться
        originalPosition = xrOrigin.position;
        originalRotation = xrOrigin.rotation;

        if (viewType == "R")
        {
            viewpoint_R = GameObject.Find($"ServerRack_{rackId}/ServerBox_{servId}/ViewPoint_R")?.transform;
            if (viewpoint_R == null) return;
            currentView = ViewType.R;
            targetPosition = viewpoint_R.position;
            targetRotation = viewpoint_R.rotation;
        }
        else if (viewType == "T")
        {
            viewpoint_T = GameObject.Find($"ServerRack_{rackId}/ServerBox_{servId}/ViewPoint_T")?.transform;
            if (viewpoint_T == null) return;
            currentView = ViewType.T;
            targetPosition = viewpoint_T.position;
            targetRotation = viewpoint_T.rotation;
        }
        else
        {
            return;
        }

        currentRackId = rackId;
        currentServId = servId;

        // Отключаем стандартное перемещение игрока
        SetLocomotionActive(false);

        BrokenComponentManager.Instance?.SetCollidersForViewMode(
            viewType == "R" ? BrokenComponentManager.ComponentKind.HardDrive : BrokenComponentManager.ComponentKind.Normal,
            rackId,
            servId
        );

        isSpecialViewActive = true;
        Time.timeScale = 1f; // На случай если была пауза
        Debug.Log($"[CameraViewManager] Установлен вид {viewType} для стойки {rackId}, сервера {servId}");
    }

    public void ExitSpecialView()
    {
        if (!isSpecialViewActive) return;

        isSpecialViewActive = false;
        currentView = ViewType.None;

        BrokenComponentManager.Instance?.ResetAllColliders();

        // Возвращаем игрока на исходную позицию
        if (xrOrigin != null)
        {
            xrOrigin.position = originalPosition;
            xrOrigin.rotation = originalRotation;
        }

        // Включаем перемещение обратно
        SetLocomotionActive(true);

        Debug.Log("[CameraViewManager] Выход из специального вида.");
    }

    // Вспомогательный метод для вкл/выкл перемещения
    private void SetLocomotionActive(bool active)
    {
        if (teleportationProvider != null) teleportationProvider.enabled = active;
        if (continuousMoveProvider != null) continuousMoveProvider.enabled = active;
        if (snapTurnProvider != null) snapTurnProvider.enabled = active;
    }
}
