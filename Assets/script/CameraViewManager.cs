using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;

public class CameraViewManager : MonoBehaviour
{
    public static CameraViewManager Instance { get; private set; }

    [Header("XR Origin")]
    [SerializeField] private XROrigin xrOrigin;

    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 2f;

    private CharacterController characterController;

    private Transform viewpoint_R;
    private Transform viewpoint_T;

    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

    private bool movingToTarget = false;

    private bool isSpecialViewActive;

    private enum ViewType
    {
        None,
        R,
        T
    }

    private ViewType currentView = ViewType.None;

    public bool IsSpecialViewActive => isSpecialViewActive;
    public bool IsViewR => currentView == ViewType.R;
    public bool IsViewT => currentView == ViewType.T;

    private TeleportationProvider teleportationProvider;
    private ActionBasedContinuousMoveProvider continuousMoveProvider;
    private ActionBasedSnapTurnProvider snapTurnProvider;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (xrOrigin == null)
            xrOrigin = FindObjectOfType<XROrigin>();

        if (xrOrigin == null)
        {
            Debug.LogError("[CameraViewManager] XR Origin not found!");
            return;
        }

        characterController = xrOrigin.GetComponent<CharacterController>();

        teleportationProvider = xrOrigin.GetComponent<TeleportationProvider>();
        continuousMoveProvider = xrOrigin.GetComponent<ActionBasedContinuousMoveProvider>();
        snapTurnProvider = xrOrigin.GetComponent<ActionBasedSnapTurnProvider>();
    }

    private void Update()
    {
        if (movingToTarget)
        {
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
        Vector3 direction = targetPosition - xrOrigin.transform.position;

        direction.y = 0f;

        float distance = direction.magnitude;

        if (distance < 0.05f)
        {
            movingToTarget = false;

            xrOrigin.transform.position = targetPosition;
            xrOrigin.transform.rotation = targetRotation;

            return;
        }

        Vector3 move = direction.normalized * moveSpeed * Time.deltaTime;

        if (characterController != null)
        {
            characterController.Move(move);
        }
        else
        {
            xrOrigin.transform.position += move;
        }

        Quaternion targetRot = Quaternion.Euler(
            0,
            targetRotation.eulerAngles.y,
            0
        );

        xrOrigin.transform.rotation = Quaternion.Slerp(
            xrOrigin.transform.rotation,
            targetRot,
            5f * Time.deltaTime
        );
    }

    public void SetView(string viewType, int servId, int rackId)
    {
        if (xrOrigin == null)
            return;

        originalPosition = xrOrigin.transform.position;
        originalRotation = xrOrigin.transform.rotation;

        if (viewType == "R")
        {
            viewpoint_R = GameObject.Find(
                $"ServerRack_{rackId}/ServerBox_{servId}/ViewPoint_R"
            )?.transform;

            if (viewpoint_R == null)
                return;

            currentView = ViewType.R;

            targetPosition = viewpoint_R.position;
            targetRotation = viewpoint_R.rotation;
        }
        else if (viewType == "T")
        {
            viewpoint_T = GameObject.Find(
                $"ServerRack_{rackId}/ServerBox_{servId}/ViewPoint_T"
            )?.transform;

            if (viewpoint_T == null)
                return;

            currentView = ViewType.T;

            targetPosition = viewpoint_T.position;
            targetRotation = viewpoint_T.rotation;
        }
        else
        {
            return;
        }

        SetLocomotionActive(false);

        movingToTarget = true;

        isSpecialViewActive = true;

        Debug.Log($"[CameraViewManager] View {viewType} activated");
    }

    public void ExitSpecialView()
    {
        if (!isSpecialViewActive)
            return;

        targetPosition = originalPosition;
        targetRotation = originalRotation;

        movingToTarget = true;

        currentView = ViewType.None;

        isSpecialViewActive = false;

        SetLocomotionActive(true);

        Debug.Log("[CameraViewManager] Exit special view");
    }

    private void SetLocomotionActive(bool active)
    {
        if (teleportationProvider != null)
            teleportationProvider.enabled = active;

        if (continuousMoveProvider != null)
            continuousMoveProvider.enabled = active;

        if (snapTurnProvider != null)
            snapTurnProvider.enabled = active;
    }
}