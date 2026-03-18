using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouse : MonoBehaviour
{
    float rotationX = 0f;

    public Vector2 sensitivity = Vector2.one * 200f;
    public float maxVerticalAngle = 80f;
    private Transform playerBody;

    void Start()
    {
        playerBody = GameObject.FindGameObjectWithTag("Player").transform;

        // Блокируем курсор в центре экрана
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity.x;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity.y;

        // Вертикаль (поворот камеры)
        rotationX += mouseY * -1;
        rotationX = Mathf.Clamp(rotationX, -maxVerticalAngle, maxVerticalAngle);

        // Применяем вертикальный поворот к камере
        transform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);

        // Горизонталь (поворот тела игрока)
        playerBody.Rotate(Vector3.up * mouseX);
    }

    // Рисуем простую точку в центре экрана
    void OnGUI()
    {
        // Координаты центра экрана
        float centerX = Screen.width / 2;
        float centerY = Screen.height / 2;

        // Рисуем белую точку 4x4 пикселя
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(centerX - 2, centerY - 2, 4, 4), Texture2D.whiteTexture);
    }
}