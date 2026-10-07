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

        // Проверяем наличие XRSimpleInteractable
        var interactable = GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = gameObject.AddComponent<XRSimpleInteractable>();
            interactable.interactionLayers = -1;
            interactable.selectMode = InteractableSelectMode.Single;
        }

        // ФИКС: Обязательно динамически подписываемся на XR-события
        if (interactable != null)
        {
            interactable.hoverEntered.AddListener((args) => OnHoverEntered());
            interactable.hoverExited.AddListener((args) => OnHoverExited());
            interactable.selectEntered.AddListener((args) => OnSelectEntered());
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
            // ===== СКЛАД: берём компонент =====
            if (inventoryManager != null)
            {
                if (Time.time - lastPickupTime < pickupCooldown)
                {
                    Debug.Log($"[ComponentClickable] Слишком быстро, подождите...");
                    return;
                }

                lastPickupTime = Time.time;

                if (!inventoryManager.HasItem)
                {
                    inventoryManager.PickUp(gameObject);
                    Debug.Log($"[ComponentClickable] Взят компонент со склада: {gameObject.name}");
                    StartCoroutine(PickupFeedback());
                }
                else
                {
                    Debug.Log($"[ComponentClickable] В руке уже есть компонент: {inventoryManager.CurrentItem}.");
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

            // СЛУЧАЙ 1: СНИМАЕМ КОМПОНЕНТ (С микро-задержкой от телепортации)
            if (!hasItem && componentData.isInScene)
            {
                Vector3 hitPoint = GetHitPointFromController();
                // ФИКС: Запускаем корутину безопасного удаления вместо мгновенного скрытия
                StartCoroutine(DelayedHideComponent(componentId, hitPoint));
            }
            // СЛУЧАЙ 2: СТАВИМ НОВЫЙ КОМПОНЕНТ
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
                    Debug.Log($"[ComponentClickable] Неподходящий компонент! Нужен: {componentData.sceneTag}");
                }
            }
            else if (hasItem && componentData.isInScene)
            {
                Debug.Log($"[ComponentClickable] В слоте уже есть компонент. Сначала снимите его.");
            }
            else if (!hasItem && !componentData.isInScene)
            {
                Debug.Log($"[ComponentClickable] Слот пуст. Возьмите деталь со склада.");
            }
        }

        SetHighlight(false);
    }

    // ФИКС: Корутина, которая ждет долю секунды, чтобы XR-интеракт успел завершиться и луч не «пробивал» пол
    private System.Collections.IEnumerator DelayedHideComponent(string id, Vector3 hitPoint)
    {
        // Выключаем коллайдер прямо сейчас, чтобы предотвратить спам-клики, 
        // но даем кадру физики завершить обработку луча контроллера
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        yield return new WaitForSeconds(0.15f); // Короткая пауза (150 миллисекунд)

        if (brokenComponentManager != null && brokenComponentManager.TryHideComponent(id, hitPoint))
        {
            Debug.Log($"[ComponentClickable] Снят компонент: {id} (удалён)");
        }
    }

    private System.Collections.IEnumerator PickupFeedback()
    {
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

        // Складские предметы — доступны ВСЕГДА (их можно брать в любой момент)
        if (isWarehouseItem)
        {
            return true;
        }

        // Если вообще никакой режим ремонта не активен — взаимодействовать нельзя
        if (!cameraViewManager.IsRepairModeActive) return false;

        // СТРОГАЯ ПРОВЕРКА: принадлежит ли этот компонент именно ОТКРЫТОМУ сейчас серверу
        if (brokenComponentManager != null && brokenComponentManager.Components != null)
        {
            // Ищем данные этого компонента в общем списке менеджера
            var componentData = brokenComponentManager.Components.FirstOrDefault(c => c.componentId == componentId);
            if (componentData != null)
            {
                // Взаимодействие разрешено, ТОЛЬКО если номер стойки и сервера совпадает с текущим активным
                return cameraViewManager.CurrentRackId == componentData.nmbRack &&
                       cameraViewManager.CurrentServId == componentData.nmbServ;
            }
        }

        return false;
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