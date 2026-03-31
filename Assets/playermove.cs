using UnityEngine;

public class playermove : MonoBehaviour
{
    public float speed = 5f;
    private CharacterController controller;
    private CameraViewManager cameraViewManager;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraViewManager = FindObjectOfType<CameraViewManager>();
    }

    void Update()
    {
        // ≈сли врем€ остановлено (меню €щика открыто), блокируем движение
        if (Time.timeScale == 0) return;

        // ≈сли активен режим просмотра - блокируем движение
        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive) return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Ќаправление движени€
        Vector3 move = transform.right * horizontalInput + transform.forward * verticalInput;

        // ѕримен€ем движение через Character Controller
        controller.Move(move * speed * Time.deltaTime);
    }
}