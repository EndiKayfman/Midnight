using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StorageBuilding : AbstractBuilding, ISavableBuilding
{
    [Header("Storage Settings")]
    public List<ResourceType> storableResources;
    
    [System.Serializable]
    public class StorageData
    {
        public string resourceId;
        public int amount;
    }
    
    [SerializeField] private List<StorageData> _serializedStorage = new List<StorageData>();
    private Dictionary<string, int> _runtimeStorage = new Dictionary<string, int>();

    protected override void Awake()
    {
        base.Awake();
        InitializeStorage();
    }

    private void InitializeStorage()
    {
        _runtimeStorage = _serializedStorage.ToDictionary(
            item => item.resourceId, 
            item => item.amount
        );
        
        foreach (var resource in storableResources.Where(r => r != null))
        {
            string resourceId = resource.name;
            if (!_runtimeStorage.ContainsKey(resourceId))
            {
                _runtimeStorage[resourceId] = 0;
            }
        }
    }

    protected override void HandleInputTransfer()
    {
        foreach (var resource in storableResources.Where(r => r != null))
        {
            if (ResourceManager.Instance.GetResourceCount(resource) > 0)
            {
                if (TransferResourceToStorage(resource))
                {
                    return;
                }
            }
        }
    }

    private bool TransferResourceToStorage(ResourceType resource)
    {
        StartCoroutine(TransferResource(
            resource,
            playerTransform.position + Vector3.up * 0.5f,
            inputResourceTarget.position,
            true
        ));
        
        ResourceManager.Instance.RemoveResource(resource, 1);
        AddResource(resource, 1);
        return true;
    }

    protected override void HandleOutputTransfer()
    {
        foreach (var resource in storableResources.Where(r => r != null))
        {
            if (TryGetResource(resource, out int count) && count > 0)
            {
                if (TransferResourceFromStorage(resource))
                {
                    return;
                }
            }
        }
    }

    private bool TransferResourceFromStorage(ResourceType resource)
    {
        StartCoroutine(TransferResource(
            resource,
            outputResourceTarget.position,
            playerTransform.position + Vector3.up * 0.5f,
            false
        ));
        
        RemoveResource(resource, 1);
        ResourceManager.Instance.AddResource(resource, 1);
        return true;
    }

    public void AddResource(ResourceType resource, int amount)
    {
        if (resource == null) return;
        
        string resourceId = resource.name;
        if (!_runtimeStorage.ContainsKey(resourceId))
        {
            _runtimeStorage[resourceId] = 0;
        }
        
        _runtimeStorage[resourceId] += amount;
        UpdateSerializedData();
    }

    public bool RemoveResource(ResourceType resource, int amount)
    {
        if (resource == null || amount <= 0) return false;
        
        string resourceId = resource.name;
        if (!_runtimeStorage.ContainsKey(resourceId) || _runtimeStorage[resourceId] < amount)
        {
            return false;
        }
        
        _runtimeStorage[resourceId] -= amount;
        UpdateSerializedData();
        Debug.Log($"[Storage] Removed {amount} {resourceId}, remaining: {_runtimeStorage[resourceId]}");
        return true;
    }

    public bool TryGetResource(ResourceType resource, out int amount)
    {
        amount = 0;
        if (resource == null) return false;
        
        return _runtimeStorage.TryGetValue(resource.name, out amount);
    }

    public bool HasResource(ResourceType resource, int requiredAmount = 1)
    {
        if (resource == null) return false;
        
        return TryGetResource(resource, out int amount) && amount >= requiredAmount;
    }

    private void UpdateSerializedData()
    {
        _serializedStorage = _runtimeStorage
            .Where(p => p.Value > 0)
            .Select(p => new StorageData { resourceId = p.Key, amount = p.Value })
            .ToList();
    }

    protected override void CheckProduction()
    {
        // Не требуется для склада
    }

    #region Saving/Loading
    public override void LoadSaveData(BuildingSaveData data)
    {
        _serializedStorage.Clear();
        
        foreach (var item in data.storedResources)
        {
            _serializedStorage.Add(new StorageData {
                resourceId = item.resourceName,
                amount = item.amount
            });
        }
        
        InitializeStorage();
        
        Debug.Log($"Loaded {data.storedResources.Count} resources into storage");
        PrintStorageContents();
    }

    public override BuildingSaveData GetSaveData()
    {
        var data = base.GetSaveData();
        data.isConstructed = true;
        
        foreach (var pair in _runtimeStorage)
        {
            if (pair.Value > 0)
            {
                data.storedResources.Add(new ResourceSaveData {
                    resourceName = pair.Key,
                    amount = pair.Value
                });
            }
        }
        
        return data;
    }

    [ContextMenu("Print Storage Contents")]
    public void PrintStorageContents()
    {
        Debug.Log($"=== Storage {BuildingId} Contents ===");
        foreach (var pair in _runtimeStorage.Where(p => p.Value > 0))
        {
            Debug.Log($"{pair.Key}: {pair.Value}");
        }
    }
    #endregion
}