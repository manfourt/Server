using UnityEngine;

public class ServerBoxController : MonoBehaviour
{
    [Header("Id стойки и сервера")]
    public int servId;
    public int rackId;

    private Open door;
    private CameraViewManager cameraViewManager;
    private bool isRepairModeActive = false;

    private void Awake()
    {
        door = GetComponent<Open>();
    }

    private void Start()
    {
        cameraViewManager = CameraViewManager.Instance;
        SetInternalComponentsVisible(false);
    }

    private void Update()
    {
        if (door == null) return;

        bool isOpen = door.IsOpen;

        SetInternalComponentsVisible(isOpen);

        // Когда дверь открылась - активируем режим ремонта
        if (isOpen && !isRepairModeActive && cameraViewManager != null && !cameraViewManager.IsRepairModeActive)
        {
            ActivateRepairMode();
        }

        // УБРАЛИ автоматическое закрытие двери при выходе из режима
        // Теперь дверь закрывается только через кнопку на контроллере
    }

    public void OnDoorOpened()
    {
        ActivateRepairMode();
    }

    public void OnDoorClosed()
    {
        if (cameraViewManager != null && cameraViewManager.IsRepairModeActive)
        {
            cameraViewManager.ExitRepairMode();
        }
        isRepairModeActive = false;
    }

    private void ActivateRepairMode()
    {
        if (cameraViewManager != null && !cameraViewManager.IsRepairModeActive)
        {
            cameraViewManager.EnterRepairMode(rackId, servId);
            isRepairModeActive = true;
            Debug.Log($"[ServerBoxController] Режим ремонта активирован для сервера {servId}");
        }
    }

    private void SetInternalComponentsVisible(bool visible)
    {
        BrokenComponentManager bcm = BrokenComponentManager.Instance;
        if (bcm == null) return;

        foreach (var comp in bcm.Components)
        {
            if (comp.nmbRack != rackId || comp.nmbServ != servId)
                continue;

            if (comp.kind == BrokenComponentManager.ComponentKind.HardDrive)
                continue;

            if (!comp.isInScene)
                continue;

            if (comp.sceneObject == null)
                continue;

            Renderer[] renderers = comp.sceneObject.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in renderers)
            {
                if (r != null)
                    r.enabled = visible;
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