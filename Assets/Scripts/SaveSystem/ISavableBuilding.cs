public interface ISavableBuilding
{
    BuildingSaveData GetSaveData();
    void LoadSaveData(BuildingSaveData data);
    string BuildingId { get; }
}