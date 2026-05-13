using UnityEngine;

public class ViewpointClickable : MonoBehaviour
{
    [SerializeField] private string viewType = "R"; // "R" или "T"

    private int servId;
    private int rackId;

    private SpriteRenderer spriteRenderer;
    private CameraViewManager cameraViewManager;

    private void Start()
    {
        var parentBox = GetComponentInParent<ServerBoxController>();
        if (parentBox != null)
        {
            servId = parentBox.servId;
            rackId = parentBox.rackId;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        cameraViewManager = CameraViewManager.Instance;
    }

    private void Update()
    {
        // Поворачиваем спрайт лицом к камере
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0); // Разворачиваем, т.к. LookAt смотрит "затылком"
        }
    }

    public void OnHoverEntered()
    {
        // Можно добавить эффект подсветки при наведении, если захотите
    }

    public void OnHoverExited()
    {
        // Убираем эффект подсветки
    }

    public void OnSelect()
    {
        if (cameraViewManager != null)
        {
            cameraViewManager.SetView(viewType, servId, rackId);
        }
    }

    // Метод для скрытия/показа иконки из ServerBoxController
    public void SetActive(bool active)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = active;
        }
    }
}
