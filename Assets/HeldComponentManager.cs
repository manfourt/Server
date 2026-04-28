//using UnityEngine;
//using UnityEngine.UI;

//public class HeldComponentManager : MonoBehaviour
//{
//    public static HeldComponentManager Instance;

//    [Header("UI")]
//    public Image heldItemIcon;              // Image для иконки в канвасе

//    private GameObject heldObject;          // Сам объект (скрыт)
//    private PickupableHardDrive heldPickup; // Ссылка для возврата

//    void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);

//        if (heldItemIcon != null)
//            heldItemIcon.gameObject.SetActive(false);
//    }

//    /// <summary>
//    /// Взять предмет в руку. Если уже что-то держим – возвращаем старый на место.
//    /// </summary>
//    public void PickUpItem(GameObject obj, Sprite icon)
//    {
//        // Возвращаем предыдущий предмет на место
//        if (heldObject != null)
//            ReturnHeld();

//        // Скрываем новый объект
//        obj.SetActive(false);

//        // Запоминаем его и его компонент (если есть PickupableHardDrive)
//        heldObject = obj;
//        heldPickup = obj.GetComponent<PickupableHardDrive>();

//        // Показываем иконку
//        if (heldItemIcon != null && icon != null)
//        {
//            heldItemIcon.sprite = icon;
//            heldItemIcon.gameObject.SetActive(true);
//        }
//        else if (heldItemIcon != null)
//        {
//            heldItemIcon.gameObject.SetActive(false);
//        }

//        Debug.Log($"Взят предмет: {obj.name}");
//    }

//    /// <summary>
//    /// Возвращает удерживаемый предмет обратно в сцену.
//    /// </summary>
//    public void ReturnHeld()
//    {
//        if (heldObject == null) return;

//        // Активируем объект обратно
//        heldObject.SetActive(true);

//        // Сообщаем его компоненту, если он есть
//        if (heldPickup != null)
//            heldPickup.ShowAgain();

//        // Сбрасываем
//        heldObject = null;
//        heldPickup = null;

//        // Прячем иконку
//        if (heldItemIcon != null)
//            heldItemIcon.gameObject.SetActive(false);

//        Debug.Log("Предмет возвращён на место");
//    }
//}