using UnityEngine;

public abstract class SavableBuilding : MonoBehaviour
{
    [Header("Base Building Settings")]
    [SerializeField] protected string buildingId;
    [SerializeField] protected GameObject constructionModel;
    [SerializeField] protected GameObject completedModel;

    protected bool isConstructed = false;

    public string BuildingId 
    {
        get
        {
            if (string.IsNullOrEmpty(buildingId))
            {
                buildingId = $"{gameObject.name}_{transform.position}";
            }
            return buildingId;
        }
    }

    protected void InitializeVisuals()
    {
        if (constructionModel != null) 
            constructionModel.SetActive(!isConstructed);
        if (completedModel != null) 
            completedModel.SetActive(isConstructed);
    }

    public abstract BuildingSaveData GetSaveData();
    public abstract void LoadSaveData(BuildingSaveData data);
}