using UnityEngine;

public class EyeButtonInteract : MonoBehaviour
{
    [SerializeField] private string viewType = "R";

    private int servId;
    private int rackId;

    private void Start()
    {
        var box = GetComponentInParent<ServerBoxController>();

        if (box == null)
        {
            Debug.LogError("[EyeButtonInteract] ServerBoxController not found");
            return;
        }

        servId = box.servId;
        rackId = box.rackId;
    }

    public void EnterView()
    {
        Debug.Log($"ENTER VIEW {viewType}");

        if (CameraViewManager.Instance == null)
        {
            Debug.LogError("CameraViewManager == null");
            return;
        }

        CameraViewManager.Instance.SetView(viewType, servId, rackId);
    }
}