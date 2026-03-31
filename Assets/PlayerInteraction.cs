// PlayerInteraction.cs
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 3f;
    public LayerMask interactableLayer;
    public Camera playerCamera;

    private ServerBoxController targetBox;
    private CameraViewManager cameraViewManager;

    void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        cameraViewManager = FindObjectOfType<CameraViewManager>();
    }

    void Update()
    {
        if (Time.timeScale == 0) return;
        if (playerCamera == null) return;
        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            if (hit.transform == null) return;

            if (hit.transform.CompareTag("ServerBox"))
            {
                targetBox = hit.transform.GetComponent<ServerBoxController>();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (targetBox != null)
                    {
                        targetBox.OpenBoxUI();
                    }
                }
            }
        }
    }
}