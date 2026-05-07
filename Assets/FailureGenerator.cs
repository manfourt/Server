using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FailureGenerator : MonoBehaviour
{
    [Header("Тайминги")]
    [SerializeField] private float baseMinCooldown = 20f;   // когда всё исправно
    [SerializeField] private float baseMaxCooldown = 40f;
    [SerializeField] private float brokenMinCooldown = 50f; // когда есть хотя бы одна непочиненная поломка
    [SerializeField] private float brokenMaxCooldown = 70f;
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
            // Проверяем, есть ли сейчас сломанные (непочиненные) компоненты
            bool hasBroken = brokenComponentManager.Components.Any(c => c.isBroken);

            float minTime = hasBroken ? brokenMinCooldown : baseMinCooldown;
            float maxTime = hasBroken ? brokenMaxCooldown : baseMaxCooldown;

            float waitTime = Random.Range(minTime, maxTime);
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

        string message = BuildFailureMessage(selected);
        Debug.Log($"[FailureGenerator] {message}");

        if (MonitorUIManager.Instance != null)
            MonitorUIManager.Instance.ShowFailure(selected.componentId, message);

        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.ShowFailureMessage();
        }
    }

    private string BuildFailureMessage(BrokenComponentManager.ComponentData comp)
    {
        // Базовое описание отказа
        string baseMessage = comp.failureType;

        // Добавляем номер компонента, если их несколько в сервере
        if (comp.nmbComp > 0)
            baseMessage += $" №{comp.nmbComp}";

        // Уточняем расположение
        string location = $"\nв сервере {comp.nmbServ} {comp.nmbRack}-й стойки!";
        return $"{baseMessage} {location}";
    }

    public bool IsBroken(string componentId)
    {
        if (brokenComponentManager == null)
            brokenComponentManager = BrokenComponentManager.Instance ?? FindObjectOfType<BrokenComponentManager>();

        return brokenComponentManager != null && brokenComponentManager.IsBroken(componentId);
    }
}