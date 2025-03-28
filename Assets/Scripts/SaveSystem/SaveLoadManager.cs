using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class SaveLoadManager : MonoBehaviour
{
    public static SaveLoadManager Instance { get; private set; }
    private string savePath;

    [SerializeField] private ResourcesDatabase resourcesDatabase;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        savePath = Path.Combine(Application.persistentDataPath, "game_save.json");
    }

    private void Start()
    {
        LoadGame();
    }

     public void SaveGame()
    {
        try
        {
            GameSaveData saveData = new GameSaveData();
            
            var inventory = ResourceManager.Instance.GetInventory();
            foreach (var item in inventory)
            {
                if (item.Value > 0 && item.Key != null)
                {
                    saveData.playerInventory.Add(new ResourceSaveData
                    {
                        resourceName = item.Key.name,
                        amount = item.Value
                    });
                }
            }

            var allBuildings = FindObjectsOfType<MonoBehaviour>().OfType<ISavableBuilding>();
            foreach (var building in allBuildings)
            {
                if (building is StorageBuilding || building.GetSaveData().isConstructed)
                {
                    saveData.buildings.Add(building.GetSaveData());
                }
            }

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(savePath, json);
            Debug.Log("Game saved successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
        }
    }
    
    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;
        
        string json = File.ReadAllText(savePath);
        GameSaveData saveData = JsonUtility.FromJson<GameSaveData>(json);
        
        var allBuildings = Resources.FindObjectsOfTypeAll<GameObject>()
            .SelectMany(go => go.GetComponents<ISavableBuilding>())
            .ToList();
        
        foreach (var buildingData in saveData.buildings)
        {
            var building = allBuildings.FirstOrDefault(b => b.BuildingId == buildingData.buildingId);
            if (building != null)
            {
                building.LoadSaveData(buildingData);
            }
            else
            {
                Debug.LogWarning($"Building not found: {buildingData.buildingId}");
            }
        }
        
        ResourceManager.Instance.ClearInventory();
        foreach (var item in saveData.playerInventory)
        {
            ResourceType type = GetResourceTypeByName(item.resourceName);
            if (type != null)
            {
                ResourceManager.Instance.AddResource(type, item.amount);
            }
        }
    }

    public ResourceType GetResourceTypeByName(string name)
    {
        if (resourcesDatabase != null && resourcesDatabase.allResources != null)
        {
            foreach (var resource in resourcesDatabase.allResources)
            {
                if (resource != null && resource.name == name)
                {
                    return resource;
                }
            }
        }
        
        var loadedResource = Resources.Load<ResourceType>(name);
        if (loadedResource != null) return loadedResource;

        Debug.LogWarning($"Resource {name} not found!");
        return null;
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }

#if UNITY_EDITOR
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) SaveGame();
    }
#endif
}