using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    private Dictionary<ResourceType, int> _resources = new Dictionary<ResourceType, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (_resources.ContainsKey(type))
        {
            _resources[type] += amount;
        }
        else
        {
            _resources.Add(type, amount);
        }

        Debug.Log($"Added {amount} {type.name}. Total: {_resources[type]}");
    }

    public int GetResourceCount(ResourceType type)
    {
        return _resources.ContainsKey(type) ? _resources[type] : 0;
    }
}