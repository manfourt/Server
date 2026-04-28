using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance;

    [SerializeField] private GameObject playerHUDPanel;
    [SerializeField] private Text failureText;

    [SerializeField] private float showTime = 5f;

    Coroutine currentRoutine;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // старт — пусто
        if (playerHUDPanel != null)
            playerHUDPanel.SetActive(false);
    }

    public void ShowFailureMessage()
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine =
            StartCoroutine(
                ShowTemporaryMessage()
            );
    }

    IEnumerator ShowTemporaryMessage()
    {
        if (playerHUDPanel != null)
            playerHUDPanel.SetActive(true);

        if (failureText != null)
            failureText.text = "Новая поломка";

        Debug.Log("Показ уведомления");

        yield return new WaitForSeconds(showTime);

        if (playerHUDPanel != null)
            playerHUDPanel.SetActive(false);

        currentRoutine = null;
    }
}