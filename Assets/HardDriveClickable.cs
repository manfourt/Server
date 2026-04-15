using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Outline))]
public class HardDriveClickable : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Настройки")]
    public string componentId; // ID жёсткого диска (1-6)
    public bool isHardDrive = false; // Это жёсткий диск? (определяет, в каком режиме удалять)

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

        SetupLayer();
    }

    void SetupLayer()
    {
        string layerName = isHardDrive ? "BrokenHardDrive" : "BrokenCompnent";
        int targetLayer = LayerMask.NameToLayer(layerName);

        if (targetLayer != -1)
        {
            gameObject.layer = targetLayer;
            Debug.Log($"Установлен слой {layerName} для {gameObject.name}");
        }
        else
        {
            Debug.LogWarning($"Слой {layerName} не найден! Создайте его в Project Settings");
        }
    }


    void Update()
    {
        // Проверяем, активен ли спецрежим и подходит ли он для этого типа компонента
        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive)
        {
            bool isCorrectMode = false;
            string requiredMode = "";

            if (isHardDrive)
            {
                // Жёсткие диски удаляются только в режиме R
                isCorrectMode = cameraViewManager.IsViewR;
                requiredMode = "R";
            }
            else
            {
                // Обычные компоненты удаляются только в режиме T
                isCorrectMode = cameraViewManager.IsViewT;
                requiredMode = "T";
            }

            if (isCorrectMode)
            {
                // Ручной Raycast для точного попадания
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                // Получаем маску слоя для текущего типа компонента
                int componentLayer = LayerMask.NameToLayer(isHardDrive ? "BrokenHardDrive" : "BrokenCompnent");
                int layerMask = 1 << componentLayer;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    if (hit.collider.gameObject == gameObject)
                    {
                        if (!isHovered)
                        {
                            isHovered = true;
                            outline.enabled = true;
                            Debug.Log($"Наведение на {gameObject.name} (режим {requiredMode})");
                        }

                        if (Input.GetMouseButtonDown(0))
                        {
                            Debug.Log($"Клик по {gameObject.name}");
                            if (brokenComponentManager != null)
                            {
                                bool success = false;

                                if (isHardDrive)
                                {
                                    success = brokenComponentManager.DeleteHardDrive(componentId);
                                }
                                else
                                {
                                    success = brokenComponentManager.DeleteComponent(componentId);
                                }

                                if (success)
                                {
                                    isDestroyed = true;
                                    Destroy(gameObject);
                                }
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
                // Не в том режиме - отключаем подсветку
                isHovered = false;
                outline.enabled = false;
            }
        }
        else if (isHovered)
        {
            // Не в спецрежиме - отключаем подсветку
            isHovered = false;
            outline.enabled = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Проверяем режим
        if (cameraViewManager == null || !cameraViewManager.IsSpecialViewActive)
        {
            Debug.Log("Удаление доступно только в спецрежиме");
            return;
        }

        if (isHardDrive && !cameraViewManager.IsViewR)
        {
            Debug.Log("Жёсткие диски удаляются только в режиме R");
            return;
        }

        if (!isHardDrive && !cameraViewManager.IsViewT)
        {
            Debug.Log("Обычные компоненты удаляются только в режиме T");
            return;
        }

        if (isDestroyed)
        {
            Debug.Log($"Компонент {componentId} уже удалён");
            return;
        }

        if (brokenComponentManager != null)
        {
            bool success = false;
            if (isHardDrive)
            {
                success = brokenComponentManager.DeleteHardDrive(componentId);
            }
            else
            {
                success = brokenComponentManager.DeleteComponent(componentId);
            }

            if (success)
            {
                isDestroyed = true;
                Destroy(gameObject);
                Debug.Log($"Компонент {componentId} удалён кликом мыши в режиме {(isHardDrive ? "R" : "T")}");
            }
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        bool shouldHighlight = false;

        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive && !isDestroyed)
        {
            if (isHardDrive && cameraViewManager.IsViewR)
            {
                shouldHighlight = true;
            }
            else if (!isHardDrive && cameraViewManager.IsViewT)
            {
                shouldHighlight = true;
            }
        }

        if (shouldHighlight && outline != null)
        {
            outline.enabled = true;
            Debug.Log($"Наведение на {gameObject.name} (подсветка включена)");
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (outline != null)
        {
            outline.enabled = false;
        }
    }

    public void DisableOutline()
    {
        if (outline != null)
            outline.enabled = false;
    }
}