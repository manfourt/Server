using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Outline))]
public class ComponentClickable : MonoBehaviour
{
    public string GetComponentId() => componentId;
    [SerializeField] private string componentId;
    [SerializeField] private BrokenComponentManager.ComponentKind kind;

    private Outline outline;
    private CameraViewManager cameraViewManager;
    private BrokenComponentManager brokenComponentManager;
    private Camera mainCamera;

    [Header("Склад")]
    [SerializeField] private bool isWarehouseItem = false;

    public void Initialize(string id, BrokenComponentManager.ComponentKind componentKind)
    {
        componentId = id;
        kind = componentKind;
        ApplyLayer();
    }

    private void Awake()
    {
        outline = GetComponent<Outline>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        cameraViewManager = CameraViewManager.Instance ?? FindObjectOfType<CameraViewManager>();
        brokenComponentManager = BrokenComponentManager.Instance ?? FindObjectOfType<BrokenComponentManager>();

        if (outline != null)
            outline.enabled = false;

        ApplyLayer();
    }

    private void ApplyLayer()
    {
        if (kind == BrokenComponentManager.ComponentKind.HardDrive)
        {
            int layer = LayerMask.NameToLayer("BrokenHardDrive");
            if (layer >= 0) gameObject.layer = layer;
        }
        else
        {
            int layer = LayerMask.NameToLayer("BrokenCompnent");
            if (layer >= 0) gameObject.layer = layer;
        }
    }

    private void Update()
    {
        if (brokenComponentManager == null)
            brokenComponentManager = BrokenComponentManager.Instance ?? FindObjectOfType<BrokenComponentManager>();

        if (cameraViewManager == null)
            cameraViewManager = CameraViewManager.Instance ?? FindObjectOfType<CameraViewManager>();

        if (brokenComponentManager == null || cameraViewManager == null)
        {
            SetHighlight(false);
            return;
        }

        // В специальном режиме просмотра складские предметы недоступны
        if (isWarehouseItem && cameraViewManager.IsSpecialViewActive)
        {
            SetHighlight(false);
            return;
        }

        // Для не-складских предметов требуется CanInteract
        if (!isWarehouseItem && !brokenComponentManager.CanInteract(kind))
        {
            SetHighlight(false);
            return;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera == null)
        {
            SetHighlight(false);
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);

        ComponentClickable nearestClickable = null;
        float nearestDistance = Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            ComponentClickable clicked = hit.collider.GetComponentInParent<ComponentClickable>();
            if (clicked != null && hit.distance < nearestDistance)
            {
                nearestDistance = hit.distance;
                nearestClickable = clicked;
            }
        }

        bool hitThisObject = (nearestClickable == this);

        if (hitThisObject)
        {
            SetHighlight(true);

            if (Input.GetMouseButtonDown(0))
            {
                // === Ветка для склада ===
                if (isWarehouseItem)
                {
                    // Пытаемся взять предмет
                    InventoryManager.Instance?.PickUp(gameObject);
                    SetHighlight(false);
                }
                // === Ветка для компонентов сервера ===
                else
                {
                    bool success = brokenComponentManager.TryHideComponent(componentId);

                    if (success)
                        SetHighlight(false);
                }
            }
        }
        else
        {
            SetHighlight(false);
        }
    }


    private void SetHighlight(bool value)
    {
        if (outline != null)
            outline.enabled = value;
    }
}