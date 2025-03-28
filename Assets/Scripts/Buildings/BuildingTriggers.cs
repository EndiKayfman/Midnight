using UnityEngine;

public class BuildingTrigger : MonoBehaviour
{
    public enum TriggerType { Input, Output }
    public TriggerType triggerType;
    
    [SerializeField] private AbstractBuilding _building;

    public void Initialize(AbstractBuilding parentBuilding)
    {
        _building = parentBuilding;
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>().isTrigger = true;
        }
        else
        {
            GetComponent<Collider>().isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || _building == null) return;
        
        switch (triggerType)
        {
            case TriggerType.Input:
                _building.PlayerEnteredInput(other.transform);
                break;
            case TriggerType.Output:
                _building.PlayerEnteredOutput(other.transform);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player") || _building == null) return;
        
        switch (triggerType)
        {
            case TriggerType.Input:
                _building.PlayerExitedInput();
                break;
            case TriggerType.Output:
                _building.PlayerExitedOutput();
                break;
        }
    }

    private void OnValidate()
    {
        if (_building == null)
        {
            _building = GetComponentInParent<AbstractBuilding>();
        }
    }
}