using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float interactRange = 3f;
    [SerializeField] LayerMask interactableLayer;
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
}