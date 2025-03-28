using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Main References")]
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private GameObject inventoryWindow;
    [SerializeField] private Transform itemsContainer;
    [SerializeField] private InventoryItemUI itemPrefab;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private Dictionary<ResourceType, InventoryItemUI> currentItems = new Dictionary<ResourceType, InventoryItemUI>();
    private bool isInventoryOpen = false;

    private void Start()
    {
        if (inventoryButton != null)
        {
            inventoryButton.onClick.AddListener(ToggleInventory);
            closeButton.onClick.AddListener(ToggleInventory);
            
            LogDebug("Inventory button listener added");
        }
        else
        {
            LogError("Inventory button reference is missing!");
        }

        if (inventoryWindow != null)
        {
            inventoryWindow.SetActive(false);
            LogDebug("Inventory window initialized");
        }

        ResourceManager.Instance.OnInventoryChanged += UpdateInventoryUI;
        LogDebug("Subscribed to inventory changes");
    }

    private void ToggleInventory()
    {
        if (isInventoryOpen)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    private void OpenInventory()
    {
        if (inventoryWindow == null) return;
        
        inventoryWindow.SetActive(true);
        isInventoryOpen = true;
        UpdateInventoryUI(ResourceManager.Instance.GetInventory());
        LogDebug("Inventory opened");
    }

    private void CloseInventory()
    {
        if (inventoryWindow == null) return;
        
        inventoryWindow.SetActive(false);
        isInventoryOpen = false;
        LogDebug("Inventory closed");
    }

    private void UpdateInventoryUI(Dictionary<ResourceType, int> inventory)
    {
        if (itemsContainer == null || itemPrefab == null)
        {
            LogError("UI references are not set!");
            return;
        }

        LogDebug($"Updating UI with {inventory.Count} items");
        
        List<ResourceType> toRemove = new List<ResourceType>();
        foreach (var existingItem in currentItems)
        {
            if (!inventory.ContainsKey(existingItem.Key) || inventory[existingItem.Key] <= 0)
            {
                Destroy(existingItem.Value.gameObject);
                toRemove.Add(existingItem.Key);
                LogDebug($"Removed item: {existingItem.Key?.name}");
            }
        }

        foreach (var key in toRemove)
        {
            currentItems.Remove(key);
        }
        
        foreach (var item in inventory)
        {
            if (item.Value <= 0) continue;

            if (currentItems.ContainsKey(item.Key))
            {
                currentItems[item.Key].UpdateCount(item.Value);
                LogDebug($"Updated item: {item.Key.name} x{item.Value}");
            }
            else
            {
                var newItem = Instantiate(itemPrefab, itemsContainer);
                newItem.Initialize(item.Key.icon, item.Key.displayName, item.Value);
                currentItems.Add(item.Key, newItem);
                LogDebug($"Added new item: {item.Key.name} x{item.Value}");
            }
        }
    }

    private void LogDebug(string message)
    {
        if (debugMode) Debug.Log($"[InventoryUI] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[InventoryUI] {message}");
    }

    private void OnDestroy()
    {
        if (ResourceManager.Instance != null)
        {
            ResourceManager.Instance.OnInventoryChanged -= UpdateInventoryUI;
        }
        
        if (inventoryButton != null)
        {
            inventoryButton.onClick.RemoveListener(ToggleInventory);
        }
    }
}