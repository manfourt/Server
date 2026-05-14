using UnityEngine;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;

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

    [Header("Склад")]
    [SerializeField] private bool isWarehouseItem = false;

    private bool isHovered = false;
    private float lastPickupTime = 0f;
    private float pickupCooldown = 0.5f; // Защита от многократного взятия

    public void Initialize(string id, BrokenComponentManager.ComponentKind componentKind)
    {
        componentId = id;
        kind = componentKind;
        ApplyLayer();
    }

    private void Awake()
    {
        outline = GetComponent<Outline>();
    }

    private void Start()
    {
        cameraViewManager = CameraViewManager.Instance ?? FindObjectOfType<CameraViewManager>();
        brokenComponentManager = BrokenComponentManager.Instance ?? FindObjectOfType<BrokenComponentManager>();
        inventoryManager = InventoryManager.Instance;

        if (outline != null)
            outline.enabled = false;

        ApplyLayer();

        if (GetComponent<XRSimpleInteractable>() == null)
        {
            var interactable = gameObject.AddComponent<XRSimpleInteractable>();
            interactable.interactionLayers = -1;
            interactable.selectMode = InteractableSelectMode.Single;
        }

        Debug.Log($"[ComponentClickable] Инициализирован: {gameObject.name}, isWarehouseItem={isWarehouseItem}");
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

    public void OnHoverEntered()
    {
        bool canInteract = CanInteract();

        if (!canInteract) return;

        isHovered = true;
        SetHighlight(true);
    }

    public void OnHoverExited()
    {
        isHovered = false;
        SetHighlight(false);
    }

    public void OnSelectEntered()
    {
        bool canInteract = CanInteract();

        if (!canInteract)
        {
            Debug.Log($"[ComponentClickable] Нельзя взаимодействовать с {gameObject.name}");
            return;
        }

        if (isWarehouseItem)
        {
            // ===== СКЛАД: берём компонент (НЕ ИСЧЕЗАЕТ, просто добавляем в инвентарь) =====
            if (inventoryManager != null)
            {
                // Защита от многократного быстрого нажатия
                if (Time.time - lastPickupTime < pickupCooldown)
                {
                    Debug.Log($"[ComponentClickable] Слишком быстро, подождите...");
                    return;
                }

                lastPickupTime = Time.time;

                // Проверяем, есть ли место в инвентаре (если лимит 1 предмет)
                if (!inventoryManager.HasItem)
                {
                    inventoryManager.PickUp(gameObject);
                    // Объект НЕ исчезает! gameObject.SetActive(false) - НЕ ВЫЗЫВАЕМ
                    Debug.Log($"[ComponentClickable] Взят компонент со склада: {gameObject.name} (объект остался на месте)");

                    // Визуальный фидбек - кратковременная подсветка
                    StartCoroutine(PickupFeedback());
                }
                else
                {
                    Debug.Log($"[ComponentClickable] В руке уже есть компонент: {inventoryManager.CurrentItem}. Сначала поставьте его или очистите руку.");
                }
            }
        }
        else
        {
            // ===== КОМПОНЕНТ В СЕРВЕРЕ =====
            var componentData = brokenComponentManager?.Components.FirstOrDefault(c => c.componentId == componentId);
            if (componentData == null)
            {
                Debug.LogWarning($"[ComponentClickable] Нет данных для {componentId}");
                return;
            }

            bool hasItem = inventoryManager != null && inventoryManager.HasItem;

            Debug.Log($"[ComponentClickable] Серверный компонент: id={componentId}, hasItem={hasItem}, isInScene={componentData.isInScene}");

            // СЛУЧАЙ 1: СНИМАЕМ ЛЮБОЙ КОМПОНЕНТ (НЕ запоминаем в инвентарь)
            if (!hasItem && componentData.isInScene)
            {
                Vector3 hitPoint = GetHitPointFromController();

                if (brokenComponentManager.TryHideComponent(componentId, hitPoint))
                {
                    Debug.Log($"[ComponentClickable] Снят компонент: {componentId} (удалён)");
                }
            }
            // СЛУЧАЙ 2: СТАВИМ НОВЫЙ КОМПОНЕНТ из инвентаря
            else if (hasItem && !componentData.isInScene)
            {
                string handItemTag = inventoryManager.CurrentItem.ToString();
                Debug.Log($"[ComponentClickable] Пытаемся поставить {handItemTag} в слот {componentData.sceneTag}");

                if (handItemTag == componentData.sceneTag)
                {
                    if (brokenComponentManager.TryRestoreComponent(componentId))
                    {
                        inventoryManager.ClearHand();
                        Debug.Log($"[ComponentClickable] Установлен компонент: {componentId}");
                    }
                }
                else
                {
                    Debug.Log($"[ComponentClickable] Неподходящий компонент! Нужен: {componentData.sceneTag}, в руке: {handItemTag}");
                }
            }
            else if (hasItem && componentData.isInScene)
            {
                Debug.Log($"[ComponentClickable] В слоте уже есть компонент. Сначала снимите его (рука должна быть пуста).");
            }
            else if (!hasItem && !componentData.isInScene)
            {
                Debug.Log($"[ComponentClickable] Слот пуст, но рука пуста. Сначала возьмите компонент со склада.");
            }
        }

        SetHighlight(false);
    }

    private System.Collections.IEnumerator PickupFeedback()
    {
        // Визуальный фидбек при взятии со склада
        Color originalColor = Color.white;
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            originalColor = renderer.material.color;
            renderer.material.color = Color.green;
            yield return new WaitForSeconds(0.2f);
            renderer.material.color = originalColor;
        }
    }

    private Vector3 GetHitPointFromController()
    {
        var rayInteractors = FindObjectsOfType<XRRayInteractor>();

        foreach (var rayInteractor in rayInteractors)
        {
            if (rayInteractor.interactablesSelected.Count > 0)
            {
                if (rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit hit))
                {
                    return hit.point;
                }
            }
        }

        return transform.position;
    }

    private void SetHighlight(bool value)
    {
        if (outline != null)
            outline.enabled = value;
    }

    private bool CanInteract()
    {
        if (cameraViewManager == null)
        {
            cameraViewManager = CameraViewManager.Instance ?? FindObjectOfType<CameraViewManager>();
            if (cameraViewManager == null) return false;
        }

        // Складские предметы - доступны ВСЕГДА (и в режиме ремонта, и вне)
        if (isWarehouseItem)
        {
            return true; // Всегда можно взять со склада
        }

        // Компоненты в сервере - только в режиме ремонта
        return cameraViewManager.IsRepairModeActive;
    }

    private void OnEnable()
    {
        if (outline != null)
            outline.enabled = false;
        isHovered = false;
    }

    private void OnDisable()
    {
        if (outline != null)
            outline.enabled = false;
        isHovered = false;
    }
}