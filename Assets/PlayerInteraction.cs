using UnityEngine;
using System.Linq;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float interactRange = 3f;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] LayerMask warehouseLayer;
    [SerializeField] Camera playerCamera;

    CameraViewManager cameraViewManager;
    ServerBoxController box;

    bool waitingForViewChoice = false;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        cameraViewManager = CameraViewManager.Instance;
    }

    void Update()
    {
        // R/T проверяем ВСЕГДА
        CheckViewSelection();

        // блокируем только движение/взаимодействие
        if (Time.timeScale == 0f)
            return;

        if (cameraViewManager != null &&
           cameraViewManager.IsSpecialViewActive)
            return;

        CheckServerInteraction();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            InventoryManager.Instance?.ClearHand();
        }
    }

    void CheckServerInteraction()
    {
        Ray ray =
            playerCamera.ViewportPointToRay(
                new Vector3(.5f, .5f, 0)
            );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactRange,
            interactableLayer))
        {
            box = hit.collider.GetComponentInParent<ServerBoxController>();

            if (box != null)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // === ЕСЛИ В РУКЕ ЕСТЬ ПРЕДМЕТ ЧИНИМ СЕРВЕР ===
                    if (InventoryManager.Instance != null && InventoryManager.Instance.HasItem)
                    {
                        TryRepairInServer(box);
                        return;
                    }

                    // === ИНАЧЕ ОТКРЫВАЕМ ===
                    if (box.IsDoorOpen())
                    {
                        box.OpenBoxUI();

                        waitingForViewChoice = true;

                        Debug.Log("Нажмите R или T");
                    }
                }
            }
        }
    }

    void CheckViewSelection()
    {
        if (!waitingForViewChoice)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R detected");

            cameraViewManager.SetView("R", box.servId, box.rackId);

            waitingForViewChoice = false;

            if (UIManager.Instance != null)
                UIManager.Instance.HideMenu();

            Time.timeScale = 1f;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("T detected");

            cameraViewManager.SetView("T", box.servId, box.rackId);

            waitingForViewChoice = false;

            if (UIManager.Instance != null)
                UIManager.Instance.HideMenu();

            Time.timeScale = 1f;
        }
    }
    void TryRepairInServer(ServerBoxController box)
    {
        Debug.Log("[Repair] Попытка ремонта через сервер");

        if (InventoryManager.Instance == null)
        {
            Debug.Log("[Repair] Inventory NULL");
            return;
        }

        var heldItem = InventoryManager.Instance.CurrentItem;
        Debug.Log("[Repair] В руке: " + heldItem);

        var brokenManager = BrokenComponentManager.Instance;
        if (brokenManager == null)
        {
            Debug.Log("[Repair] BrokenManager NULL");
            return;
        }

        // переводим тип в тег
        string neededTag = heldItem.ToString().Trim();
        Debug.Log("[Repair] Ищем тип: '" + neededTag + "'");

        Debug.Log("[Repair] Ищем сломанный компонент типа: " + neededTag);

        var comp = brokenManager.FindBrokenByType(neededTag, box.rackId, box.servId);

        if (comp == null)
        {
            Debug.Log("[Repair] Нет сломанных компонентов такого типа");
            return;
        }

        Debug.Log("[Repair] Найден: " + comp.componentId);

        bool repaired = brokenManager.TryRepairComponent(comp.componentId);

        Debug.Log("[Repair] Результат ремонта: " + repaired);

        if (repaired)
        {
            InventoryManager.Instance.ClearHand();
            Debug.Log("[Repair] УСПЕШНО ПОЧИНЕНО");
        }
    }
}