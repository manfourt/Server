using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FailureGenerator : MonoBehaviour
{
    [Header("Настройки генерации")]
    public float minTimeBetweenFailures = 10f;   // Минимум 10 секунд
    public float maxTimeBetweenFailures = 30f;   // Максимум 30 секунд
    public bool generateFailures = true;         // Включена ли генерация

    [Header("Список компонентов")]
    public List<FailureComponent> components = new List<FailureComponent>();

    [System.Serializable]
    public class FailureComponent
    {
        public string componentName;      // Название компонента (Fan1, CPU и т.д.)
        public string failureType;        // Тип поломки
        public GameObject targetObject;   // Ссылка на объект в сцене (опционально)
        public bool isBroken = false;     // Сломан ли уже
    }

    void Start()
    {
        if (generateFailures)
        {
            StartCoroutine(GenerateFailures());
        }
    }

    IEnumerator GenerateFailures()
    {
        while (generateFailures)
        {
            // Ждём случайное время
            float waitTime = Random.Range(minTimeBetweenFailures, maxTimeBetweenFailures);
            yield return new WaitForSeconds(waitTime);

            // Генерируем поломку
            GenerateRandomFailure();
        }
    }

    // ИСПРАВЛЕНО: убран yield break, так как метод void
    void GenerateRandomFailure()
    {
        // Получаем ещё не сломанные компоненты
        List<FailureComponent> availableComponents = components.FindAll(c => !c.isBroken);

        if (availableComponents.Count == 0)
        {
            Debug.Log("Все компоненты уже сломаны!");
            return; // Просто выходим из метода, без yield
        }

        // Выбираем случайный компонент
        FailureComponent selected = availableComponents[Random.Range(0, availableComponents.Count)];

        // Помечаем как сломанный
        selected.isBroken = true;

        // Показываем уведомление на мониторе
        if (MonitorUIManager.Instance != null)
        {
            MonitorUIManager.Instance.ShowNotification($"{selected.componentName} - {selected.failureType}");
        }

        // Визуальный эффект поломки (опционально)
        StartCoroutine(VisualFailureEffect(selected));

        // Если привязан объект - помечаем его как сломанный через 5 секунд
        if (selected.targetObject != null)
        {
            StartCoroutine(DelayedBreak(selected.targetObject, 5f));
        }

        Debug.Log($"Поломка: {selected.componentName} - {selected.failureType}");
    }

    IEnumerator DelayedBreak(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);

        // Находим компонент в BrokenComponentManager и помечаем как сломанный
        BrokenComponentManager brokenManager = FindObjectOfType<BrokenComponentManager>();
        if (brokenManager != null)
        {
            // Здесь логика пометки компонента как сломанного
            Debug.Log($"Компонент {obj.tag} теперь сломан и готов к удалению!");
        }
    }

    IEnumerator VisualFailureEffect(FailureComponent component)
    {
        // Эффект мигания на мониторе
        if (MonitorUIManager.Instance != null)
        {
            MonitorUIManager.Instance.ShowUrgentNotification($"{component.componentName} - {component.failureType}");
        }
        yield return null;
    }
}