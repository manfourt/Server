using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Outline))]
public class ComponentClickable : MonoBehaviour
{
    [SerializeField] private string componentId;
    [SerializeField] private BrokenComponentManager.ComponentKind kind;

    private Outline outline;
    private CameraViewManager cameraViewManager;
    private BrokenComponentManager brokenComponentManager;
    private Camera mainCamera;

    public void Initialize(string id, BrokenComponentManager.ComponentKind componentKind)
    {
        componentId = id;
        kind = componentKind;
        ApplyLayer();
    }

    private void Awake()
    {
        outline = GetComponent<Outline>();
        mainCamera = Camera.main;
    }

    private void Start()
    {
        cameraViewManager = CameraViewManager.Instance ?? FindObjectOfType<CameraViewManager>();
        brokenComponentManager = BrokenComponentManager.Instance ?? FindObjectOfType<BrokenComponentManager>();

        if (outline != null)
            outline.enabled = false;

        ApplyLayer();
    }

    private void ApplyLayer()
    {
        if (kind == BrokenComponentManager.ComponentKind.HardDrive)
        {
            int layer = LayerMask.NameToLayer("BrokenHardDrive");
            if (layer >= 0) gameObject.layer = layer;
        }
        else
        {
            int layer = LayerMask.NameToLayer("BrokenComponent");
            if (layer >= 0) gameObject.layer = layer;
        }
    }

    private void Update()
    {
        if (brokenComponentManager == null)
            brokenComponentManager = BrokenComponentManager.Instance ?? FindObjectOfType<BrokenComponentManager>();

        if (cameraViewManager == null)
            cameraViewManager = CameraViewManager.Instance ?? FindObjectOfType<CameraViewManager>();

        if (brokenComponentManager == null || cameraViewManager == null)
        {
            SetHighlight(false);
            return;
        }

        if (!cameraViewManager.IsSpecialViewActive || !brokenComponentManager.CanInteract(kind))
        {
            SetHighlight(false);
            return;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
        {
            SetHighlight(false);
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);

        ComponentClickable nearestClickable = null;
        float nearestDistance = Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            ComponentClickable clicked =
               hit.collider.GetComponentInParent<ComponentClickable>();

            if (clicked != null)
            {
                if (hit.distance < nearestDistance)
                {
                    nearestDistance = hit.distance;
                    nearestClickable = clicked;
                }
            }
        }

        bool hitThisObject =
            (nearestClickable == this);

        if (hitThisObject)
        {
            SetHighlight(true);

            if (Input.GetMouseButtonDown(0))
            {
                bool success =
                   brokenComponentManager.TryHideComponent(componentId);

                if (success)
                    SetHighlight(false);
            }
        }
        else
        {
            SetHighlight(false);
        }
    }

    private void SetHighlight(bool value)
    {
        if (outline != null)
            outline.enabled = value;
    }
}