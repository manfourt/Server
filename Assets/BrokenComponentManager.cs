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
    public string componentId;     // ID жёсткого диска (1-6)
}

public class BrokenComponentManager : MonoBehaviour
{
    [Header("Компоненты для удаления")]
    public List<ComponentData> components = new List<ComponentData>();

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
            Debug.Log($"Компонент: tag='{comp.tag}', isHardDrive={comp.isHardDrive}, componentId='{comp.componentId}'");

            if (!string.IsNullOrEmpty(comp.tag))
            {
                GameObject obj = GameObject.FindGameObjectWithTag(comp.tag);
                comp.existsInScene = (obj != null);

                if (obj != null)
                {
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
        clickable.componentId = comp.componentId;
        clickable.isHardDrive = comp.isHardDrive;

        // Настраиваем коллайдер для кликов
        Collider col = obj.GetComponent<Collider>();
        if (col == null)
        {
            col = obj.AddComponent<BoxCollider>();
            Debug.Log($"Добавлен коллайдер для {comp.tag}");
        }

        // Настраиваем слой для raycast
        string layerName = comp.isHardDrive ? "BrokenHardDrive" : "BrokenCompnent";
        int targetLayer = LayerMask.NameToLayer(layerName);
        if (targetLayer != -1)
        {
            obj.layer = targetLayer;
        }
        else
        {
            Debug.LogWarning($"Слой {layerName} не найден! Используйте слой по умолчанию");
            obj.layer = 0; // Default layer
        }


        // Убеждаемся, что у объекта есть компонент Outline
        Outline outline = obj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = obj.AddComponent<Outline>();
        }
        outline.enabled = false;

        Debug.Log($"Добавлен компонент кликабельности для {comp.tag} (ID: {comp.componentId}, тип: {(comp.isHardDrive ? "HDD" : "Component")}, existsInScene= {(comp.existsInScene)})");
    }

    void InitializeDefaultComponents()
    {
        components = new List<ComponentData>
        {
            // Обычные компоненты (удаляются в режиме T)
            new ComponentData { key = KeyCode.Alpha1, tag = "Fan1", isBroken = true, existsInScene = false, isHardDrive = false, componentId = "Fan1" },
            new ComponentData { key = KeyCode.Alpha2, tag = "Fan2", isBroken = true, existsInScene = false, isHardDrive = false, componentId = "Fan2" },
            new ComponentData { key = KeyCode.Alpha3, tag = "PSU", isBroken = true, existsInScene = false, isHardDrive = false, componentId = "PSU" },
            new ComponentData { key = KeyCode.Alpha4, tag = "Cooler", isBroken = true, existsInScene = false, isHardDrive = false, componentId = "Cooler" },
            new ComponentData { key = KeyCode.Alpha5, tag = "CPU", isBroken = true, existsInScene = false, isHardDrive = false, componentId = "CPU" },
            new ComponentData { key = KeyCode.Alpha6, tag = "RAM1", isBroken = true, existsInScene = false, isHardDrive = false, componentId = "RAM1" },
            new ComponentData { key = KeyCode.Alpha7, tag = "RAM2", isBroken = true, existsInScene = false, isHardDrive = false, componentId = "RAM2" },
            new ComponentData { key = KeyCode.Alpha8, tag = "RAM3", isBroken = true, existsInScene = false, isHardDrive = false, componentId = "RAM3" },
            new ComponentData { key = KeyCode.Alpha9, tag = "RAM4", isBroken = true, existsInScene = false, isHardDrive = false, componentId = "RAM4" },

            // Жёсткие диски (удаляются в режиме R)
            new ComponentData { tag = "Hard_drive1", isBroken = true, existsInScene = false, isHardDrive = true, componentId = "1" },
            new ComponentData { tag = "Hard_drive2", isBroken = true, existsInScene = false, isHardDrive = true, componentId = "2" },
            new ComponentData { tag = "Hard_drive3", isBroken = true, existsInScene = false, isHardDrive = true, componentId = "3" },
            new ComponentData { tag = "Hard_drive4", isBroken = true, existsInScene = false, isHardDrive = true, componentId = "4" },
            new ComponentData { tag = "Hard_drive5", isBroken = true, existsInScene = false, isHardDrive = true, componentId = "5" },
            new ComponentData { tag = "Hard_drive6", isBroken = true, existsInScene = false, isHardDrive = true, componentId = "6" }
        };


        Debug.Log("Инициализированы компоненты по умолчанию");
    }

    public bool DeleteComponent(string componentId)
    {
        // Находим компонент с нужным ID
        ComponentData targetComp = components.Find(c => !c.isHardDrive && c.componentId == componentId);

        if (targetComp == null)
        {
            Debug.LogError($"Компонент с ID {componentId} не найден в списке компонентов");
            return false;
        }

        if (!targetComp.existsInScene)
        {
            Debug.Log($"Компонент {targetComp.tag} уже удалён");
            return false;
        }

        if (!targetComp.isBroken)
        {
            Debug.Log($"Компонент {targetComp.tag} не сломан, удалять нельзя");
            return false;
        }

        // Проверяем, что мы в режиме T
        if (cameraViewManager == null || !cameraViewManager.IsSpecialViewActive || !cameraViewManager.IsViewT)
        {
            Debug.Log("Обычные компоненты удаляются только в режиме T");
            return false;
        }

        // Удаляем объект из сцены
        GameObject obj = GameObject.FindGameObjectWithTag(targetComp.tag);
        if (obj != null)
        {
            Destroy(obj);
            targetComp.existsInScene = false;
            Debug.Log($"Удалён {targetComp.tag} (компонент {componentId}) кликом мыши в режиме T");
            return true;
        }

        return false;
    }
    
    public bool DeleteHardDrive(string hardDriveId)
    {
        // Находим компонент с нужным ID
        ComponentData targetComp = components.Find(c => c.isHardDrive && c.componentId == hardDriveId);

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
        
    }
}