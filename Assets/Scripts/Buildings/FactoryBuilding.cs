using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FactoryBuilding : AbstractBuilding, ISavableBuilding
{
    [System.Serializable]
    public class ProcessingRecipe
    {
        public ResourceType inputResource;
        public int inputAmount;
        public ResourceType outputResource;
        public int outputAmount;
        public float processingTime = 5f;
    }

    [Header("Processing Settings")]
    public ProcessingRecipe recipe;
    public Dictionary<ResourceType, int> storedResources = new Dictionary<ResourceType, int>();
    private bool isProcessing;

    protected override void InitializeBuilding()
    {
        base.InitializeBuilding();
        
        storedResources[recipe.inputResource] = 0;
        storedResources[recipe.outputResource] = 0;
    }

    protected override void HandleInputTransfer()
    {
        if (storedResources[recipe.inputResource] < recipe.inputAmount && 
            ResourceManager.Instance.GetResourceCount(recipe.inputResource) > 0)
        {
            StartCoroutine(TransferResource(
                recipe.inputResource,
                playerTransform.position + Vector3.up * 0.5f,
                inputResourceTarget.position,
                true
            ));
            ResourceManager.Instance.RemoveResource(recipe.inputResource, 1);
            storedResources[recipe.inputResource]++;
        }
    }

    protected override void HandleOutputTransfer()
    {
        if (storedResources[recipe.outputResource] > 0)
        {
            StartCoroutine(TransferResource(
                recipe.outputResource,
                outputResourceTarget.position,
                playerTransform.position + Vector3.up * 0.5f,
                false
            ));
            storedResources[recipe.outputResource]--;
            ResourceManager.Instance.AddResource(recipe.outputResource, 1);
        }
    }

    protected override void CheckProduction()
    {
        if (isProcessing) return;

        if (storedResources[recipe.inputResource] >= recipe.inputAmount)
        {
            StartCoroutine(ProcessingProcess());
        }
    }

    private IEnumerator ProcessingProcess()
    {
        isProcessing = true;
        float timer = 0f;

        ShowProgressBar("Processing...", 0f);

        while (timer < recipe.processingTime)
        {
            if (isPlayerMoving)
            {
                yield return null;
                continue;
            }

            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / recipe.processingTime);
            ShowProgressBar("Processing...", progress);
            
            yield return null;
        }

        storedResources[recipe.inputResource] -= recipe.inputAmount;
        storedResources[recipe.outputResource] += recipe.outputAmount;

        HideProgressBar();
        isProcessing = false;
    }

    #region Saving/Loading
    public BuildingSaveData GetSaveData()
    {
        var data = new BuildingSaveData
        {
            buildingId = this.BuildingId,
            isConstructed = true
        };

        foreach (var pair in storedResources)
        {
            if (pair.Key != null && pair.Value > 0)
            {
                data.storedResources.Add(new ResourceSaveData
                {
                    resourceName = pair.Key.name,
                    amount = pair.Value
                });
            }
        }

        return data;
    }

    public void LoadSaveData(BuildingSaveData data)
    {
        foreach (var resourceData in data.storedResources)
        {
            var resourceType = SaveLoadManager.Instance.GetResourceTypeByName(resourceData.resourceName);
            if (resourceType != null && storedResources.ContainsKey(resourceType))
            {
                storedResources[resourceType] = resourceData.amount;
            }
        }
    }
    #endregion
}