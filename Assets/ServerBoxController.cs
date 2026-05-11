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
        // Синхронизируем состояние вьюпоинтов при старте
        UpdateViewpointsState();
    }

    private void Update()
    {
        // Постоянно синхронизируем состояние вьюпоинтов с дверью
        UpdateViewpointsState();
    }

    private void UpdateViewpointsState()
    {
        bool isOpen = IsDoorOpen();

        // Если спецрежим активен — вьюпоинты выключаем
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

    /// <summary> Попытка ремонта (вызывается по клавише E) </summary>
    public void TryRepair()
    {
        if (!IsDoorOpen())
        {
            Debug.Log("Дверца закрыта, ремонт невозможен.");
            return;
        }

        if (InventoryManager.Instance == null || !InventoryManager.Instance.HasItem)
        {
            Debug.Log("Нет предмета в руке для ремонта.");
            return;
        }

        var heldItem = InventoryManager.Instance.CurrentItem;
        string neededTag = heldItem.ToString().Trim();

        BrokenComponentManager brokenManager = BrokenComponentManager.Instance;
        if (brokenManager == null) return;

        var comp = brokenManager.FindBrokenByType(neededTag, rackId, servId);
        if (comp == null)
        {
            Debug.Log("[Repair] Нет сломанных компонентов такого типа для ремонта.");
            return;
        }

        bool repaired = brokenManager.TryRepairComponent(comp.componentId);
        if (repaired)
        {
            InventoryManager.Instance.ClearHand();
            Debug.Log("[Repair] Успешно починен " + comp.componentId);
        }
    }
}