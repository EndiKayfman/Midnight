using UnityEngine;
using System.Collections;

public abstract class AbstractBuilding : MonoBehaviour
{
    [Header("Base Settings")]
    public float transferDelay = 0.5f;
    public float movementThreshold = 0.1f;
    public float resourceFlySpeed = 5f;
    public float resourceFlyHeight = 1.5f;

    [Header("Base References")]
    public Transform inputResourceTarget;
    public Transform outputResourceTarget;
    public GameObject progressBarPrefab;
    public Vector3 progressBarOffset = new Vector3(0, 3f, 0);

    protected enum TriggerType { Input, Output }
    
    protected bool isPlayerInInputZone;
    protected bool isPlayerInOutputZone;
    protected bool isPlayerMoving;
    protected Transform playerTransform;
    protected Vector3 lastPlayerPosition;
    protected Coroutine currentProcess;
    protected GameObject currentFlyingResource;
    protected bool isTransferInProgress;
    protected ProgressBarController currentProgressBar;
    
    [Header("Save Settings")]
    [SerializeField] private string _buildingId;
    public string BuildingId => _buildingId;
    
    protected abstract void HandleInputTransfer();
    protected abstract void HandleOutputTransfer();
    protected abstract void CheckProduction();

    protected virtual void Awake()
    {
        if (string.IsNullOrEmpty(_buildingId))
        {
            _buildingId = System.Guid.NewGuid().ToString();
        }
        
        InitializeBuilding();
    }
    
    
    
    public virtual BuildingSaveData GetSaveData()
    {
        return new BuildingSaveData
        {
            buildingId = this.BuildingId,
            isConstructed = false
        };
    }

    public virtual void LoadSaveData(BuildingSaveData data)
    {
       
    }

    protected virtual void InitializeBuilding()
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

    protected virtual void Update()
    {
        if (playerTransform == null) return;

        UpdatePlayerMovement();
        HandleResourceTransfer();
    }

    protected void UpdatePlayerMovement()
    {
        isPlayerMoving = Vector3.Distance(lastPlayerPosition, playerTransform.position) > movementThreshold;
        lastPlayerPosition = playerTransform.position;
    }

    protected void HandleResourceTransfer()
    {
        if (isPlayerMoving)
        {
            StopCurrentProcess();
            return;
        }

        if (isPlayerInInputZone && currentProcess == null)
        {
            currentProcess = StartCoroutine(ResourceTransferProcess());
        }
        else if (isPlayerInOutputZone && currentProcess == null)
        {
            currentProcess = StartCoroutine(ResourceTransferProcess());
        }
    }

    protected virtual IEnumerator ResourceTransferProcess()
    {
        while ((isPlayerInInputZone || isPlayerInOutputZone) && playerTransform != null)
        {
            if (isPlayerMoving || isTransferInProgress)
            {
                yield return null;
                continue;
            }

            bool transferred = false;

            if (isPlayerInInputZone)
            {
                HandleInputTransfer();
                transferred = true;
            }
            else if (isPlayerInOutputZone)
            {
                HandleOutputTransfer();
                transferred = true;
            }

            CheckProduction();

            yield return transferred ? new WaitForSeconds(transferDelay) : null;
        }

        currentProcess = null;
    }

    protected IEnumerator TransferResource(ResourceType type, Vector3 from, Vector3 to, bool isInput)
    {
        isTransferInProgress = true;
        
        currentFlyingResource = Instantiate(type.flyingVisualPrefab, from, Quaternion.identity);
        
        float distance = Vector3.Distance(from, to);
        float duration = distance / resourceFlySpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (ShouldStopTransfer(isInput))
            {
                Destroy(currentFlyingResource);
                isTransferInProgress = false;
                yield break;
            }

            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float height = Mathf.Sin(t * Mathf.PI) * resourceFlyHeight;
            currentFlyingResource.transform.position = Vector3.Lerp(from, to, t) + Vector3.up * height;
            
            yield return null;
        }

        Destroy(currentFlyingResource);
        isTransferInProgress = false;
    }

    protected void ShowProgressBar(string text, float progress)
    {
        if (progressBarPrefab == null) return;

        if (currentProgressBar == null)
        {
            var progressBarObj = Instantiate(progressBarPrefab, 
                transform.position + progressBarOffset, 
                Quaternion.identity);
            currentProgressBar = progressBarObj.GetComponent<ProgressBarController>();
        }

        //currentProgressBar.SetText(text);
        currentProgressBar.UpdateProgress(progress);
    }

    protected void HideProgressBar()
    {
        if (currentProgressBar != null)
        {
            Destroy(currentProgressBar.gameObject);
            currentProgressBar = null;
        }
    }

    protected bool ShouldStopTransfer(bool isInput)
    {
        return isPlayerMoving || 
              (isInput && !isPlayerInInputZone) || 
              (!isInput && !isPlayerInOutputZone);
    }

    protected void StopCurrentProcess()
    {
        if (currentProcess != null)
        {
            StopCoroutine(currentProcess);
            currentProcess = null;
        }

        if (currentFlyingResource != null)
        {
            Destroy(currentFlyingResource);
            currentFlyingResource = null;
        }

        HideProgressBar();
        isTransferInProgress = false;
    }
    
    public void PlayerEnteredInput(Transform player)
    {
        playerTransform = player;
        lastPlayerPosition = player.position;
        isPlayerInInputZone = true;
        StartResourceProcess();
    }

    public void PlayerEnteredOutput(Transform player)
    {
        playerTransform = player;
        lastPlayerPosition = player.position;
        isPlayerInOutputZone = true;
        StartResourceProcess();
    }

    public void PlayerExitedInput()
    {
        isPlayerInInputZone = false;
        CheckPlayerExit();
    }

    public void PlayerExitedOutput()
    {
        isPlayerInOutputZone = false;
        CheckPlayerExit();
    }

    protected void StartResourceProcess()
    {
        if (currentProcess == null)
        {
            currentProcess = StartCoroutine(ResourceTransferProcess());
        }
    }

    protected void CheckPlayerExit()
    {
        if (!isPlayerInInputZone && !isPlayerInOutputZone)
        {
            playerTransform = null;
            StopCurrentProcess();
        }
    }

    protected virtual void OnDisable()
    {
        StopCurrentProcess();
    }
}