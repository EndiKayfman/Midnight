using System.Collections.Generic;

[System.Serializable]
public class ResourceSaveData
{
    public string resourceName;
    public int amount;
}

[System.Serializable]
public class BuildingSaveData
{
    public string buildingId;
    public bool isConstructed;
    public List<ResourceSaveData> storedResources = new List<ResourceSaveData>();
}

[System.Serializable]
public class GameSaveData
{
    public List<ResourceSaveData> playerInventory = new List<ResourceSaveData>();
    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
}