using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ComponentData
{
    public KeyCode key;            // клавиша для удаления
    public string tag;             // тег объекта в сцене
    public bool isBroken;          // сломан ли компонент
    public bool existsInScene;     // присутствует ли объект в сцене
}

public class BrokenComponentManager : MonoBehaviour
{
    [Header("Компоненты для удаления")]
    public List<ComponentData> components = new List<ComponentData>();

    private CameraViewManager cameraViewManager;

    void Start()
    {
        cameraViewManager = FindObjectOfType<CameraViewManager>();

        // Если список пуст - инициализируем
        if (components == null || components.Count == 0)
        {
            InitializeDefaultComponents();
        }
        else
        {
            // Проверяем, что все теги не пустые
            foreach (var comp in components)
            {
                if (string.IsNullOrEmpty(comp.tag))
                {
                    Debug.LogError($"Компонент {comp.key} имеет пустой тег! Исправьте в инспекторе.");
                }
            }
        }

        // Определяем, какие объекты реально есть в сцене
        foreach (var comp in components)
        {
            if (!string.IsNullOrEmpty(comp.tag))
            {
                GameObject obj = GameObject.FindGameObjectWithTag(comp.tag);
                comp.existsInScene = (obj != null);
                if (obj == null && comp.existsInScene == false)
                {
                    Debug.Log($"Объект с тегом {comp.tag} не найден в сцене");
                }
            }
        }
    }

    void InitializeDefaultComponents()
    {
        components = new List<ComponentData>
        {
            new ComponentData { key = KeyCode.Alpha1, tag = "Fan1", isBroken = true, existsInScene = false },
            new ComponentData { key = KeyCode.Alpha2, tag = "Fan2", isBroken = true, existsInScene = false },
            new ComponentData { key = KeyCode.Alpha3, tag = "PSU", isBroken = true, existsInScene = false },
            new ComponentData { key = KeyCode.Alpha4, tag = "Cooler", isBroken = true, existsInScene = false },
            new ComponentData { key = KeyCode.Alpha5, tag = "CPU", isBroken = true, existsInScene = false },
            new ComponentData { key = KeyCode.Alpha6, tag = "RAM1", isBroken = true, existsInScene = false },
            new ComponentData { key = KeyCode.Alpha7, tag = "RAM2", isBroken = true, existsInScene = false },
            new ComponentData { key = KeyCode.Alpha8, tag = "RAM3", isBroken = true, existsInScene = false },
            new ComponentData { key = KeyCode.Alpha9, tag = "RAM4", isBroken = true, existsInScene = false },
            new ComponentData { key = KeyCode.Alpha0, tag = "Motherboard", isBroken = true, existsInScene = false }
        };

        Debug.Log("Инициализированы компоненты по умолчанию");
    }

    void Update()
    {
        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive)
        {
            foreach (var comp in components)
            {
                if (Input.GetKeyDown(comp.key))
                {
                    // Проверяем, что тег не пустой
                    if (string.IsNullOrEmpty(comp.tag))
                    {
                        Debug.LogError($"Тег для клавиши {comp.key} не задан!");
                        break;
                    }

                    if (comp.existsInScene && comp.isBroken)
                    {
                        GameObject obj = GameObject.FindGameObjectWithTag(comp.tag);
                        if (obj != null)
                        {
                            Destroy(obj);
                            comp.existsInScene = false;
                            Debug.Log($"Удалён {comp.tag} (клавиша {comp.key})");
                        }
                    }
                    else
                    {
                        if (!comp.existsInScene)
                            Debug.Log($"Компонент {comp.tag} уже удалён");
                        else if (!comp.isBroken)
                            Debug.Log($"Компонент {comp.tag} не сломан, удалять нельзя");
                    }
                    break;
                }
            }
        }
    }
}