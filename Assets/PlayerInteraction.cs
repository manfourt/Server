using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float interactRange = 3f;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] Camera playerCamera;

    CameraViewManager cameraViewManager;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
        cameraViewManager = CameraViewManager.Instance;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;
        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive) return;

        // Q Ц очистить руку
        if (Input.GetKeyDown(KeyCode.Q))
        {
            InventoryManager.Instance?.ClearHand();
        }
    }
}