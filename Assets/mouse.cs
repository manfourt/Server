using UnityEngine;

public class mouse : MonoBehaviour
{
    [SerializeField] private Vector2 sensitivity = Vector2.one * 200f;
    [SerializeField] private float maxVerticalAngle = 80f;

    private float rotationX;
    private Transform playerBody;
    private CameraViewManager cameraManager;
    private bool isSpecialViewActive;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
            playerBody = playerObject.transform;

        cameraManager = CameraViewManager.Instance ?? FindObjectOfType<CameraViewManager>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (isSpecialViewActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                ExitSpecialView();

            return;
        }

        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity.x;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity.y;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -maxVerticalAngle, maxVerticalAngle);

        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);

        if (cameraManager != null)
            cameraManager.UpdateOriginalPosition(transform.position, transform.rotation);
    }

    public void SetSpecialView(bool active)
    {
        isSpecialViewActive = active;
    }

    private void ExitSpecialView()
    {
        isSpecialViewActive = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraManager != null)
            cameraManager.ExitSpecialView();

        if (UIManager.Instance != null)
            UIManager.Instance.HideMenu();
    }

    private void OnGUI()
    {
        if (isSpecialViewActive)
            return;

        float centerX = Screen.width / 2f;
        float centerY = Screen.height / 2f;

        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(centerX - 2, centerY - 2, 4, 4), Texture2D.whiteTexture);
    }
}