using UnityEngine;
using System.Linq;

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
    private InventoryManager inventoryManager;
    private Camera mainCamera;

    [Header("—ÍÎ‡‰")]
    [SerializeField] private bool isWarehouseItem = false;

    private float actionCooldown = 0f;
    private bool forceDisable = false;

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
        inventoryManager = InventoryManager.Instance;

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
        if (forceDisable)
        {
            SetHighlight(false);
            return;
        }

        if (actionCooldown > 0f)
        {
            actionCooldown -= Time.deltaTime;
            SetHighlight(false);
            return;
        }

        if (brokenComponentManager == null)
            brokenComponentManager = BrokenComponentManager.Instance ?? FindObjectOfType<BrokenComponentManager>();
        if (cameraViewManager == null)
            cameraViewManager = CameraViewManager.Instance ?? FindObjectOfType<CameraViewManager>();
        if (inventoryManager == null)
            inventoryManager = InventoryManager.Instance;

        if (brokenComponentManager == null || cameraViewManager == null)
        {
            SetHighlight(false);
            return;
        }

        if (isWarehouseItem && cameraViewManager.IsSpecialViewActive)
        {
            SetHighlight(false);
            return;
        }

        if (!isWarehouseItem && !cameraViewManager.IsSpecialViewActive)
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

        var data = brokenComponentManager.Components.FirstOrDefault(c => c.componentId == componentId);
        if (data == null && !isWarehouseItem)
        {
            SetHighlight(false);
            return;
        }

        bool isHovered = false;
        bool canInteract = false;
        Vector3 hitPoint = Vector3.zero;

        if (isWarehouseItem)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);
            ComponentClickable nearest = null;
            float nearestDist = Mathf.Infinity;
            foreach (var hit in hits)
            {
                var clicked = hit.collider.GetComponentInParent<ComponentClickable>();
                if (clicked != null && hit.distance < nearestDist)
                {
                    nearestDist = hit.distance;
                    nearest = clicked;
                }
            }

            if (nearest == this && nearestDist <= 2f)
            {
                RaycastHit firstHit;
                if (Physics.Raycast(ray, out firstHit, 3f))
                {
                    var firstClickable = firstHit.collider.GetComponentInParent<ComponentClickable>();
                    isHovered = (firstClickable == this);
                }
            }
            canInteract = isHovered;
            SetHighlight(isHovered);
        }
        else
        {
            bool hasItem = inventoryManager != null && inventoryManager.HasItem;
            bool isPresent = data.isInScene;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool hitThis = Physics.Raycast(ray, out hit, 100f) &&
                           (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform));

            hitPoint = hit.point;

            if (hitThis)
            {
                if (!hasItem && isPresent)
                {
                    canInteract = true;
                }
                else if (hasItem && !isPresent)
                {
                    string handTag = inventoryManager.CurrentItem.ToString();
                    if (handTag == data.sceneTag)
                        canInteract = true;
                }
            }
            isHovered = hitThis && canInteract;
            SetHighlight(isHovered);
        }

        // ===== ≈ƒ»Õ—“¬≈ÕÕ€… ¡ÀŒ   À» ¿ =====
        if (isHovered && Input.GetMouseButtonDown(0) && actionCooldown <= 0f)
        {
            if (isWarehouseItem)
            {
                inventoryManager?.PickUp(gameObject);
                SetHighlight(false);
                actionCooldown = 0.3f;
            }
            else
            {
                bool hasItem = inventoryManager != null && inventoryManager.HasItem;
                if (!hasItem)
                {
                    brokenComponentManager.TryHideComponent(componentId, hitPoint);
                    Debug.Log($"[ComponentClickable] ”‰‡Î∏Ì ÍÓÏÔÓÌÂÌÚ: {componentId}");
                }
                else
                {
                    bool success = brokenComponentManager.TryRestoreComponent(componentId);
                    if (success)
                    {
                        inventoryManager.ClearHand();
                        Debug.Log($"[ComponentClickable] ¬ÓÒÒÚ‡ÌÓ‚ÎÂÌ ÍÓÏÔÓÌÂÌÚ: {componentId}");
                    }
                }
                SetHighlight(false);
                actionCooldown = 0.5f;
            }
        }
    }

    private void SetHighlight(bool value)
    {
        if (outline != null)
            outline.enabled = value;
    }

    public void ForceDisableHighlight()
    {
        forceDisable = true;
        if (outline != null)
            outline.enabled = false;
    }

    public void EnableHighlight()
    {
        forceDisable = false;
    }

    private void OnDisable()
    {
        if (outline != null)
            outline.enabled = false;
        actionCooldown = 0f;
        forceDisable = false;
    }

    private void OnEnable()
    {
        forceDisable = false;
        actionCooldown = 0f;
        if (outline != null)
            outline.enabled = false;
    }
}