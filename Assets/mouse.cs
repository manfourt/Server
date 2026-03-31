using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouse : MonoBehaviour
{
    float rotationX = 0f;

    public Vector2 sensitivity = Vector2.one * 200f;
    public float maxVerticalAngle = 80f;
    private Transform playerBody;

    // Ссылка на менеджер камеры
    private CameraViewManager cameraManager;
    private bool isSpecialViewActive = false;


    void Start()
    {
        playerBody = GameObject.FindGameObjectWithTag("Player").transform;
        cameraManager = FindObjectOfType<CameraViewManager>();


        // Блокируем курсор в центре экрана
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Проверка на выход из режима просмотра
        if (isSpecialViewActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ExitSpecialView();
            return;
        }

        // Если активен режим просмотра (R/T), обычное управление мышью отключается
        if (isSpecialViewActive) return;


        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity.x;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity.y;

        // Вертикаль (поворот камеры)
        rotationX += mouseY * -1;
        rotationX = Mathf.Clamp(rotationX, -maxVerticalAngle, maxVerticalAngle);

        // Применяем вертикальный поворот к камере
        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // Горизонталь (поворот тела игрока)
        playerBody.Rotate(Vector3.up * mouseX);

        // Обновляем позицию для менеджера камеры (чтобы знать, куда возвращаться)
        if (cameraManager != null)
        {
            cameraManager.UpdateOriginalPosition(transform.position, transform.rotation);
        }
    }

    // Вызывается из CameraViewManager при выборе вида R или T
    public void SetSpecialView(bool active)
    {
        isSpecialViewActive = active;
        if (active)
        {
            Cursor.lockState = CursorLockMode.None; // Освобождаем курсор для UI
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void ExitSpecialView()
    {
        isSpecialViewActive = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraManager != null)
            cameraManager.ExitSpecialView();

        if (UIManager.Instance != null)
            UIManager.Instance.HideMenu();
    }


    // Рисуем простую точку в центре экрана
    void OnGUI()
    {
        // Рисуем прицел только если не активен режим просмотра
        if (!isSpecialViewActive)
        {

            // Координаты центра экрана
            float centerX = Screen.width / 2;
            float centerY = Screen.height / 2;

            // Рисуем белую точку 4x4 пикселя
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(centerX - 2, centerY - 2, 4, 4), Texture2D.whiteTexture);
        }
    }
}