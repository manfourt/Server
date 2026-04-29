using UnityEngine;

public class ServerBoxController : MonoBehaviour
{
    [Header("Id стойки и сервера")]
    [SerializeField] public int servId;
    [SerializeField] public int rackId;

    private Open door;

    private void Awake()
    {
        door = GetComponent<Open>();
    }

    public bool IsDoorOpen()
    {
        return door != null && door.IsOpen;
    }

    public void OpenBoxUI()
    {
        if (!IsDoorOpen())
        {
            Debug.Log("[ServerBoxController] Нельзя открыть UI: дверца закрыта.");
            return;
        }

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMenu();

        Time.timeScale = 0f;
        Debug.Log("[ServerBoxController] Открыто UI ящика.");
    }

    public void CloseBoxUI()
    {
        Time.timeScale = 1f;

        if (UIManager.Instance != null)
            UIManager.Instance.HideMenu();

        Debug.Log("[ServerBoxController] UI ящика закрыто.");
    }
}