using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FailureGenerator : MonoBehaviour
{
    [Header("Генерация поломок")]
    [SerializeField] private float minTimeBetweenFailures = 10f;
    [SerializeField] private float maxTimeBetweenFailures = 30f;
    [SerializeField] private bool generateFailures = true;

    [Header("Только HDD")]
    [SerializeField] private bool hardDrivesOnly = true;

    [Header("Текст сообщения")]
    [SerializeField] private string failurePrefix = "Новая поломка";

    private BrokenComponentManager brokenComponentManager;

    private void Start()
    {
        brokenComponentManager = BrokenComponentManager.Instance ?? FindObjectOfType<BrokenComponentManager>();

        if (generateFailures)
            StartCoroutine(GenerateFailuresRoutine());
    }

    private IEnumerator GenerateFailuresRoutine()
    {
        while (generateFailures)
        {
            float waitTime = Random.Range(minTimeBetweenFailures, maxTimeBetweenFailures);
            yield return new WaitForSeconds(waitTime);

            GenerateOneFailure();
        }
    }

    public void GenerateOneFailure()
    {
        if (brokenComponentManager == null)
            brokenComponentManager = BrokenComponentManager.Instance ?? FindObjectOfType<BrokenComponentManager>();

        if (brokenComponentManager == null)
        {
            Debug.LogError("[FailureGenerator] BrokenComponentManager не найден.");
            return;
        }

        List<BrokenComponentManager.ComponentData> available = brokenComponentManager.GetAvailableForFailure(hardDrivesOnly);

        if (available.Count == 0)
        {
            Debug.Log("[FailureGenerator] Нет доступных компонентов для поломки.");
            return;
        }

        BrokenComponentManager.ComponentData selected = available[Random.Range(0, available.Count)];
        brokenComponentManager.SetBrokenState(selected.componentId, true);

        string message = $"{failurePrefix}: {selected.componentId}";
        Debug.Log($"[FailureGenerator] {message}");

        if (MonitorUIManager.Instance != null)
            MonitorUIManager.Instance.ShowFailure(message);

        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.ShowFailureMessage();
        }
    }

    public bool IsBroken(string componentId)
    {
        if (brokenComponentManager == null)
            brokenComponentManager = BrokenComponentManager.Instance ?? FindObjectOfType<BrokenComponentManager>();

        return brokenComponentManager != null && brokenComponentManager.IsBroken(componentId);
    }
}