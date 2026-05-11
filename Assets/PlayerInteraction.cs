using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float interactRange = 3f;
    [SerializeField] LayerMask interactableLayer; // слой ServerBox
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

        //  лавиша E Ц попытка ремонта
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteractWithServer(box => box.TryRepair());
        }

        // Q Ц очистить руку
        if (Input.GetKeyDown(KeyCode.Q))
        {
            InventoryManager.Instance?.ClearHand();
        }
    }

    private void TryInteractWithServer(System.Action<ServerBoxController> action)
    {
        if (playerCamera == null) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            ServerBoxController box = hit.collider.GetComponent<ServerBoxController>();
            if (box == null)
                box = hit.collider.GetComponentInParent<ServerBoxController>();

            if (box != null)
                action?.Invoke(box);
        }
    }
}