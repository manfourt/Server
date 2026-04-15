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
    public bool openserverbox;

    void Start()
    {
        anim = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        anim.SetBool("Open", true);
        outlineComponent = GetComponent<Outline>();
        cameraViewManager = FindObjectOfType<CameraViewManager>();
        openserverbox = !(anim.GetBool("Open"));

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
                    Debug.Log($"ЛКМ на объекте {gameObject.name}");

                    if ((uiManager != null && uiManager.IsMenuOpen) || cameraViewManager.IsSpecialViewActive)
                    {
                        Debug.Log("Клик заблокирован: меню открыто или спецрежим");
                        return;
                    }

                    bool isOpen = anim.GetBool("Open");
                    Debug.Log($"Текущее состояние анимации isOpen = {isOpen}");

                    anim.SetBool("Open", !isOpen);
                    Debug.Log($"Новое состояние анимации = {!isOpen}");

                    // Проверяем наличие ServerBoxController
                    ServerBoxController serverBox = GetComponent<ServerBoxController>();
                    Debug.Log($"ServerBoxController найден: {serverBox != null}");

                    if (serverBox != null)
                    {
                        openserverbox = isOpen;
                        Debug.Log($"!!! openserverbox изменён на {openserverbox} !!!");
                    }
                    else
                    {
                        Debug.Log("Это не ящик (ServerBoxController отсутствует)");
                    }
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