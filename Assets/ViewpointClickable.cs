using UnityEngine;

public class ViewPointClickable : MonoBehaviour
{
    [SerializeField] private string viewType = "R";

    public static bool IsAnyViewpointHovered { get; private set; }

    private int servId;
    private int rackId;

    private Outline outline;
    private SpriteRenderer spriteRenderer;
    private CameraViewManager cameraViewManager;
    private ServerBoxController parentBox;
    private bool isHovered = false;

    private void Start()
    {
        // Ищем родительский ServerBoxController
        parentBox = GetComponentInParent<ServerBoxController>();
        if (parentBox != null)
        {
            servId = parentBox.servId;
            rackId = parentBox.rackId;
        }

        outline = GetComponent<Outline>();
        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        spriteRenderer = GetComponent<SpriteRenderer>();

        cameraViewManager = CameraViewManager.Instance;

        if (outline != null)
        {
            outline.OutlineWidth = 5f;
            outline.enabled = false;
        }
    }

    private void Update()
    {
        // Поворот к камере
        if (Camera.main != null)
        {
            Vector3 direction = Camera.main.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-direction);
        }

        if (cameraViewManager == null)
            cameraViewManager = CameraViewManager.Instance;

        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive)
        {
            SetActive(false);
            IsAnyViewpointHovered = false;
            return;
        }

        if (parentBox == null || !parentBox.IsDoorOpen())
        {
            SetActive(false);
            IsAnyViewpointHovered = false;
            return;
        }

        SetActive(true);

        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        isHovered = false;

        LayerMask viewpointMask = LayerMask.GetMask("Viewpoint");
        if (Physics.Raycast(ray, out hit, 100f, viewpointMask))
        {
            // Проверяем, попал ли луч в этот объект
            isHovered = (hit.collider.gameObject == gameObject);
        }

        IsAnyViewpointHovered = isHovered;

        if (outline != null)
        {
            outline.enabled = isHovered;
        }

        if (isHovered && Input.GetMouseButtonDown(0))
        {
            cameraViewManager.SetView(viewType, servId, rackId);
        }
    }

    private void SetActive(bool active)
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = active;

        if (!active && outline != null)
            outline.enabled = false;
    }

    private void OnDisable()
    {
        if (isHovered)
            IsAnyViewpointHovered = false;
    }

    private void OnDestroy()
    {
        if (isHovered)
            IsAnyViewpointHovered = false;
    }
}