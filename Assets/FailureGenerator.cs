using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FailureGenerator : MonoBehaviour
{
    [Header("Настройки генерации")]
    public float minTimeBetweenFailures = 10f;
    public float maxTimeBetweenFailures = 30f;
    public bool generateFailures = true;

    [Header("Список жёстких дисков")]
    public List<HardDriveFailure> hardDrives = new List<HardDriveFailure>();

    private BrokenComponentManager brokenManager;

    [System.Serializable]
    public class HardDriveFailure
    {
        public string componentId;          // ID жёсткого диска (1-6)
        public string failureMessage = "ОТКАЗ ЖЁСТКОГО ДИСКА!";
        public GameObject targetObject;     // Ссылка на объект в сцене (опционально)
    }

    void Start()
    {
        brokenManager = FindObjectOfType<BrokenComponentManager>();

        // Инициализация списка жёстких дисков, если он пуст
        if (hardDrives.Count == 0)
        {
            InitializeDefaultHardDrives();
        }

        // При старте все жёсткие диски исправны
        SetAllHardDrivesWorking();

        if (generateFailures)
        {
            StartCoroutine(GenerateFailures());
        }
    }

    void InitializeDefaultHardDrives()
    {
        hardDrives = new List<HardDriveFailure>
        {
            new HardDriveFailure { componentId = "1", failureMessage = "ОТКАЗ ЖЁСТКОГО ДИСКА 1!" },
            new HardDriveFailure { componentId = "2", failureMessage = "ОТКАЗ ЖЁСТКОГО ДИСКА 2!" },
            new HardDriveFailure { componentId = "3", failureMessage = "ОТКАЗ ЖЁСТКОГО ДИСКА 3!" },
            new HardDriveFailure { componentId = "4", failureMessage = "ОТКАЗ ЖЁСТКОГО ДИСКА 4!" },
            new HardDriveFailure { componentId = "5", failureMessage = "ОТКАЗ ЖЁСТКОГО ДИСКА 5!" },
            new HardDriveFailure { componentId = "6", failureMessage = "ОТКАЗ ЖЁСТКОГО ДИСКА 6!" }
        };
    }

    void SetAllHardDrivesWorking()
    {
        if (brokenManager == null) return;

        foreach (var hd in hardDrives)
        {
            ComponentData data = brokenManager.components.Find(c => c.isHardDrive && c.componentId == hd.componentId);
            if (data != null)
            {
                data.isBroken = false;
                Debug.Log($"Жёсткий диск {hd.componentId} исправен");
            }
        }
    }

    IEnumerator GenerateFailures()
    {
        while (generateFailures)
        {
            float waitTime = Random.Range(minTimeBetweenFailures, maxTimeBetweenFailures);
            yield return new WaitForSeconds(waitTime);

            BreakRandomHardDrive();
        }
    }

    void BreakRandomHardDrive()
    {
        if (brokenManager == null)
        {
            brokenManager = FindObjectOfType<BrokenComponentManager>();
            if (brokenManager == null)
            {
                Debug.LogError("BrokenComponentManager не найден!");
                return;
            }
        }

        // Получаем список ещё не сломанных жёстких дисков
        List<HardDriveFailure> availableDrives = new List<HardDriveFailure>();

        foreach (var hd in hardDrives)
        {
            ComponentData data = brokenManager.components.Find(c => c.isHardDrive && c.componentId == hd.componentId);
            if (data != null && !data.isBroken)
            {
                availableDrives.Add(hd);
            }
        }

        if (availableDrives.Count == 0)
        {
            Debug.Log("Все жёсткие диски уже сломаны!");
            return;
        }

        // Выбираем случайный жёсткий диск
        HardDriveFailure selected = availableDrives[Random.Range(0, availableDrives.Count)];

        // Сначала показываем уведомление (красное, мигающее)
        if (MonitorUIManager.Instance != null)
        {
            MonitorUIManager.Instance.ShowUrgentNotification($"ОТКАЗ HDD {selected.componentId}!");
            Debug.Log($"Уведомление отправлено: HDD {selected.componentId}");
        }
        else
        {
            Debug.LogWarning("MonitorUIManager не найден в сцене!");
        }

        // Показываем уведомление на HUD игрока
        if (PlayerHUD.Instance != null)
        {
            PlayerHUD.Instance.ShowMessage("Новая поломка!", 5f);
        }

        // Запускаем корутину для отложенной поломки (ждём окончания уведомления)
        StartCoroutine(DelayedBreak(selected.componentId));
    }

    IEnumerator DelayedBreak(string componentId)
    {
        // Ждём 5 секунд (время показа уведомления)
        yield return new WaitForSeconds(5f);

        // Теперь помечаем как сломанный
        ComponentData selectedData = brokenManager.components.Find(c => c.isHardDrive && c.componentId == componentId);
        if (selectedData != null)
        {
            selectedData.isBroken = true;
            Debug.Log($"Жёсткий диск {componentId} теперь СЛОМАН и готов к удалению!");
        }
    }

    // Публичный метод для ручного вызова поломки (для тестов)
    public void ForceBreakRandomHardDrive()
    {
        BreakRandomHardDrive();
    }

    // Публичный метод для проверки состояния
    public bool IsHardDriveBroken(string componentId)
    {
        if (brokenManager == null) return false;

        ComponentData data = brokenManager.components.Find(c => c.isHardDrive && c.componentId == componentId);
        return data != null && data.isBroken;
    }
}