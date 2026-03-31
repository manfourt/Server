using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float interactRange = 3f;
    public LayerMask interactableLayer;
    public Camera playerCamera; 

    private GameObject currentHeldItem;
    private ServerBoxController targetBox;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        // Блокируем взаимодействие, если меню открыто
        if (Time.timeScale == 0) return;

        // Луч должен исходить из центра камеры
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Рисуем луч для визуальной проверки
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.red);

        // Проверка на взятие предмета (E)
        if (Physics.Raycast(ray, out hit, interactRange, interactableLayer))
        {
            // Взять предмет (E)
            if (hit.transform.CompareTag("Sklad") && currentHeldItem == null)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    PickupItem(hit.transform.gameObject);
                }
            }

            // Взаимодействие с ящиком
            if (hit.transform.CompareTag("ServerBox"))
            {
                targetBox = hit.transform.GetComponent<ServerBoxController>();

                // Положить предмет (Q)
                if (Input.GetKeyDown(KeyCode.Q) && currentHeldItem != null)
                {
                    PlaceItemInBox();
                }

                // Открыть меню ящика (E), если руки пусты
                if (Input.GetKeyDown(KeyCode.E) && currentHeldItem == null)
                {
                    targetBox.OpenBoxUI();
                }
            }
        }
    }


    void PickupItem(GameObject item)
    {
        currentHeldItem = item;
        item.transform.SetParent(transform);
        item.transform.localPosition = new Vector3(0.5f, -0.5f, 1f);
        item.transform.localRotation = Quaternion.identity;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        // Отключаем коллайдер, чтобы предмет не мешал
        Collider col = item.GetComponent<Collider>();
        if (col) col.enabled = false;

        Debug.Log("Предмет взят: " + item.name);
    }

    void PlaceItemInBox()
    {
        if (targetBox != null && currentHeldItem != null)
        {
            targetBox.TryPlaceItem(currentHeldItem);
            currentHeldItem = null;
        }
    }

}