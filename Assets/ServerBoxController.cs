using UnityEngine;

public class ServerBoxController : MonoBehaviour
{
    [Header("Настройки ящика")]
    public Transform[] emptySlots;
    public Transform teleportTargetPosition;

    private bool isOpen = false;
    private CameraViewManager cameraViewManager;

    void Start()
    {
        cameraViewManager = FindObjectOfType<CameraViewManager>();
        if (cameraViewManager == null)
        {
            Debug.LogError("CameraViewManager не найден в сцене!");
        }
    }

    public void TryPlaceItem(GameObject itemToPlace)
    {
        if (itemToPlace == null) return;

        if (emptySlots == null || emptySlots.Length == 0)
        {
            Debug.LogWarning("emptySlots не назначен, предмет не может быть размещен");
            return;
        }

        foreach (Transform slot in emptySlots)
        {
            if (slot != null && slot.childCount == 0)
            {
                itemToPlace.transform.SetParent(slot);
                itemToPlace.transform.localPosition = Vector3.zero;
                itemToPlace.transform.localRotation = Quaternion.identity;

                Rigidbody rb = itemToPlace.GetComponent<Rigidbody>();
                if (rb) rb.isKinematic = true;

                Collider col = itemToPlace.GetComponent<Collider>();
                if (col) col.enabled = false;

                Debug.Log($"Комплектующая установлена в слот {slot.name}!");
                return;
            }
        }
        Debug.Log("Нет свободных мест в боксе!");
    }

    public void TeleportBox()
    {
        if (teleportTargetPosition != null)
        {
            transform.position = teleportTargetPosition.position;
            transform.rotation = teleportTargetPosition.rotation;
            Debug.Log($"Ящик телепортирован");
        }
    }

    public void OpenBoxUI()
    {
        Debug.Log("OpenBoxUI вызван");
        isOpen = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMenu();
            Debug.Log("Меню показано, ожидаем нажатия R или T");
        }
        else
        {
            Debug.LogError("UIManager.Instance = null!");
        }

        Time.timeScale = 0; // Останавливаем время
    }

    public void CloseBoxUI()
    {
        Debug.Log("CloseBoxUI вызван");
        isOpen = false;

        // Возвращаем время только если не активен специальный режим просмотра
        if (cameraViewManager != null)
        {
            // Проверяем, активен ли специальный режим через рефлексию или публичное свойство
            // Пока просто проверяем через публичное поле (нужно добавить)
            // Если нет публичного поля, то время не восстанавливаем здесь
        }

        Time.timeScale = 1;
        Debug.Log("Время возобновлено");
    }
}