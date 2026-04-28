using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class playermove : MonoBehaviour
{
    [SerializeField] private float speed = 5f;

    private CharacterController controller;
    private CameraViewManager cameraViewManager;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraViewManager = CameraViewManager.Instance ?? FindObjectOfType<CameraViewManager>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive)
            return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontalInput + transform.forward * verticalInput;
        controller.Move(move * speed * Time.deltaTime);
    }
}