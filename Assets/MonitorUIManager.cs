using System.Collections;
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
    [SerializeField] private float notificationDuration = 5f;
    [SerializeField] private float fadeDuration = 0.35f;

    private CanvasGroup notificationGroup;
    private int currentScreenIndex = 0;
    private Coroutine notificationRoutine;

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
        {
            notificationGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (notificationGroup == null)
                notificationGroup = notificationPanel.AddComponent<CanvasGroup>();

            notificationGroup.alpha = 0f;
            notificationPanel.SetActive(false);
        }

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

    public void ShowFailure(string message)
    {
        if (notificationRoutine != null)
            StopCoroutine(notificationRoutine);

        notificationRoutine = StartCoroutine(ShowFailureRoutine(message));
    }

    private IEnumerator ShowFailureRoutine(string message)
    {
        if (notificationPanel == null || notificationText == null)
            yield break;

        notificationText.color = Color.red;
        notificationText.text = message;

        notificationPanel.SetActive(true);
        if (notificationGroup != null)
            notificationGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(notificationDuration);

        if (notificationGroup != null)
        {
            float elapsed = 0f;
            float startAlpha = 1f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                notificationGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
                yield return null;
            }

            notificationGroup.alpha = 0f;
        }

        notificationPanel.SetActive(false);

        if (notificationText != null)
            notificationText.color = Color.white;

        notificationRoutine = null;
    }
}