using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mouse : MonoBehaviour
{
    float rotationX = 0f;
    float rotationY = 0f;

    public Vector2 sensitivity = Vector2.one * 200f;
    public float maxVerticalAngle = 80f;
    public Transform playerBody;

    void Start()
    {
        
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
}
