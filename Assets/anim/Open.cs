using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Open : MonoBehaviour
{
    private Animator anim;
    public float openDistance = 5f;
    private Transform player;
    private Outline outlineComponent;
    private UIManager uiManager;
    private CameraViewManager cameraViewManager;

    void Start()
    {
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        anim.SetBool("Open", true);
        outlineComponent = GetComponent<Outline>();
        cameraViewManager = FindObjectOfType<CameraViewManager>();

        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
        }
        else
        {
            Debug.LogError("Outline component not found on ");
        }

        uiManager = UIManager.Instance;
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
                if (!outlineComponent.enabled && !cameraViewManager.IsSpecialViewActive)
                {
                    outlineComponent.enabled = true;
                }

                // Открытие/закрытие по клику
                if (Input.GetMouseButtonDown(0))
                {
                    // Если меню активно (ящик открыт) — игнорируем клик по двери
                    if (uiManager != null && uiManager.IsMenuOpen || cameraViewManager.IsSpecialViewActive)
                    { return; outlineComponent.enabled = false; }

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