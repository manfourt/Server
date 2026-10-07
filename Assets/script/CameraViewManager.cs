using UnityEngine;

using UnityEngine;

public class CameraViewManager : MonoBehaviour
{
    public static CameraViewManager Instance { get; private set; }

    [Header("VR Телепортация")]
    [SerializeField] private GameObject teleportInteractor;

    private bool isRepairModeActive = false;
    private int currentRackId = 0;
    private int currentServId = 0;

    // ===== ДОБАВЬТЕ ЭТИ ДВЕ СТРОЧКИ ПЕРЕД СУЩЕСТВУЮЩИМИ СВОЙСТВАМИ =====
    public int CurrentRackId => currentRackId;
    public int CurrentServId => currentServId;

    public bool IsSpecialViewActive => isRepairModeActive;
    public bool IsRepairModeActive => isRepairModeActive;
    public bool IsViewR => isRepairModeActive;
    public bool IsViewT => isRepairModeActive;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // УБРАЛИ Escape - теперь выход только через закрытие двери или отдельную кнопку
    }

    public void EnterRepairMode(int rackId, int servId)
    {
        if (isRepairModeActive) return;

        currentRackId = rackId;
        currentServId = servId;
        isRepairModeActive = true;

        Debug.Log($"[CameraViewManager] ===== РЕЖИМ РЕМОНТА АКТИВИРОВАН ===== для сервера {servId}");

        // ФИКС: Отключаем луч телепортации при входе в режим ремонта
        if (teleportInteractor != null)
        {
            teleportInteractor.SetActive(false);
        }

        if (BrokenComponentManager.Instance != null)
        {
            BrokenComponentManager.Instance.SetCollidersForRepairMode(rackId, servId);
        }
    }

    public void ExitRepairMode()
    {
        if (!isRepairModeActive) return;

        isRepairModeActive = false;

        Debug.Log("[CameraViewManager] ===== ВЫХОД ИЗ РЕЖИМА РЕМОНТА =====");

        // ФИКС: Включаем луч телепортации обратно при выходе
        if (teleportInteractor != null)
        {
            teleportInteractor.SetActive(true);
        }

        if (BrokenComponentManager.Instance != null)
        {
            BrokenComponentManager.Instance.DisableAllComponentColliders();
        }
    }
}