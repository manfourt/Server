using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Для ServerBox")]
    public float interactRange = 3f;
    public LayerMask interactableLayer;

    [Header("Для подбора предметов")]
    public Transform handParent;          // Пустой объект перед камерой - НАЗНАЧИТЬ В ИНСПЕКТОРЕ!
    public float pickupRange = 3f;
    public LayerMask pickupLayer;         // Слой Pickupable - НАЗНАЧИТЬ В ИНСПЕКТОРЕ!

    private Camera playerCamera;
    private CameraViewManager cameraViewManager;
    private ServerBoxController targetBox;
    private PickupItem currentItem;
    private Outline currentOutline;

    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null) playerCamera = GetComponent<Camera>();
        cameraViewManager = FindObjectOfType<CameraViewManager>();
    }

    void Update()
    {
        // Блокировка при паузе или спецрежиме
        if (Time.timeScale == 0) return;
        if (playerCamera == null) return;
        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive) return;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // ===== 1. Взаимодействие с ServerBox =====
        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            if (hit.transform.CompareTag("ServerBox"))
            {
                targetBox = hit.transform.GetComponent<ServerBoxController>();
                Open openScript = hit.transform.GetComponent<Open>();

                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (targetBox != null && openScript != null && openScript.openserverbox)
                    {
                        targetBox.OpenBoxUI();
                    }
                    else
                    {
                        Debug.Log("Сначала откройте ящик левой кнопкой мыши");
                    }
                }
                return; // Если смотрим на ServerBox, не проверяем подбор
            }
        }

        // ===== 2. Подбор предметов со склада =====
        if (Physics.Raycast(ray, out hit, pickupRange, pickupLayer))
        {
            PickupItem item = hit.collider.GetComponent<PickupItem>();
            if (item != null)
            {
                // Подсветка
                item.ShowOutline(true);
                if (currentItem != null && currentItem != item)
                {
                    currentItem.ShowOutline(false);
                }
                currentItem = item;

                // Создание копии по F
                if (Input.GetKeyDown(KeyCode.F))
                {
                    // Если уже держим предмет - уничтожаем его
                    if (handParent.childCount > 0)
                    {
                        PickupItem oldItem = handParent.GetComponentInChildren<PickupItem>();
                        if (oldItem != null) Destroy(oldItem.gameObject);
                    }

                    // Создаём копию в руке
                    PickupItem newItem = item.CreateCopy(handParent);
                    Debug.Log($"В руке: {newItem.componentId}");
                }
                return;
            }
        }

        // Сброс подсветки
        if (currentItem != null)
        {
            currentItem.ShowOutline(false);
            currentItem = null;
        }
    }

    // Возвращает текущий предмет в руке
    public PickupItem GetHeldItem()
    {
        if (handParent != null && handParent.childCount > 0)
            return handParent.GetComponentInChildren<PickupItem>();
        return null;
    }

    // Очистка выделения (вызывается из CameraViewManager)
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