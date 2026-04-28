using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject viewSelectionPanel;

    public bool IsMenuOpen => viewSelectionPanel != null && viewSelectionPanel.activeSelf;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        if (viewSelectionPanel != null)
            viewSelectionPanel.SetActive(false);
    }

    public void ShowMenu()
    {
        if (viewSelectionPanel != null)
            viewSelectionPanel.SetActive(true);
    }

    public void HideMenu()
    {
        if (viewSelectionPanel != null)
            viewSelectionPanel.SetActive(false);
    }
}