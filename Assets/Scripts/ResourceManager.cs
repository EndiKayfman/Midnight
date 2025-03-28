using UnityEngine;
using System.Collections.Generic;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    public delegate void InventoryChangedHandler(Dictionary<ResourceType, int> inventory);
    public event InventoryChangedHandler OnInventoryChanged;

    private Dictionary<ResourceType, int> inventory = new Dictionary<ResourceType, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        if (inventory == null)
        {
            inventory = new Dictionary<ResourceType, int>();
        }
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (type == null || amount <= 0) return;

        if (inventory.ContainsKey(type))
        {
            inventory[type] += amount;
        }
        else
        {
            inventory.Add(type, amount);
        }

        OnInventoryChanged?.Invoke(new Dictionary<ResourceType, int>(inventory));
    }

    public void RemoveResource(ResourceType type, int amount)
    {
        if (type == null || !inventory.ContainsKey(type)) return;

        inventory[type] = Mathf.Max(0, inventory[type] - amount);
        OnInventoryChanged?.Invoke(new Dictionary<ResourceType, int>(inventory));
    }

    public int GetResourceCount(ResourceType type)
    {
        return inventory.ContainsKey(type) ? inventory[type] : 0;
    }

    public Dictionary<ResourceType, int> GetInventory()
    {
        return new Dictionary<ResourceType, int>(inventory);
    }
    

    public void ClearInventory()
    {
        inventory.Clear();
        OnInventoryChanged?.Invoke(new Dictionary<ResourceType, int>(inventory));
    }
}