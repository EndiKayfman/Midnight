using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class BuildingConstructionSystem : MonoBehaviour, ISavableBuilding
{
    [System.Serializable]
    public class ConstructionStage
    {
        public ResourceType resourceType;
        public int requiredAmount;
        public float transferDelay = 0.5f;
    }

    [Header("Construction Settings")] 
    public List<ConstructionStage> constructionStages;
    public int totalHitsRequired = 10;
    public float hitCooldown = 0.5f;
    public float movementThreshold = 0.1f;

    [Header("Visual References")] 
    public GameObject constructionModel;
    public GameObject completedModel;
    public Transform resourceTargetPoint;
    public ProgressBarController progressBar;
    public ParticleSystem buildEffect;
    public float resourceFlySpeed = 5f;
    public float resourceFlyHeight = 1.5f;

    [Header("Save Settings")]
    [SerializeField] private string _buildingId;
    public string BuildingId => _buildingId;
    
    private Dictionary<ResourceType, int> _depositedResources;
    [SerializeField] private int _currentHits;
    private bool _isPlayerInRange;
    private bool _isPlayerMoving;
    private Transform _playerTransform;
    private Vector3 _lastPlayerPosition;
    private Coroutine _constructionRoutine;
    private GameObject _currentFlyingResource;
    
    public bool IsConstructionComplete { get; private set; }

    public delegate void BuildingCompleted();
    public event BuildingCompleted OnBuildingCompleted;

    private void Awake()
    {
        if (string.IsNullOrEmpty(_buildingId))
        {
            _buildingId = System.Guid.NewGuid().ToString();
        }

        InitializeConstruction();
    }

    public void InitializeConstruction()
    {
        if (IsConstructionComplete)
        {
            CompleteConstruction();
            return;
        }

        constructionModel.SetActive(true);
        completedModel.SetActive(false);
        progressBar?.gameObject.SetActive(false);
    
        _depositedResources = new Dictionary<ResourceType, int>();
        foreach (var stage in constructionStages)
        {
            _depositedResources[stage.resourceType] = 0;
        }
    
        enabled = true;
        GetComponent<Collider>().enabled = true;
    }
    
    private void CheckPlayerMovement()
    {
        if (_playerTransform == null) return;
        
        Vector3 currentPosition = _playerTransform.position;
        _isPlayerMoving = Vector3.Distance(_lastPlayerPosition, currentPosition) > movementThreshold;
        _lastPlayerPosition = currentPosition;
    }
    
    private void Update()
    {
        CheckPlayerMovement();
        
        if (!_isPlayerInRange || _playerTransform == null) return;
        
        if (_isPlayerInRange && !_isPlayerMoving)
        {
            StartConstructionProcess();
        }
        else if (_isPlayerMoving && _constructionRoutine != null)
        {
            StopConstructionProcess();
        }
    }

    private void StartConstructionProcess()
    {
        if (_constructionRoutine == null && !IsConstructionComplete)
        {
            _constructionRoutine = StartCoroutine(ConstructionProcess());
        }
    }

    private void StopConstructionProcess()
    {
        if (_constructionRoutine != null)
        {
            StopCoroutine(_constructionRoutine);
            _constructionRoutine = null;

            if (_currentFlyingResource != null)
            {
                Destroy(_currentFlyingResource);
                _currentFlyingResource = null;
            }
        }
    }

    private IEnumerator ConstructionProcess()
    {
        while (!AllResourcesDeposited())
        {
            if (_isPlayerMoving) yield break;

            bool transferred = false;

            foreach (var stage in constructionStages)
            {
                if (_depositedResources[stage.resourceType] < stage.requiredAmount &&
                    ResourceManager.Instance.GetResourceCount(stage.resourceType) > 0)
                {
                    ResourceManager.Instance.RemoveResource(stage.resourceType, 1);
                    _depositedResources[stage.resourceType]++;
                    transferred = true;

                    yield return AnimateResourceTransfer(stage.resourceType);
                    yield return new WaitForSeconds(stage.transferDelay);
                    break;
                }
            }

            if (!transferred) yield return null;
        }
        
        if (AllResourcesDeposited())
        {
            progressBar?.gameObject.SetActive(true);

            while (_currentHits < totalHitsRequired)
            {
                if (_isPlayerMoving) yield break;

                _currentHits++;
                progressBar?.UpdateProgress((float)_currentHits / totalHitsRequired);
                yield return new WaitForSeconds(hitCooldown);
            }

            CompleteConstruction();
        }

        _constructionRoutine = null;
    }

    private IEnumerator AnimateResourceTransfer(ResourceType type)
    {
        if (type.flyingVisualPrefab == null || _playerTransform == null) yield break;

        _currentFlyingResource = Instantiate(
            type.flyingVisualPrefab,
            _playerTransform.position + Vector3.up * 0.5f,
            Quaternion.identity
        );

        Vector3 startPos = _currentFlyingResource.transform.position;
        Vector3 endPos = resourceTargetPoint.position;
        float duration = Vector3.Distance(startPos, endPos) / resourceFlySpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (_isPlayerMoving)
            {
                Destroy(_currentFlyingResource);
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float height = Mathf.Sin(t * Mathf.PI) * resourceFlyHeight;

            _currentFlyingResource.transform.position = Vector3.Lerp(startPos, endPos, t) + Vector3.up * height;
            yield return null;
        }

        Destroy(_currentFlyingResource);
        _currentFlyingResource = null;
    }

    private bool AllResourcesDeposited()
    {
        foreach (var stage in constructionStages)
        {
            if (_depositedResources[stage.resourceType] < stage.requiredAmount)
                return false;
        }
        return true;
    }

    public void CompleteConstruction()
    {
        constructionModel.SetActive(false);
        completedModel.SetActive(true);
        progressBar?.gameObject.SetActive(false);

        if (buildEffect != null)
        {
            buildEffect?.Play();
        }
        
        var buildingFunctionality = GetComponent<BuildingFunctionality>();
        if (buildingFunctionality != null)
        {
            buildingFunctionality.Activate();
        }
        
        enabled = false;
        GetComponent<Collider>().enabled = false;
        
        IsConstructionComplete = true;
        OnBuildingCompleted?.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _isPlayerInRange = true;
        _playerTransform = other.transform;
        _lastPlayerPosition = _playerTransform.position;
        _isPlayerMoving = false;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _isPlayerInRange = false;
        _playerTransform = null;
        StopConstructionProcess();
    }

    #region Saving/Loading

    public BuildingSaveData GetSaveData()
    {
        var data = new BuildingSaveData
        {
            buildingId = this.BuildingId,
            isConstructed = this.IsConstructionComplete
        };
        
        if (!IsConstructionComplete)
        {
            foreach (var stage in constructionStages)
            {
                data.storedResources.Add(new ResourceSaveData
                {
                    resourceName = stage.resourceType.name,
                    amount = _depositedResources[stage.resourceType]
                });
            }
            
            data.storedResources.Add(new ResourceSaveData
            {
                resourceName = "ConstructionHits",
                amount = _currentHits
            });
        }
        else
        {
            var buildingFunctionality = GetComponent<BuildingFunctionality>();
            if (buildingFunctionality != null)
            {
                data.storedResources = buildingFunctionality.GetStoredResourcesForSave();
            }
        }
        
        return data;
    }

    public void LoadSaveData(BuildingSaveData data)
    {
        if (data.isConstructed)
        {
            IsConstructionComplete = true;
            constructionModel.SetActive(false);
            completedModel.SetActive(true);
            enabled = false;
            GetComponent<Collider>().enabled = false;
            
            var buildingFunctionality = GetComponent<BuildingFunctionality>();
            if (buildingFunctionality != null)
            {
                buildingFunctionality.LoadStoredResources(data.storedResources);
                buildingFunctionality.Activate();
            }
        }
        else
        {
            foreach (var resourceData in data.storedResources)
            {
                if (resourceData.resourceName == "ConstructionHits")
                {
                    _currentHits = resourceData.amount;
                    continue;
                }
                
                var resourceType = SaveLoadManager.Instance.GetResourceTypeByName(resourceData.resourceName);
                if (resourceType != null && _depositedResources.ContainsKey(resourceType))
                {
                    _depositedResources[resourceType] = resourceData.amount;
                }
            }
            
            UpdateConstructionProgressVisual();
        }
    }

    private void UpdateConstructionProgressVisual()
    {
        if (progressBar != null)
        {
            float totalResources = 0;
            float deposited = 0;
            
            foreach (var stage in constructionStages)
            {
                totalResources += stage.requiredAmount;
                deposited += _depositedResources[stage.resourceType];
            }
            
            float progress = deposited / totalResources;
            progressBar.UpdateProgress(progress);
        }
    }

    #endregion
}