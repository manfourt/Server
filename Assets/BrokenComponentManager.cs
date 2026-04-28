using System;
using System.Collections.Generic;
using UnityEngine;

public class BrokenComponentManager : MonoBehaviour
{
    public static BrokenComponentManager Instance { get; private set; }

    public enum ComponentKind
    {
        Normal,
        HardDrive
    }

    [Serializable]
    public class ComponentData
    {
        public string componentId;
        public string sceneTag;
        public ComponentKind kind = ComponentKind.Normal;

        [Tooltip("Сломан ли компонент")]
        public bool isBroken;

        [Tooltip("Находится ли объект сейчас в сцене (true) или скрыт (false)")]
        public bool isInScene = true;

        [HideInInspector] public GameObject sceneObject;
    }

    [Header("Все компоненты сервера")]
    [SerializeField] private List<ComponentData> components = new List<ComponentData>();

    [Header("Слои для разных режимов")]
    [SerializeField] private string normalLayerName = "BrokenComponent";
    [SerializeField] private string hardDriveLayerName = "BrokenHardDrive";

    [Header("Автопоиск объектов по тегам")]
    [SerializeField] private bool autoBindOnStart = true;

    private CameraViewManager cameraViewManager;

    public IReadOnlyList<ComponentData> Components => components;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        cameraViewManager = FindObjectOfType<CameraViewManager>();

        if (components == null || components.Count == 0)
            InitializeDefaultComponents();

