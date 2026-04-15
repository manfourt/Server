// PlayerInteraction.cs
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 3f;
    public LayerMask interactableLayer;
    public Camera playerCamera;

    private ServerBoxController targetBox;
    private CameraViewManager cameraViewManager;
    private Outline currentOutline;

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
                Open openScript = hit.transform.GetComponent<Open>();

                // Отладка - проверяем, что нашли
                Debug.Log($"Open найден: {openScript != null}, openserverbox = {openScript?.openserverbox}");

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (targetBox != null && openScript != null && openScript.openserverbox)
                    {
                        Debug.Log("Вызов OpenBoxUI");
                        targetBox.OpenBoxUI();
                    }
                    else
                    {
                        Debug.Log($"Не могу открыть UI: targetBox={targetBox != null}, openScript={openScript != null}, openserverbox={openScript?.openserverbox}");
                    }
                }
            }
        }
    }

    public void ClearCurrentSelection()
    {
        if (currentOutline != null)
        {
            currentOutline.enabled = false;
            currentOutline = null;
        }
        targetBox = null;
    }
}