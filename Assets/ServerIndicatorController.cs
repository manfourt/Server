using UnityEngine;

public class ServerIndicatorController : MonoBehaviour
{
    [Header("Индикаторы")]
    [SerializeField] private GameObject okIndicator;
    [SerializeField] private GameObject notOkIndicator;

    [Header("Настройки")]
    [SerializeField] private int rackId = 1;
    [SerializeField] private int servId = 1;
    [SerializeField] private float checkInterval = 1f;

    private BrokenComponentManager brokenManager;
    private float nextCheckTime = 0f;

    private void Start()
    {
        brokenManager = BrokenComponentManager.Instance;

        SetIndicator(okIndicator, false);
        SetIndicator(notOkIndicator, false);
    }

    private void Update()
    {
        if (Time.time >= nextCheckTime)
        {
            nextCheckTime = Time.time + checkInterval;
            CheckServerStatus();
        }
    }

    private void CheckServerStatus()
    {
        if (brokenManager == null)
        {
            brokenManager = BrokenComponentManager.Instance;
            if (brokenManager == null) return;
        }

        bool hasProblem = false;

        foreach (var comp in brokenManager.Components)
        {
            if (comp.nmbRack == rackId && comp.nmbServ == servId)
            {
                // Проблема если: сломан ИЛИ отсутствует в сцене
                if (comp.isBroken || !comp.isInScene)
                {
                    hasProblem = true;
                    break;
                }
            }
        }

        // Зелёный только если ВООБЩЕ нет проблем
        SetIndicator(okIndicator, !hasProblem);
        SetIndicator(notOkIndicator, hasProblem);
    }

    private void SetIndicator(GameObject indicator, bool active)
    {
        if (indicator == null) return;

        Light pointLight = indicator.GetComponentInChildren<Light>(true);

        if (pointLight != null)
        {
            pointLight.enabled = active;
        }
        else
        {
            indicator.SetActive(active);
        }
    }

    public void ForceUpdate()
    {
        nextCheckTime = 0f;
    }
}