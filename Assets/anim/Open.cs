using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit;

public class Open : MonoBehaviour
{
    [SerializeField] private float openDistance = 5f;
    [SerializeField] private LayerMask doorLayerMask = ~0;

    [Header("Анимация серверного бокса (код)")]
    [SerializeField] private float slideDistance = 0.6465f;
    [SerializeField] private float animationSpeed = 7.5f;

    private Animator anim;
    private Outline outlineComponent;
    private CameraViewManager cameraViewManager;

    public bool IsOpen => doorOpen;
    private bool doorOpen = false;
    private bool isAnimating = false;

    private static Open currentlyOpenedBox = null;
    private bool isServerBox;

    private Vector3 initialLocalPosition;
    private Vector3 targetLocalPosition;
    private bool isDisabled = false;
    private Coroutine currentAnimationCoroutine = null;

    private void Start()
    {
        anim = GetComponent<Animator>();
        outlineComponent = GetComponent<Outline>();
        cameraViewManager = CameraViewManager.Instance ?? FindObjectOfType<CameraViewManager>();

        isServerBox = GetComponent<ServerBoxController>() != null;

        if (isServerBox)
        {
            initialLocalPosition = transform.localPosition;
            targetLocalPosition = initialLocalPosition + new Vector3(0, 0, slideDistance);
        }
        else if (anim != null)
        {
            anim.SetBool("Open", true);
        }

        if (outlineComponent != null)
            outlineComponent.enabled = false;

        if (GetComponent<XRSimpleInteractable>() == null && isServerBox)
        {
            var interactable = gameObject.AddComponent<XRSimpleInteractable>();
            interactable.interactionLayers = -1;
        }
    }

    private void OnEnable()
    {
        isDisabled = false;
    }

    private void OnDisable()
    {
        isDisabled = true;

        if (currentAnimationCoroutine != null)
        {
            try { StopCoroutine(currentAnimationCoroutine); }
            catch { }
            currentAnimationCoroutine = null;
        }
    }

    public void OnHoverEntered()
    {
        if (isDisabled || !gameObject.activeInHierarchy) return;
        if (cameraViewManager != null && cameraViewManager.IsRepairModeActive) return;

        if (outlineComponent != null) outlineComponent.enabled = true;
    }

    public void OnHoverExited()
    {
        if (outlineComponent != null) outlineComponent.enabled = false;
    }

    public void OnSelectEntered()
    {
        if (isDisabled || !gameObject.activeInHierarchy) return;
        if (cameraViewManager != null && cameraViewManager.IsRepairModeActive) return;

        ToggleDoor();
    }

    private void ToggleDoor()
    {
        if (isAnimating || isDisabled || !gameObject.activeInHierarchy) return;

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
        if (isDisabled || !gameObject.activeInHierarchy) return;

        doorOpen = true;

        if (isServerBox)
        {
            if (currentlyOpenedBox != null && currentlyOpenedBox != this)
                currentlyOpenedBox.CloseDoor();

            currentlyOpenedBox = this;
            StopCurrentAnimation();
            StartAnimation(targetLocalPosition);

            ServerBoxController controller = GetComponent<ServerBoxController>();
            if (controller != null) controller.OnDoorOpened();
        }
        else if (anim != null) anim.SetBool("Open", false);
    }

    public void CloseDoor()
    {
        if (isDisabled) return;

        doorOpen = false;

        if (isServerBox)
        {
            if (currentlyOpenedBox == this) currentlyOpenedBox = null;
            StopCurrentAnimation();
            StartAnimation(initialLocalPosition);

            ServerBoxController controller = GetComponent<ServerBoxController>();
            if (controller != null) controller.OnDoorClosed();
        }
        else if (anim != null) anim.SetBool("Open", true);
    }

    private void StopCurrentAnimation()
    {
        if (currentAnimationCoroutine != null)
        {
            try { StopCoroutine(currentAnimationCoroutine); }
            catch { }
            currentAnimationCoroutine = null;
        }
        isAnimating = false;
    }

    private void StartAnimation(Vector3 targetPos)
    {
        if (!gameObject.activeInHierarchy || isDisabled)
        {
            transform.localPosition = targetPos;
            isAnimating = false;
            return;
        }
        currentAnimationCoroutine = StartCoroutine(AnimateDoor(targetPos));
    }

    private IEnumerator AnimateDoor(Vector3 targetPos)
    {
        isAnimating = true;
        Vector3 startPosition = transform.localPosition;
        float duration = 1f / animationSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!gameObject.activeInHierarchy || isDisabled) break;
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            transform.localPosition = Vector3.Lerp(startPosition, targetPos, t);
            yield return null;
        }

        if (gameObject != null && gameObject.activeInHierarchy && !isDisabled)
            transform.localPosition = targetPos;

        isAnimating = false;
        currentAnimationCoroutine = null;
    }

    private void OnDestroy()
    {
        if (isServerBox && currentlyOpenedBox == this)
            currentlyOpenedBox = null;
    }
}