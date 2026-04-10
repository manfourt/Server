using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ComponentData
{
    public KeyCode key;            // клавиша для удаления
    public string tag;             // тег объекта в сцене
    public bool isBroken;          // сломан ли компонент
    public bool existsInScene;     // присутствует ли объект в сцене
    public bool isHardDrive;       // является ли жёстким диском (для проверки)
    public string hardDriveId;     // ID жёсткого диска (1-6)
}

public class BrokenComponentManager : MonoBehaviour
{
    [Header("Компоненты для удаления")]
    public List<ComponentData> components = new List<ComponentData>();

    [Header("Настройки")]
    public LayerMask hardDriveLayer; // Слой для жёстких дисков

    private CameraViewManager cameraViewManager;

    void Start()
    {
        cameraViewManager = FindObjectOfType<CameraViewManager>();

        if (components == null || components.Count == 0)
        {
            InitializeDefaultComponents();
        }

        // Проверяем наличие объектов в сцене и добавляем компоненты Clickable
        foreach (var comp in components)
        {
            if (!string.IsNullOrEmpty(comp.tag))
            {
                GameObject obj = GameObject.FindGameObjectWithTag(comp.tag);
                comp.existsInScene = (obj != null);

                if (obj != null && comp.isHardDrive)
                {
                    // Добавляем компонент для кликов мышью на жёсткие диски
                    AddClickableComponent(obj, comp);
                }

                if (obj == null)
                {
                    Debug.Log($"Объект с тегом {comp.tag} не найден в сцене");
                }
            }
        }
    }

    void AddClickableComponent(GameObject obj, ComponentData comp)
    {
        HardDriveClickable clickable = obj.GetComponent<HardDriveClickable>();
        if (clickable == null)
        {
            clickable = obj.AddComponent<HardDriveClickable>();
        }

        // Устанавливаем ID жёсткого диска
        clickable.hardDriveId = comp.hardDriveId;

        // Настраиваем коллайдер для кликов
        Collider col = obj.GetComponent<Collider>();
        if (col == null)
        {
            col = obj.AddComponent<BoxCollider>();
            Debug.Log($"Добавлен коллайдер для {comp.tag}");
        }

        // Настраиваем слой для raycast
        obj.layer = LayerMask.NameToLayer("BrokenHardDrive");

        // Убеждаемся, что у объекта есть компонент Outline
        Outline outline = obj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = obj.AddComponent<Outline>();
        }
        outline.enabled = false;

        Debug.Log($"Добавлен компонент кликабельности для {comp.tag} (ID: {comp.hardDriveId})");
    }

    void InitializeDefaultComponents()
    {
        components = new List<ComponentData>
        {
            new ComponentData { key = KeyCode.Alpha1, tag = "Fan1", isBroken = true, existsInScene = false, isHardDrive = false },
            new ComponentData { key = KeyCode.Alpha2, tag = "Fan2", isBroken = true, existsInScene = false, isHardDrive = false },
            new ComponentData { key = KeyCode.Alpha3, tag = "PSU", isBroken = true, existsInScene = false, isHardDrive = false },
            new ComponentData { key = KeyCode.Alpha4, tag = "Cooler", isBroken = true, existsInScene = false, isHardDrive = false },
            new ComponentData { key = KeyCode.Alpha5, tag = "CPU", isBroken = true, existsInScene = false, isHardDrive = false },
            new ComponentData { key = KeyCode.Alpha6, tag = "RAM1", isBroken = true, existsInScene = false, isHardDrive = false },
            new ComponentData { key = KeyCode.Alpha7, tag = "RAM2", isBroken = true, existsInScene = false, isHardDrive = false },
            new ComponentData { key = KeyCode.Alpha8, tag = "RAM3", isBroken = true, existsInScene = false, isHardDrive = false },
            new ComponentData { key = KeyCode.Alpha9, tag = "RAM4", isBroken = true, existsInScene = false, isHardDrive = false },

            // Жёсткие диски (теперь с ID для кликов)
            new ComponentData { tag = "Hard_drive1", isBroken = true, existsInScene = false, isHardDrive = true, hardDriveId = "1" },
            new ComponentData { tag = "Hard_drive2", isBroken = true, existsInScene = false, isHardDrive = true, hardDriveId = "2" },
            new ComponentData { tag = "Hard_drive3", isBroken = true, existsInScene = false, isHardDrive = true, hardDriveId = "3" },
            new ComponentData { tag = "Hard_drive4", isBroken = true, existsInScene = false, isHardDrive = true, hardDriveId = "4" },
            new ComponentData { tag = "Hard_drive5", isBroken = true, existsInScene = false, isHardDrive = true, hardDriveId = "5" },
            new ComponentData { tag = "Hard_drive6", isBroken = true, existsInScene = false, isHardDrive = true, hardDriveId = "6" }
        };

        Debug.Log("Инициализированы компоненты по умолчанию");
    }

    public bool DeleteHardDrive(string hardDriveId)
    {
        // Находим компонент с нужным ID
        ComponentData targetComp = components.Find(c => c.isHardDrive && c.hardDriveId == hardDriveId);

        if (targetComp == null)
        {
            Debug.LogError($"Жёсткий диск с ID {hardDriveId} не найден в списке компонентов");
            return false;
        }

        if (!targetComp.existsInScene)
        {
            Debug.Log($"Жёсткий диск {targetComp.tag} уже удалён");
            return false;
        }

        if (!targetComp.isBroken)
        {
            Debug.Log($"Жёсткий диск {targetComp.tag} не сломан, удалять нельзя");
            return false;
        }

        // Проверяем, что мы в режиме R
        if (cameraViewManager == null || !cameraViewManager.IsSpecialViewActive || !cameraViewManager.IsViewR)
        {
            Debug.Log("Удаление жёстких дисков доступно только в режиме R");
            return false;
        }

        // Удаляем объект из сцены
        GameObject obj = GameObject.FindGameObjectWithTag(targetComp.tag);
        if (obj != null)
        {
            Destroy(obj);
            targetComp.existsInScene = false;
            Debug.Log($"Удалён {targetComp.tag} (жёсткий диск {hardDriveId}) кликом мыши в режиме R");
            return true;
        }

        return false;
    }


    void Update()
    {
        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive)
        {
            foreach (var comp in components)
            {
                if (comp.isHardDrive) continue;

                if (Input.GetKeyDown(comp.key))
                {
                    if (string.IsNullOrEmpty(comp.tag))
                    {
                        Debug.LogError($"Тег для клавиши {comp.key} не задан!");
                        break;
                    }

                    if (!comp.existsInScene)
                    {
                        Debug.Log($"Компонент {comp.tag} уже удалён");
                        break;
                    }

                    if (!comp.isBroken)
                    {
                        Debug.Log($"Компонент {comp.tag} не сломан, удалять нельзя");
                        break;
                    }

                    // Обычные компоненты удаляются только в режиме T
                    bool canDelete = !cameraViewManager.IsViewR;


                    if (canDelete)
                    {
                        GameObject obj = GameObject.FindGameObjectWithTag(comp.tag);
                        if (obj != null)
                        {
                            Destroy(obj);
                            comp.existsInScene = false;
                            Debug.Log($"Удалён {comp.tag} (клавиша {comp.key}) в режиме {(cameraViewManager.IsViewR ? "R" : "T")}");
                        }
                    }
                    else
                    {
                        Debug.Log($"Нельзя удалить {comp.tag} в текущем режиме. {(comp.isHardDrive ? "Жёсткие диски удаляются только в режиме R" : "Обычные компоненты удаляются только в режиме T")}");
                    }

                    break;
                }
            }
        }
    }
}