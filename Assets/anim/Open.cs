using UnityEngine;

public class Open : MonoBehaviour
{
    private static readonly int OpenHash = Animator.StringToHash("Open");

    [SerializeField] private float openDistance = 5f;
    [SerializeField] private LayerMask doorLayerMask = ~0;

    private Animator anim;
    private Transform player;
    private Outline outlineComponent;
    private CameraViewManager cameraViewManager;
    private Camera mainCamera;

    public bool IsOpen => doorOpen;
    private bool doorOpen = false;

    // Статическая ссылка на текущий открытый серверный бокс
    private static Open currentlyOpenedBox = null;

    // Является ли этот объект серверным боксом
    private bool isServerBox;

    private void Start()
    {
        anim = GetComponent<Animator>();
        outlineComponent = GetComponent<Outline>();
        cameraViewManager = CameraViewManager.Instance ?? FindObjectOfType<CameraViewManager>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCamera = Camera.main;

        // Проверяем, есть ли на объекте ServerBoxController
        isServerBox = GetComponent<ServerBoxController>() != null;

        doorOpen = false;
        if (anim != null)
            anim.SetBool("Open", true);

        if (outlineComponent != null)
            outlineComponent.enabled = false;

        doorLayerMask = ~LayerMask.GetMask("Viewpoint");
    }

    private void Update()
    {
        if (player == null || anim == null)
            return;

        if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive)
        {
            if (outlineComponent != null)
                outlineComponent.enabled = false;
            return;
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool isLookingAtDoor = false;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (distance <= openDistance && Physics.Raycast(ray, out RaycastHit hit, openDistance, doorLayerMask))
        {
            if (hit.collider != null && hit.collider.GetComponentInParent<Open>() == this)
            {
                isLookingAtDoor = true;

                if (outlineComponent != null)
                    outlineComponent.enabled = true;

                if (Input.GetMouseButtonDown(0) && !ViewPointClickable.IsAnyViewpointHovered)
                {
                    ToggleDoor();
                }
            }
        }

        if (!isLookingAtDoor && outlineComponent != null)
            outlineComponent.enabled = false;
    }

    private void ToggleDoor()
    {
        if (doorOpen)
        {
            CloseDoor();
        }
        else
        {
            // Только для серверных боксов: закрываем предыдущий открытый
            if (isServerBox && currentlyOpenedBox != null && currentlyOpenedBox != this)
            {
                currentlyOpenedBox.CloseDoor();
            }

            OpenDoor();
        }
    }

    private void OpenDoor()
    {
        doorOpen = true;

        if (isServerBox)
            currentlyOpenedBox = this;

        if (anim != null)
            anim.SetBool("Open", false);

        Debug.Log($"Дверь открыта: {gameObject.name} (серверный бокс: {isServerBox})");
    }

    private void CloseDoor()
    {
        doorOpen = false;

        if (isServerBox && currentlyOpenedBox == this)
            currentlyOpenedBox = null;

        if (anim != null)
            anim.SetBool("Open", true);

        // Выходим из спецрежима при закрытии
        if (isServerBox && cameraViewManager != null && cameraViewManager.IsSpecialViewActive)
            cameraViewManager.ExitSpecialView();

        Debug.Log($"Дверь закрыта: {gameObject.name}");
    }

    private void OnDestroy()
    {
        if (isServerBox && currentlyOpenedBox == this)
            currentlyOpenedBox = null;
    }

    private void OnDisable()
    {
        if (doorOpen)
        {
            CloseDoor();
        }
    }
}