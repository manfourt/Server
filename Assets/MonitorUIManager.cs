using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonitorUIManager : MonoBehaviour
{
    public static MonitorUIManager Instance { get; private set; }

    [Header("Экран монитора")]
    [SerializeField] private RawImage backgroundImage;
    [SerializeField] private Texture2D[] screenshots;
    [SerializeField] private float screenChangeInterval = 10f;

    [Header("Сообщение о поломке")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private Text notificationText;

    private int currentScreenIndex = 0;

    // Очередь поломок для отображения
    private Queue<(string componentId, string message)> failureQueue = new Queue<(string, string)>();

    // Текущая отображаемая поломка
    private (string componentId, string message)? currentFailure = null;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (notificationPanel != null)
            notificationPanel.SetActive(false);

        if (backgroundImage != null && screenshots != null && screenshots.Length > 0)
            backgroundImage.texture = screenshots[0];

        if (screenshots != null && screenshots.Length > 1)
            StartCoroutine(RotateScreenshotsRoutine());
    }

    private IEnumerator RotateScreenshotsRoutine()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(screenChangeInterval);

            if (backgroundImage == null || screenshots == null || screenshots.Length == 0)
                continue;

            currentScreenIndex = (currentScreenIndex + 1) % screenshots.Length;
            backgroundImage.texture = screenshots[currentScreenIndex];
        }
    }

    // Показать сообщение о поломке
    public void ShowFailure(string componentId, string message)
    {
        // Добавляем в очередь
        failureQueue.Enqueue((componentId, message));

        // Если сейчас ничего не показывается - показываем сразу
        if (currentFailure == null)
        {
            ShowNextFailure();
        }

        Debug.Log($"[MonitorUIManager] Поломка добавлена в очередь: {componentId} (в очереди: {failureQueue.Count})");
    }

    // Скрыть сообщение о поломке (когда починили)
    public void HideFailure(string componentId)
    {
        // Если это текущая отображаемая поломка
        if (currentFailure.HasValue && currentFailure.Value.componentId == componentId)
        {
            Debug.Log($"[MonitorUIManager] Текущая поломка исправлена: {componentId}");

            // Показываем следующую из очереди
            ShowNextFailure();
        }
        else
        {
            // Удаляем из очереди, если она там есть
            RemoveFromQueue(componentId);
            Debug.Log($"[MonitorUIManager] Поломка удалена из очереди: {componentId}");
        }
    }

    // Показать следующую поломку из очереди
    private void ShowNextFailure()
    {
        if (failureQueue.Count > 0)
        {
            // Берём следующую из очереди
            var nextFailure = failureQueue.Dequeue();
            currentFailure = nextFailure;

            // Отображаем на мониторе
            DisplayFailure(nextFailure.message);

            Debug.Log($"[MonitorUIManager] Показана поломка: {nextFailure.componentId} (осталось в очереди: {failureQueue.Count})");
        }
        else
        {
            // Очередь пуста - скрываем панель
            currentFailure = null;
            HideDisplay();

            Debug.Log("[MonitorUIManager] Все поломки исправлены, монитор очищен");
        }
    }

    // Удалить поломку из очереди (если она не текущая)
    private void RemoveFromQueue(string componentId)
    {
        // Создаём новую очередь без указанной поломки
        var newQueue = new Queue<(string, string)>();

        while (failureQueue.Count > 0)
        {
            var item = failureQueue.Dequeue();
            if (item.componentId != componentId)
            {
                newQueue.Enqueue(item);
            }
        }

        failureQueue = newQueue;
    }

    // Отобразить сообщение на экране
    private void DisplayFailure(string message)
    {
        if (notificationPanel == null || notificationText == null)
            return;

        notificationText.text = message;
        notificationText.color = Color.red;
        notificationPanel.SetActive(true);
    }

    // Скрыть отображение
    private void HideDisplay()
    {
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }

        if (notificationText != null)
        {
            notificationText.text = "";
        }
    }

    // Скрыть все сообщения (экстренный сброс)
    public void HideAllFailures()
    {
        failureQueue.Clear();
        currentFailure = null;
        HideDisplay();
        Debug.Log("[MonitorUIManager] Все сообщения экстренно скрыты");
    }
}