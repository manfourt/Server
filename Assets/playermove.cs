using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playermove : MonoBehaviour
{
    public float speed = 5f;
    private CharacterController controller;

    void Start()
    {
        // Получаем компонент Character Controller при старте
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Если время остановлено (меню ящика открыто), блокируем движение
        if (Time.timeScale == 0) return;

        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Направление движения
        Vector3 move = transform.right * horizontalInput + transform.forward * verticalInput;

        // Применяем движение через Character Controller
        controller.Move(move * speed * Time.deltaTime);
    }
}
