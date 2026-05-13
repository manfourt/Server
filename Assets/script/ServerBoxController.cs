using UnityEngine;

public class ServerBoxController : MonoBehaviour
{
    [Header("Id стойки и сервера")]
    public int servId;
    public int rackId;

    private Open door;

    private void Awake()
    {
        door = GetComponent<Open>();
    }

    private void Start()
    {
        UpdateViewpointsState();
        // При старте скрываем внутренние компоненты (дверца закрыта)
        SetInternalComponentsVisible(false);
    }

    private void Update()
    {
        UpdateViewpointsState();

        // Синхронизируем видимость компонентов с состоянием дверцы
        bool isOpen = IsDoorOpen();
        SetInternalComponentsVisible(isOpen);
    }

    private void UpdateViewpointsState()
    {
        bool isOpen = IsDoorOpen();
        if (CameraViewManager.Instance != null && CameraViewManager.Instance.IsSpecialViewActive)
        {
            SetViewpointsActive(false);
            return;
        }
        SetViewpointsActive(isOpen);
    }

    public bool IsDoorOpen()
    {
        return door != null && door.IsOpen;
    }

    private void SetViewpointsActive(bool active)
    {
        Transform vpR = transform.Find("ViewPoint_R");
        Transform vpT = transform.Find("ViewPoint_T");
        if (vpR != null && vpR.gameObject.activeSelf != active)
            vpR.gameObject.SetActive(active);
        if (vpT != null && vpT.gameObject.activeSelf != active)
            vpT.gameObject.SetActive(active);
    }

    /// <summary>
    /// Скрывает или показывает внутренние компоненты сервера (кроме HDD).
    /// </summary>
    private void SetInternalComponentsVisible(bool visible)
    {
        BrokenComponentManager bcm = BrokenComponentManager.Instance;
        if (bcm == null) return;

        foreach (var comp in bcm.Components)
        {
            // Только компоненты этого сервера
            if (comp.nmbRack != rackId || comp.nmbServ != servId)
                continue;

            // Пропускаем HDD — они видны всегда
            if (comp.kind == BrokenComponentManager.ComponentKind.HardDrive)
                continue;

            // Пропускаем отсутствующие (удалённые) компоненты
            if (!comp.isInScene)
                continue;

            if (comp.sceneObject == null)
                continue;

            // Включаем/выключаем рендереры
            MeshRenderer[] renderers = comp.sceneObject.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer mr in renderers)
            {
                mr.enabled = visible;
            }
        }
    }

    public void SetBoxColliderActive(bool active)
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = active;
    }
}