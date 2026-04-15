using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public string componentId;      // ID компонента (Fan1, PSU, 1 и т.д.)
    public bool isHardDrive;        // жёсткий диск или нет
    public bool isBroken = false;   // сломан или исправен

    private Outline outline;

    void Start()
    {
        outline = GetComponent<Outline>();
        if (outline != null) outline.enabled = false;
    }

    public void ShowOutline(bool show)
    {
        if (outline != null) outline.enabled = show;
    }

    // Создаёт копию объекта и крепит к руке
    public PickupItem CreateCopy(Transform hand)
    {
        // Создаём копию
        GameObject copy = Instantiate(gameObject);
        PickupItem copyItem = copy.GetComponent<PickupItem>();
        if (copyItem == null)
        {
            copyItem = copy.AddComponent<PickupItem>();
            copyItem.componentId = componentId;
            copyItem.isHardDrive = isHardDrive;
            copyItem.isBroken = isBroken;
        }

        // Сохраняем оригинальный масштаб
        Vector3 originalScale = transform.localScale;

        // Настраиваем копию
        copy.transform.SetParent(hand);
        copy.transform.localPosition = Vector3.zero;
        copy.transform.localRotation = Quaternion.identity;
        copy.transform.localScale = originalScale; // Восстанавливаем масштаб

        // Отключаем коллайдер и физику у копии
        Collider col = copy.GetComponent<Collider>();
        if (col != null) col.enabled = false;

        Rigidbody rb = copy.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        // Отключаем подсветку у копии
        Outline copyOutline = copy.GetComponent<Outline>();
        if (copyOutline != null) copyOutline.enabled = false;

        Debug.Log($"Создана копия: {componentId}, масштаб: {originalScale}");
        return copyItem;
    }

    // Выбросить предмет из руки
    public void Drop()
    {
        transform.SetParent(null);

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        Debug.Log($"Выброшен: {componentId}");
    }
}