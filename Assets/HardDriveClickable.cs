using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Outline))]
public class HardDriveClickable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Настройки")]
    public string hardDriveId; // ID жёсткого диска (1-6)
    
    private Outline outline;
    private CameraViewManager cameraViewManager;
    private BrokenComponentManager brokenComponentManager;
    private bool isDestroyed = false;
    private bool isHovered = false;

    void Start()
    {

        outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
            outline.OutlineColor = Color.white;
            outline.OutlineWidth = 5f;
        }
        
        cameraViewManager = FindObjectOfType<CameraViewManager>();
        brokenComponentManager = FindObjectOfType<BrokenComponentManager>();
    }

    void Update()
    {
        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive && cameraViewManager.IsViewR)
        {
            // Ручной Raycast
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Создаём маску только для слоя HardDrive
            int hardDriveLayer = LayerMask.NameToLayer("BrokenHardDrive");
            int layerMask = 1 << hardDriveLayer;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    if (!isHovered)
                    {
                        isHovered = true;
                        outline.enabled = true;
                        Debug.Log($"Наведение на {gameObject.name}");
                    }

                    if (Input.GetMouseButtonDown(0))
                    {
                        Debug.Log($"Клик по {gameObject.name}");
                        if (brokenComponentManager != null)
                        {
                            brokenComponentManager.DeleteHardDrive(hardDriveId);
                            Destroy(gameObject);
                        }
                    }
                }
                else if (isHovered)
                {
                    isHovered = false;
                    outline.enabled = false;
                }
            }
            else if (isHovered)
            {
                isHovered = false;
                outline.enabled = false;
            }
        }
        else if (isHovered)
        {
            isHovered = false;
            outline.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Проверяем, что мы в режиме просмотра и это вид R
        if (cameraViewManager == null || !cameraViewManager.IsSpecialViewActive || !cameraViewManager.IsViewR)
        {
            Debug.Log("Удаление жёстких дисков доступно только в режиме R (вид на диски)");
            return;
        }
        
        // Проверяем, не уничтожен ли уже объект
        if (isDestroyed)
        {
            Debug.Log($"Жёсткий диск {hardDriveId} уже удалён");
            return;
        }
        
        // Удаляем через менеджер
        if (brokenComponentManager != null)
        {
            bool success = brokenComponentManager.DeleteHardDrive(hardDriveId);
            if (success)
            {
                isDestroyed = true;
                Destroy(gameObject);
                Debug.Log($"Жёсткий диск {hardDriveId} удалён кликом мыши");
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Подсвечиваем ТОЛЬКО в режиме R
        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive && cameraViewManager.IsViewR && !isDestroyed)
        {
            if (outline != null)
            {
                outline.enabled = true;
                Debug.Log($"Наведение на жёсткий диск {hardDriveId} (подсветка включена)");
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Отключаем подсветку
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
    
    // Метод для ручного отключения подсветки при выходе из режима
    public void DisableOutline()
    {
        if (outline != null)
            outline.enabled = false;
    }
}