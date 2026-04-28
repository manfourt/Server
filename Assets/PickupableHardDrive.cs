//using UnityEngine;
//using UnityEngine.EventSystems;

//[RequireComponent(typeof(Collider))]
//[RequireComponent(typeof(Outline))]
//public class PickupableHardDrive : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
//{
//    public Sprite icon;               // Иконка для UI
//    private Outline outline;
//    private CameraViewManager cameraViewManager;

//    void Start()
//    {
//        outline = GetComponent<Outline>();
//        outline.enabled = false;
//        cameraViewManager = FindObjectOfType<CameraViewManager>();
//    }

//    public void OnPointerClick(PointerEventData eventData)
//    {
//        // Блокируем подбор в спецрежимах (R/T)
//        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive)
//        {
//            Debug.Log("Нельзя подбирать предметы в режиме просмотра");
//            return;
//        }

//        // Вызываем менеджер руки
//        HeldComponentManager.Instance?.PickUpItem(gameObject, icon);
//    }

//    public void OnPointerEnter(PointerEventData eventData)
//    {
//        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive) return;
//        if (outline != null) outline.enabled = true;
//    }

//    public void OnPointerExit(PointerEventData eventData)
//    {
//        if (outline != null) outline.enabled = false;
//    }

//    // Вызывается менеджером, когда объект возвращается на место
//    public void ShowAgain()
//    {
//        gameObject.SetActive(true);
//        if (outline != null) outline.enabled = false;
//    }
//}