using UnityEngine;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Outline))]
public class ComponentClickable : MonoBehaviour
{
    [SerializeField] private string componentId;
    [SerializeField] private BrokenComponentManager.ComponentKind kind;
    [SerializeField] private bool isWarehouseItem = false;

    private Outline outline;
    private CameraViewManager cameraViewManager;
    private BrokenComponentManager brokenComponentManager;
    private InventoryManager inventoryManager;

    public void Initialize(string id, BrokenComponentManager.ComponentKind componentKind)
    {
        componentId = id;
        kind = componentKind;
    }

    private void Start()
    {
        outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;

        cameraViewManager = CameraViewManager.Instance;
        brokenComponentManager = BrokenComponentManager.Instance;
        inventoryManager = InventoryManager.Instance;
    }

    // Вызывается, когда луч контроллера наводится на объект
    public void OnHoverEntered()
    {
        if (CanInteract())
        {
            SetHighlight(true);
        }
    }

    // Вызывается, когда луч контроллера уходит с объекта
    public void OnHoverExited()
    {
        SetHighlight(false);
    }

    // Вызывается при "клике" (нажатии триггера)
    public void OnSelect()
    {
        if (!CanInteract()) return;

        if (isWarehouseItem)
        {
            inventoryManager.PickUp(gameObject);
        }
        else
        {
            var componentData = brokenComponentManager.Components.FirstOrDefault(c => c.componentId == componentId);
            if (componentData == null) return;

            // Если в руке ничего нет, а компонент на месте и сломан
            if (!inventoryManager.HasItem && componentData.isInScene && componentData.isBroken)
            {
                if (brokenComponentManager.TryHideComponent(componentId, transform.position))
                {
                    inventoryManager.PickUp(gameObject);
                }
            }
            // Если в руке есть нужный компонент и слот пуст
            else if (inventoryManager.HasItem && !componentData.isInScene)
            {
                // Проверяем, совпадает ли тип предмета в руке с типом этого слота
                string handItemTag = InventoryManager.TagToItemType(inventoryManager.CurrentItem.ToString()).ToString();
                if (handItemTag == componentData.sceneTag)
                {
                    if (brokenComponentManager.TryRestoreComponent(componentId))
                    {
                        inventoryManager.ClearHand();
                    }
                }
            }
        }

        SetHighlight(false);
    }

    private void SetHighlight(bool value)
    {
        if (outline != null)
        {
            outline.enabled = value;
        }
    }

    // Проверка, можно ли сейчас взаимодействовать с этим объектом
    private bool CanInteract()
    {
        if (cameraViewManager == null) cameraViewManager = CameraViewManager.Instance;
        if (inventoryManager == null) inventoryManager = InventoryManager.Instance;

        // Если это предмет на складе
        if (isWarehouseItem)
        {
            // Взаимодействовать можно только в режиме свободного перемещения
            return !cameraViewManager.IsSpecialViewActive;
        }

        // Если это компонент в сервере
        if (!cameraViewManager.IsSpecialViewActive) return false;

        bool isCorrectView = (kind == BrokenComponentManager.ComponentKind.HardDrive && cameraViewManager.IsViewR) ||
                             (kind == BrokenComponentManager.ComponentKind.Normal && cameraViewManager.IsViewT);

        return isCorrectView;
    }
}
