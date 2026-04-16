using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance;

    [Header("UI Элементы")]
    public GameObject notificationPanel;   // Панель с фоном
    public Text notificationText;          // Текст уведомления

    private Coroutine currentNotification;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        if (notificationPanel != null)
            notificationPanel.SetActive(false);
    }

    /// <summary>
    /// Показать временное сообщение в HUD игрока
    /// </summary>
    /// <param name="message">Текст сообщения</param>
    /// <param name="duration">Длительность в секундах</param>
    public void ShowMessage(string message, float duration = 3f)
    {
        if (currentNotification != null)
            StopCoroutine(currentNotification);
        
        currentNotification = StartCoroutine(DisplayMessage(message, duration));
    }

    IEnumerator DisplayMessage(string message, float duration)
    {
        notificationText.text = message;
        notificationPanel.SetActive(true);
        
        yield return new WaitForSeconds(duration);
        
        notificationPanel.SetActive(false);
        currentNotification = null;
    }
}