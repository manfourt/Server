using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class MonitorUIManager : MonoBehaviour
{
    public static MonitorUIManager Instance;

    [Header("UI Компоненты на мониторе")]
    public RawImage backgroundImage;        // Для скриншотов
    public GameObject notificationPanel;    // Панель уведомлений
    public Text notificationText;           // Текст уведомления

    [Header("Настройки")]
    public Texture2D[] screenshots;         // Массив скриншотов
    public float screenChangeInterval = 10f; // Интервал смены
    public float notificationDuration = 5f;  // Длительность уведомления
    public float fadeDuration = 0.5f;        // Длительность затухания

    private int currentScreenIndex = 0;
    private Queue<string> notificationQueue = new Queue<string>();
    private bool isShowingNotification = false;
    private CanvasGroup notificationGroup;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Получаем CanvasGroup для плавного затухания
        notificationGroup = notificationPanel.GetComponent<CanvasGroup>();
        if (notificationGroup == null)
        {
            notificationGroup = notificationPanel.AddComponent<CanvasGroup>();
        }

        // Скрываем панель в начале
        notificationPanel.SetActive(false);


        // Принудительно устанавливаем первый скриншот
        if (screenshots.Length > 0 && backgroundImage != null)
        {
            backgroundImage.texture = screenshots[0];
            Debug.Log($"Установлен первый скриншот: {screenshots[0].name}");
        }
        else
        {
            Debug.LogError("Не удалось установить скриншот! Проверьте:");
            Debug.LogError($"- backgroundImage = {backgroundImage}");
            Debug.LogError($"- screenshots.Length = {screenshots.Length}");
        }

        // Запускаем смену скриншотов
        StartCoroutine(RotateScreenshots());

        // Фоновое изображение (должно быть САМЫМ НИЖНИМ)
        if (backgroundImage != null)
        {
            Canvas bgCanvas = backgroundImage.gameObject.GetComponent<Canvas>();
            if (bgCanvas == null) bgCanvas = backgroundImage.gameObject.AddComponent<Canvas>();
            bgCanvas.overrideSorting = true;
            bgCanvas.sortingOrder = 0;
        }

        // Панель уведомлений (должна быть ПОВЕРХ фона)
        if (notificationPanel != null)
        {
            Canvas panelCanvas = notificationPanel.GetComponent<Canvas>();
            if (panelCanvas == null) panelCanvas = notificationPanel.AddComponent<Canvas>();
            panelCanvas.overrideSorting = true;
            panelCanvas.sortingOrder = 10;

            // Добавляем непрозрачный фон, чтобы текст не смешивался со скриншотом
            Image panelImage = notificationPanel.GetComponent<Image>();
            if (panelImage == null) panelImage = notificationPanel.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.85f);
        }

        // Текст (самый верхний)
        if (notificationText != null)
        {
            Canvas textCanvas = notificationText.gameObject.GetComponent<Canvas>();
            if (textCanvas == null) textCanvas = notificationText.gameObject.AddComponent<Canvas>();
            textCanvas.overrideSorting = true;
            textCanvas.sortingOrder = 11;
        }
    }

    IEnumerator RotateScreenshots()
    {
        while (true)
        {
            yield return new WaitForSeconds(screenChangeInterval);

            if (screenshots.Length > 0 && backgroundImage != null)
            {
                currentScreenIndex = (currentScreenIndex + 1) % screenshots.Length;
                backgroundImage.texture = screenshots[currentScreenIndex];
                Debug.Log($"Смена скриншота на {currentScreenIndex}");
            }
        }
    }

    // Показать уведомление о поломке
    public void ShowNotification(string message)
    {
        notificationQueue.Enqueue(message);

        if (!isShowingNotification)
        {
            StartCoroutine(ShowNextNotification());
        }
    }

    IEnumerator ShowNextNotification()
    {
        isShowingNotification = true;

        while (notificationQueue.Count > 0)
        {
            string message = notificationQueue.Dequeue();

            // Показываем уведомление
            notificationText.text = message;
            notificationPanel.SetActive(true);
            notificationGroup.alpha = 1f;

            // Ждём
            yield return new WaitForSeconds(notificationDuration);

            // Плавно скрываем
            float elapsedTime = 0;
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                notificationGroup.alpha = 1f - (elapsedTime / fadeDuration);
                yield return null;
            }

            notificationPanel.SetActive(false);
            notificationGroup.alpha = 1f;
        }

        isShowingNotification = false;
    }

    // Срочное уведомление (красное, мигающее)
    public void ShowUrgentNotification(string message)
    {
        StartCoroutine(UrgentNotificationEffect(message));
    }

    IEnumerator UrgentNotificationEffect(string message)
    {
        notificationText.text = message;
        notificationText.color = Color.red;
        notificationPanel.SetActive(true);

        // Мигаем 3 раза
        for (int i = 0; i < 3; i++)
        {
            notificationGroup.alpha = 1f;
            yield return new WaitForSeconds(0.2f);
            notificationGroup.alpha = 0.3f;
            yield return new WaitForSeconds(0.2f);
        }

        notificationGroup.alpha = 1f;
        yield return new WaitForSeconds(2f);

        // Плавное исчезновение
        float elapsedTime = 0;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            notificationGroup.alpha = 1f - (elapsedTime / fadeDuration);
            yield return null;
        }

        notificationPanel.SetActive(false);
        notificationText.color = Color.white;
        notificationGroup.alpha = 1f;
    }
}