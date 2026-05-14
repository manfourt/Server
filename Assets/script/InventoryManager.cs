using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    public bool HasItem => CurrentItem != ItemType.None;

    [Header("Спрайты для типов компонентов")]
    [SerializeField] private Sprite cpuSprite;
    [SerializeField] private Sprite hddSprite;
    [SerializeField] private Sprite ramSprite;
    [SerializeField] private Sprite psuSprite;
    [SerializeField] private Sprite fanSprite;
    [SerializeField] private Sprite coolingSprite;

    [Header("UI в PlayerHUD (или отдельная панель)")]
    [SerializeField] private Image handItemIcon;   // ссылка на Image внутри HUD
    [SerializeField] private Text handItemText;    // опционально

    public enum ItemType { None, CPU, HDD, RAM, PSU, Fan, Cooling }
    public ItemType CurrentItem { get; private set; } = ItemType.None;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Инициализация UI: скрываем иконку при старте
        UpdateHandUI();
    }

    /// <summary>Попытаться взять компонент по тегу GameObject</summary>
    public bool PickUp(GameObject warehouseObject)
    {
        if (warehouseObject == null) return false;

        string tag = warehouseObject.tag;
        ItemType type = TagToItemType(tag);
        if (type == ItemType.None)
        {
            Debug.LogWarning($"[InventoryManager] Неизвестный тег склада: {tag}");
            return false;
        }

        CurrentItem = type;
        UpdateHandUI();
        Debug.Log($"[InventoryManager] Взят компонент: {type}");
        return true;
    }

    /// <summary>Очистить руку (после ремонта)</summary>
    public void ClearHand()
    {
        CurrentItem = ItemType.None;
        UpdateHandUI();
    }

    private void UpdateHandUI()
    {
        if (handItemIcon != null)
        {
            handItemIcon.sprite = GetSpriteForType(CurrentItem);
            handItemIcon.enabled = CurrentItem != ItemType.None;
        }
        if (handItemText != null)
            handItemText.text = CurrentItem == ItemType.None ? "" : CurrentItem.ToString();
    }

    private Sprite GetSpriteForType(ItemType type)
    {
        return type switch
        {
            ItemType.CPU => cpuSprite,
            ItemType.HDD => hddSprite,
            ItemType.RAM => ramSprite,
            ItemType.PSU => psuSprite,
            ItemType.Fan => fanSprite,
            ItemType.Cooling => coolingSprite,
            _ => null
        };
    }

    public static ItemType TagToItemType(string tag)
    {
        return tag switch
        {
            "CPU" => ItemType.CPU,
            "HDD" => ItemType.HDD,
            "RAM" => ItemType.RAM,
            "PSU" => ItemType.PSU,
            "Fan" => ItemType.Fan,
            "Cooling" => ItemType.Cooling,
            _ => ItemType.None
        };
    }
}