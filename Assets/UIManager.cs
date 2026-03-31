using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public GameObject viewSelectionPanel; // Панель с текстом "Нажмите R или T"
    // Публичное свойство для проверки, открыто ли меню
    public bool IsMenuOpen => viewSelectionPanel != null && viewSelectionPanel.activeSelf;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (viewSelectionPanel) viewSelectionPanel.SetActive(false);
    }

    public void ShowMenu()
    {
        if (viewSelectionPanel) viewSelectionPanel.SetActive(true);
    }

    public void HideMenu()
    {
        if (viewSelectionPanel) viewSelectionPanel.SetActive(false);
    }
}