        if (autoBindOnStart)
            BindAllSceneObjects();
    }

    private void InitializeDefaultComponents()
    {
        components = new List<ComponentData>
        {
            new ComponentData { componentId = "Fan1",     sceneTag = "Fan1",     kind = ComponentKind.Normal,    isBroken = false, isInScene = true },
            new ComponentData { componentId = "Fan2",     sceneTag = "Fan2",     kind = ComponentKind.Normal,    isBroken = false, isInScene = true },
            new ComponentData { componentId = "PSU",      sceneTag = "PSU",      kind = ComponentKind.Normal,    isBroken = false, isInScene = true },
            new ComponentData { componentId = "Cooler",   sceneTag = "Cooler",   kind = ComponentKind.Normal,    isBroken = false, isInScene = true },
            new ComponentData { componentId = "CPU",      sceneTag = "CPU",      kind = ComponentKind.Normal,    isBroken = false, isInScene = true },
            new ComponentData { componentId = "RAM1",     sceneTag = "RAM1",     kind = ComponentKind.Normal,    isBroken = false, isInScene = true },
            new ComponentData { componentId = "RAM2",     sceneTag = "RAM2",     kind = ComponentKind.Normal,    isBroken = false, isInScene = true },
            new ComponentData { componentId = "RAM3",     sceneTag = "RAM3",     kind = ComponentKind.Normal,    isBroken = false, isInScene = true },
            new ComponentData { componentId = "RAM4",     sceneTag = "RAM4",     kind = ComponentKind.Normal,    isBroken = false, isInScene = true },

            new ComponentData { componentId = "Hard_drive1",        sceneTag = "Hard_drive1", kind = ComponentKind.HardDrive, isBroken = false, isInScene = true },
            new ComponentData { componentId = "Hard_drive2",        sceneTag = "Hard_drive2", kind = ComponentKind.HardDrive, isBroken = false, isInScene = true },
            new ComponentData { componentId = "Hard_drive3",        sceneTag = "Hard_drive3", kind = ComponentKind.HardDrive, isBroken = false, isInScene = true },
            new ComponentData { componentId = "Hard_drive4",        sceneTag = "Hard_drive4", kind = ComponentKind.HardDrive, isBroken = false, isInScene = true },
            new ComponentData { componentId = "Hard_drive5",        sceneTag = "Hard_drive5", kind = ComponentKind.HardDrive, isBroken = false, isInScene = true },
            new ComponentData { componentId = "Hard_drive6",        sceneTag = "Hard_drive6", kind = ComponentKind.HardDrive, isBroken = false, isInScene = true },
        };
    }

    private void BindAllSceneObjects()
    {
        foreach (var data in components)
        {
            BindSceneObject(data);
        }
    }

    private void BindSceneObject(ComponentData data)
    {
        if (data == null || string.IsNullOrWhiteSpace(data.sceneTag))
            return;

        GameObject obj = GameObject.FindGameObjectWithTag(data.sceneTag);
        data.sceneObject = obj;
        data.isInScene = obj != null && obj.activeSelf;

        if (obj == null)
        {
            Debug.LogWarning($"[BrokenComponentManager] Объект с тегом '{data.sceneTag}' не найден.");
            return;
        }

        EnsureSetup(obj, data);
        ApplyStateToObject(data);
    }

    private void EnsureSetup(GameObject obj, ComponentData data)
    {
        Collider col = obj.GetComponent<Collider>();
        if (col == null)
            col = obj.AddComponent<BoxCollider>();

        Outline outline = obj.GetComponent<Outline>();
        if (outline == null)
            outline = obj.AddComponent<Outline>();
        outline.enabled = false;

        ComponentClickable clickable = obj.GetComponent<ComponentClickable>();
        if (clickable == null)
            clickable = obj.AddComponent<ComponentClickable>();

        clickable.Initialize(data.componentId, data.kind);

        SetLayer(obj, data.kind);
    }

    private void SetLayer(GameObject obj, ComponentKind kind)
    {
        string layerName = kind == ComponentKind.HardDrive ? hardDriveLayerName : normalLayerName;
        int layer = LayerMask.NameToLayer(layerName);

        if (layer >= 0)
        {
            obj.layer = layer;
        }
        else
        {
            Debug.LogWarning($"[BrokenComponentManager] Слой '{layerName}' не найден. Объект '{obj.name}' оставлен на Default.");
            obj.layer = 0;
        }
    }

    private void ApplyStateToObject(ComponentData data)
    {
        if (data.sceneObject == null)
            return;

        data.sceneObject.SetActive(data.isInScene);

        Outline outline = data.sceneObject.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;
    }

    private ComponentData FindById(string componentId)
    {
        return components.Find(c => c.componentId == componentId);
    }

    public bool CanInteract(ComponentKind kind)
    {
        if (cameraViewManager == null)
            cameraViewManager = FindObjectOfType<CameraViewManager>();

        if (cameraViewManager == null || !cameraViewManager.IsSpecialViewActive)
            return false;

        return kind == ComponentKind.HardDrive
            ? cameraViewManager.IsViewR
            : cameraViewManager.IsViewT;
    }

    public bool IsBroken(string componentId)
    {
        var data = FindById(componentId);
        return data != null && data.isBroken;
    }

    public bool IsInScene(string componentId)
    {
        var data = FindById(componentId);
        return data != null && data.isInScene;
    }

    public bool SetBrokenState(string componentId, bool broken)
    {
        var data = FindById(componentId);
        if (data == null)
        {
            Debug.LogWarning($"[BrokenComponentManager] Компонент '{componentId}' не найден.");
            return false;
        }

        data.isBroken = broken;
        return true;
    }

    public bool TryHideComponent(string componentId)
    {
        var data = FindById(componentId);
        if (data == null)
        {
            Debug.LogWarning($"[BrokenComponentManager] Компонент '{componentId}' не найден.");
            return false;
        }

        if (!data.isBroken)
        {
            Debug.Log($"[BrokenComponentManager] Компонент '{componentId}' не сломан — скрывать нельзя.");
            return false;
        }

        if (!CanInteract(data.kind))
        {
            Debug.Log($"[BrokenComponentManager] Нельзя удалить '{componentId}' в текущем режиме.");
            return false;
        }

        if (!data.isInScene)
        {
            Debug.Log($"[BrokenComponentManager] Компонент '{componentId}' уже скрыт.");
            return false;
        }

        if (data.sceneObject == null)
            BindSceneObject(data);

        if (data.sceneObject == null)
        {
            Debug.LogWarning($"[BrokenComponentManager] Объект '{data.sceneTag}' не найден в сцене.");
            return false;
        }

        data.sceneObject.SetActive(false);
        data.isInScene = false;

        Outline outline = data.sceneObject.GetComponent<Outline>();
        if (outline != null)
            outline.enabled = false;

        Debug.Log($"[BrokenComponentManager] Компонент '{componentId}' скрыт.");
        return true;
    }

    public bool TryShowComponent(string componentId, bool brokenState)
    {
        var data = FindById(componentId);
        if (data == null)
            return false;

        if (data.sceneObject == null)
            BindSceneObject(data);

        if (data.sceneObject == null)
            return false;

        data.isBroken = brokenState;
        data.isInScene = true;
        data.sceneObject.SetActive(true);
        ApplyStateToObject(data);

        Debug.Log($"[BrokenComponentManager] Компонент '{componentId}' возвращён в сцену.");
        return true;
    }

    public List<ComponentData> GetAvailableForFailure(bool hardDrivesOnly = true)
    {
        var result = new List<ComponentData>();

        foreach (var c in components)
        {
            if (!c.isInScene) continue;
            if (c.isBroken) continue;

            if (hardDrivesOnly && c.kind != ComponentKind.HardDrive)
                continue;

            result.Add(c);
        }

        return result;
    }
}