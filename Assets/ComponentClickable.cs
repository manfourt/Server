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

            // Проверяем что это наш предмет, в пределах дистанции, и нет стены между нами
            if (nearest == this && nearestDist <= 2f)
            {
                // Дополнительный рейкаст: проверяем что первый объект на пути — это складской предмет
                RaycastHit firstHit;
                if (Physics.Raycast(ray, out firstHit, 3f))
                {
                    var firstClickable = firstHit.collider.GetComponentInParent<ComponentClickable>();
                    isHovered = (firstClickable == this);
                }
            }
            canInteract = isHovered;
        }
        else
        {
            // Луч из позиции курсора
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            bool hitThis = Physics.Raycast(ray, out hit, 100f) &&
                           (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform));

            bool hasItem = inventoryManager != null && inventoryManager.HasItem;
            bool isPresent = data.isInScene;

            if (hitThis)
            {
                if (!hasItem && isPresent)
                {
                    // Рука пуста, компонент на месте — можно удалить
                    canInteract = true;
                }
                else if (hasItem && !isPresent)
                {
                    // Рука занята, место пустое — можно вставить
                    string handTag = inventoryManager.CurrentItem.ToString();
                    if (handTag == data.sceneTag)
                        canInteract = true;
                }
            }

            // ВСЕГДА показываем рамку для пустых мест (независимо от наведения)
            if (!isPresent)
            {
                SetHighlight(true);
            }

            // Если навели — используем canInteract для клика
            isHovered = hitThis && canInteract;
        }

        SetHighlight(isHovered);

        if (isHovered && Input.GetMouseButtonDown(0))
        {
            if (isWarehouseItem)
            {
                inventoryManager?.PickUp(gameObject);
                SetHighlight(false);
            }
            else
            {
                bool hasItem = inventoryManager != null && inventoryManager.HasItem;
                if (!hasItem)
                {
                    bool success = brokenComponentManager.TryHideComponent(componentId);
                    if (success)
                        Debug.Log($"[ComponentClickable] Удалён компонент: {componentId}");
                }
                else
                {
                    bool success = brokenComponentManager.TryRestoreComponent(componentId);
                    if (success)
                    {
                        inventoryManager.ClearHand();
                        Debug.Log($"[ComponentClickable] Восстановлен компонент: {componentId}");
                    }
                    else
                    {
                        Debug.Log($"[ComponentClickable] Не удалось восстановить: {componentId}");
                    }
                }
                SetHighlight(false);
            }
        }
    }

    private void SetHighlight(bool value)
    {
        if (outline != null)
            outline.enabled = value;
    }
}