using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Open : MonoBehaviour
{
    private Animator anim;
    public float openDistance = 5f;
    private Transform player;
    private Outline outlineComponent;

    void Start()
    {
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        anim.SetBool("Open", true);
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }
        else
        {
            Debug.LogError("Outline component not found on ");
        }
    }

    void Update()
    {
        if (player == null || outlineComponent == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Создаем луч из центра экрана
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        bool isLookingAtDoor = false;

        if (distance < openDistance && Physics.Raycast(ray, out hit, openDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                isLookingAtDoor = true;

                // Включаем обводку при наведении
                if (!outlineComponent.enabled)
                {
                    outlineComponent.enabled = true;
                }

                // Открытие/закрытие по клику
                if (Input.GetMouseButtonDown(0))
                {
                    bool isOpen = anim.GetBool("Open");
                    anim.SetBool("Open", !isOpen);
                }
            }
        }

        // Выключаем обводку, если не смотрим
        if (!isLookingAtDoor && outlineComponent.enabled)
        {
            outlineComponent.enabled = false;
        }
    }
}