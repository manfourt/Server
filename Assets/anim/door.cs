using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class door : MonoBehaviour
{
    private Animator anim;
    public float openDistance = 3f; // Расстояние, с которого можно открыть
    private Transform player; // Ссылка на игрока

    void Start()
    {
        anim = GetComponent<Animator>();
        // Находим игрока по тегу 
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // Вычисляем расстояние до игрока
        float distance = Vector3.Distance(transform.position, player.position);

        // Если игрок рядом И нажата E
        if (distance < openDistance && Input.GetKeyDown(KeyCode.E))
        {
            // Переключаем состояние двери
            bool isOpen = anim.GetBool("Open");
            anim.SetBool("Open", !isOpen);
        }
    }
}
