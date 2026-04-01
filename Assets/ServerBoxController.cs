// ServerBoxController.cs
using UnityEngine;

public class ServerBoxController : MonoBehaviour
{
    private bool isOpen = false;
    //private CameraViewManager cameraViewManager;

    void Start()
    {
        //cameraViewManager = FindObjectOfType<CameraViewManager>();
    }

    public void OpenBoxUI()
    {
        isOpen = true;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMenu();
        }

        Time.timeScale = 0;
    }

    public void CloseBoxUI()
    {
        isOpen = false;
        Time.timeScale = 1;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideMenu();
        }

        Time.timeScale = 1;
    }
}