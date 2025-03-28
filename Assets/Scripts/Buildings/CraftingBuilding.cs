using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CraftingBuilding : AbstractBuilding, ISavableBuilding
{
    [System.Serializable]
    public class CraftingRecipe
    {
        public ResourceType inputResource1;
        public int inputAmount1;
        public ResourceType inputResource2;
        public int inputAmount2;
        public ResourceType outputResource;
        public int outputAmount;
        public float craftingTime = 5f;
    }

    [Header("Crafting Settings")]
    public CraftingRecipe recipe;
    public Dictionary<ResourceType, int> storedResources = new Dictionary<ResourceType, int>();
    private bool isCrafting;

    protected override void Awake()
    {
        base.Awake();
        InitializeTriggers();
        InitializeBuilding();
    }

    private void InitializeTriggers()
    {
        foreach (Transform child in transform)
        {
            var trigger = child.GetComponent<BuildingTrigger>();
            if (trigger != null)
            {
                trigger.Initialize(this);
            }
        }
    }

    protected override void InitializeBuilding()
    {
        storedResources[recipe.inputResource1] = 0;
        storedResources[recipe.inputResource2] = 0;
        storedResources[recipe.outputResource] = 0;
    }

    protected override void HandleInputTransfer()
    {
        bool needResource1 = storedResources[recipe.inputResource1] < recipe.inputAmount1;
        bool needResource2 = storedResources[recipe.inputResource2] < recipe.inputAmount2;

        if (needResource1 && ResourceManager.Instance.GetResourceCount(recipe.inputResource1) > 0)
        {
            StartCoroutine(TransferResource(
                recipe.inputResource1,
                playerTransform.position + Vector3.up * 0.5f,
                inputResourceTarget.position,
                true
            ));
            ResourceManager.Instance.RemoveResource(recipe.inputResource1, 1);
            storedResources[recipe.inputResource1]++;
        }
        else if (needResource2 && ResourceManager.Instance.GetResourceCount(recipe.inputResource2) > 0)
        {
            StartCoroutine(TransferResource(
                recipe.inputResource2,
                playerTransform.position + Vector3.up * 0.5f,
                inputResourceTarget.position,
                true
            ));
            ResourceManager.Instance.RemoveResource(recipe.inputResource2, 1);
            storedResources[recipe.inputResource2]++;
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
        if (isCrafting) return;

        bool canCraft = storedResources[recipe.inputResource1] >= recipe.inputAmount1 &&
                       storedResources[recipe.inputResource2] >= recipe.inputAmount2;

        if (canCraft)
        {
            StartCoroutine(CraftingProcess());
        }
    }

    private IEnumerator CraftingProcess()
    {
        isCrafting = true;
        float timer = 0f;

        ShowProgressBar("Crafting...", 0f);

        while (timer < recipe.craftingTime)
        {
            if (isPlayerMoving)
            {
                yield return null;
                continue;
            }

            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / recipe.craftingTime);
            ShowProgressBar("Crafting...", progress);
            
            yield return null;
        }

        storedResources[recipe.inputResource1] -= recipe.inputAmount1;
        storedResources[recipe.inputResource2] -= recipe.inputAmount2;
        storedResources[recipe.outputResource] += recipe.outputAmount;

        HideProgressBar();
        isCrafting = false;
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