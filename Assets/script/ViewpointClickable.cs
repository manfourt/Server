//using UnityEngine;
//using UnityEngine.XR.Interaction.Toolkit;

//public class ViewpointClickable : MonoBehaviour
//{
//    [SerializeField] private string viewType = "R"; // "R" или "T"

//    private int servId;
//    private int rackId;
//    private SpriteRenderer spriteRenderer;
//    private CameraViewManager cameraViewManager;
//    private Outline outline; // Добавим подсветку при наведении

//    private void Start()
//    {
//        var parentBox = GetComponentInParent<ServerBoxController>();
//        if (parentBox != null)
//        {
//            servId = parentBox.servId;
//            rackId = parentBox.rackId;
//        }

//        spriteRenderer = GetComponent<SpriteRenderer>();
//        cameraViewManager = CameraViewManager.Instance;

//        // Добавляем Outline для подсветки
//        outline = GetComponent<Outline>();
//        if (outline == null)
//            outline = gameObject.AddComponent<Outline>();
//        outline.enabled = false;
//        outline.OutlineColor = Color.yellow;
//        outline.OutlineWidth = 5f;
//    }

//    private void Update()
//    {
//        // Поворачиваем спрайт лицом к камере (если используем SpriteRenderer)
//        if (Camera.main != null && spriteRenderer != null)
//        {
//            transform.LookAt(Camera.main.transform);
//            transform.Rotate(0, 180, 0);
//        }
//    }

//    // Для XR Interaction Toolkit (через XR Simple Interactable)
//    public void OnHoverEntered()
//    {
//        if (outline != null)
//            outline.enabled = true;
//    }

//    public void OnHoverExited()
//    {
//        if (outline != null)
//            outline.enabled = false;
//    }

//    public void OnSelectEntered()
//    {
//        if (cameraViewManager != null && !cameraViewManager.IsSpecialViewActive)
//        {
//            cameraViewManager.SetView(viewType, servId, rackId);
//        }
//    }

//    // Метод для скрытия/показа иконки
//    public void SetActive(bool active)
//    {
//        if (spriteRenderer != null)
//            spriteRenderer.enabled = active;
//        if (outline != null)
//            outline.enabled = false;
//    }
//}