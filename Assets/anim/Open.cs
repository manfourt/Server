using UnityEngine;
using System.Collections;

public class Open : MonoBehaviour
{
    [SerializeField] private float openDistance = 5f;
    [SerializeField] private LayerMask doorLayerMask = ~0;

    [Header("Анимация серверного бокса (код)")]
    [SerializeField] private float slideDistance = 0.6465f; // 0.9805 - 0.334 = 0.6465
    [SerializeField] private float animationSpeed = 7.5f;

    private Animator anim;
    private Transform player;
    private Outline outlineComponent;
    private CameraViewManager cameraViewManager;
    private Camera mainCamera;

    public bool IsOpen => doorOpen;
    private bool doorOpen = false;
    private bool isAnimating = false;

    private static Open currentlyOpenedBox = null;
    private bool isServerBox;

    // Запоминаем исходную позицию
    private Vector3 initialLocalPosition;
    private Vector3 targetLocalPosition;

    private void Start()
    {
        anim = GetComponent<Animator>();
        outlineComponent = GetComponent<Outline>();
        cameraViewManager = CameraViewManager.Instance ?? FindObjectOfType<CameraViewManager>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCamera = Camera.main;

        isServerBox = GetComponent<ServerBoxController>() != null;

        doorOpen = false;

        if (isServerBox)
        {
            // Запоминаем позицию, которую выставили в редакторе
            initialLocalPosition = transform.localPosition;
            // Выдвинутая позиция = исходная + смещение по Z (вперёд)
            targetLocalPosition = initialLocalPosition + new Vector3(0, 0, slideDistance);
        }
        else if (anim != null)
        {
            anim.SetBool("Open", true);
        }

        if (outlineComponent != null)
            outlineComponent.enabled = false;

        doorLayerMask = ~LayerMask.GetMask("Viewpoint");
    }

    private void Update()
    {
        if (player == null)
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

                if (Input.GetMouseButtonDown(0))
                {
                    ToggleDoor();
                }
            }
        }

        if (!isLookingAtDoor && outlineComponent != null)
            outlineComponent.enabled = false;
    }

    public void ToggleDoor()
    {
        if (isAnimating)
            return;

        if (doorOpen)
        {
            CloseDoor();
        }
        else
        {
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
        {
            currentlyOpenedBox = this;
            StopAllCoroutines();
            StartCoroutine(AnimateDoor(targetLocalPosition));
        }
        else if (anim != null)
        {
            anim.SetBool("Open", false);
        }

        Debug.Log($"Дверь открыта: {gameObject.name}");
    }

    private void CloseDoor()
    {
        doorOpen = false;

        if (isServerBox)
        {
            if (currentlyOpenedBox == this)
                currentlyOpenedBox = null;

            StopAllCoroutines();
            StartCoroutine(AnimateDoor(initialLocalPosition));

            if (cameraViewManager != null && cameraViewManager.IsSpecialViewActive)
                cameraViewManager.ExitSpecialView();
        }
        else if (anim != null)
        {
            anim.SetBool("Open", true);
        }

        Debug.Log($"Дверь закрыта: {gameObject.name}");
    }

    private IEnumerator AnimateDoor(Vector3 targetPos)
    {
        isAnimating = true;
        Vector3 startPosition = transform.localPosition;
        float duration = 1f / animationSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localPosition = Vector3.Lerp(startPosition, targetPos, t);
            yield return null;
        }

        transform.localPosition = targetPos;
        isAnimating = false;
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