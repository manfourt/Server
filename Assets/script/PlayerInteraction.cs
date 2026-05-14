using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float interactRange = 3f;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] Camera playerCamera;

    private CameraViewManager cameraViewManager;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
        cameraViewManager = CameraViewManager.Instance;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (cameraViewManager != null && cameraViewManager.IsRepairModeActive) return;

        // Очистка по клавише Q (клавиатура)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ClearInventory();
        }

        // Очистка по кнопке Menu на контроллере
        if (Input.GetButtonDown("Cancel"))
        {
            ClearInventory();
        }

        // Очистка по кнопке A/X на контроллере (можно заменить)
        if (Input.GetButtonDown("Submit"))
        {
            ClearInventory();
        }

        // Очистка по кнопке B/Y на контроллере
        if (Input.GetButtonDown("Fire2"))
        {
            ClearInventory();
        }
    }

    private void ClearInventory()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearHand();
            Debug.Log("[PlayerInteraction] Инвентарь очищен");
        }
    }
}