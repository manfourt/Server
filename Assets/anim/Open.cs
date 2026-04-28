using UnityEngine;

public class Open : MonoBehaviour
{
    private static readonly int OpenHash = Animator.StringToHash("Open");

    [SerializeField] private float openDistance = 5f;

    private Animator anim;
    private Transform player;
    private Outline outlineComponent;
    private CameraViewManager cameraViewManager;
    private UIManager uiManager;
    private Camera mainCamera;

    public bool IsOpen => doorOpen;

    private bool doorOpen = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        outlineComponent = GetComponent<Outline>();
        cameraViewManager = CameraViewManager.Instance ?? FindObjectOfType<CameraViewManager>();
        uiManager = UIManager.Instance;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCamera = Camera.main;

        doorOpen = false;
        anim.SetBool("Open", !doorOpen);

        if (outlineComponent != null)
            outlineComponent.enabled = false;
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
        if (distance <= openDistance && Physics.Raycast(ray, out RaycastHit hit, openDistance))
        {
            if (hit.collider != null && hit.collider.GetComponentInParent<Open>() == this)
            {
                isLookingAtDoor = true;

                if (outlineComponent != null)
                    outlineComponent.enabled = true;

                if (Input.GetMouseButtonDown(0))
                {
                    if ((uiManager != null && uiManager.IsMenuOpen) ||
                        (cameraViewManager != null && cameraViewManager.IsSpecialViewActive))
                    {
                        return;
                    }

                    ToggleDoor();
                }
            }
        }

        if (!isLookingAtDoor && outlineComponent != null)
            outlineComponent.enabled = false;
    }

    private void ToggleDoor()
    {
        doorOpen = !doorOpen;

        anim.SetBool("Open", !doorOpen);

        if (doorOpen)
            Debug.Log("Дверь открыта");
        else
            Debug.Log("Дверь закрыта");
    }
}