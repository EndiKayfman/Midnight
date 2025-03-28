using System.Collections.Generic;
using UnityEngine;

public abstract class BuildingFunctionality : MonoBehaviour, ISavableBuilding
{
    [Header("Save Settings")]
    public string buildingId = System.Guid.NewGuid().ToString();
    
    public string BuildingId => buildingId;
    
    protected abstract Dictionary<ResourceType, int> GetStoredResources();
    protected abstract void SetStoredResources(Dictionary<ResourceType, int> resources);
    
    public virtual void Activate()
    {
        // Активация функциональности здания
    }
    
    public List<ResourceSaveData> GetStoredResourcesForSave()
    {
        var resources = GetStoredResources();
        var result = new List<ResourceSaveData>();
        
        foreach (var pair in resources)
        {
            if (pair.Key != null && pair.Value > 0)
            {
                result.Add(new ResourceSaveData
                {
                    resourceName = pair.Key.name,
                    amount = pair.Value
                });
            }
        }
        
        return result;
    }
    
    public void LoadStoredResources(List<ResourceSaveData> savedResources)
    {
        var resources = new Dictionary<ResourceType, int>();
        
        foreach (var item in savedResources)
        {
            var resourceType = SaveLoadManager.Instance.GetResourceTypeByName(item.resourceName);
            if (resourceType != null)
            {
                resources[resourceType] = item.amount;
            }
        }
        
        SetStoredResources(resources);
    }
    
    public BuildingSaveData GetSaveData()
    {
        return new BuildingSaveData
        {
            buildingId = this.buildingId,
            isConstructed = true,
            storedResources = GetStoredResourcesForSave()
        };
    }
    
    public void LoadSaveData(BuildingSaveData data)
    {
        LoadStoredResources(data.storedResources);
    }
}