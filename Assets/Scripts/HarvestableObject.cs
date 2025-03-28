using UnityEngine;
using UnityEngine.UI;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SphereCollider))]
public class HarvestableObject : MonoBehaviour
{
    [System.Serializable]
    public class HarvestSettings
    {
        [Tooltip("Количество ударов для добычи")]
        public int hitsRequired = 10;
        
        [Tooltip("Задержка между ударами")]
        public float hitDelay = 0.5f;
        
        [Tooltip("Количество получаемого ресурса")]
        public int amount = 1;
        
        [Tooltip("Тип ресурса"), ResourceTypeSelector]
        public ResourceType resourceType;
    }

    [Header("Main Settings")]
    [SerializeField] private HarvestSettings _harvestSettings;
    [SerializeField] private float _harvestRadius = 2f;

    [Header("Visual Effects")]
    [SerializeField] private GameObject _harvestEffect;
    [SerializeField] private GameObject _hitEffect;

    [Header("Progress Bar")]
    [SerializeField] private GameObject _progressBarPrefab;
    [SerializeField] private Vector3 _progressBarOffset = new Vector3(0, 2f, 0);
    [SerializeField] private Color _fullColor = Color.green;
    [SerializeField] private Color _emptyColor = Color.red;
    [SerializeField] private TMP_Text _hitsText;

    private int _currentHits;
    private float _nextHitTime;
    private GameObject _progressBarInstance;
    private Image _progressFill;
    private bool _isPlayerInRange;
    private SphereCollider _collider;
    private Transform _playerTransform;
    private Vector3 _lastPlayerPosition;
    private bool _isPlayerMoving;

    private void Awake()
    {
        _collider = GetComponent<SphereCollider>();
        _collider.radius = _harvestRadius;
        _collider.isTrigger = true;
    }

    private void Update()
    {
        CheckPlayerMovement();
        
        if (!_isPlayerInRange || _isPlayerMoving) return;

        if (Time.time >= _nextHitTime)
        {
            ProcessHit();
            _nextHitTime = Time.time + _harvestSettings.hitDelay;
        }
    }

    private void CheckPlayerMovement()
    {
        if (_playerTransform == null) return;
        
        Vector3 currentPosition = _playerTransform.position;
        _isPlayerMoving = Vector3.Distance(_lastPlayerPosition, currentPosition) > 0.1f;
        _lastPlayerPosition = currentPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _isPlayerInRange = true;
        _playerTransform = other.transform;
        _lastPlayerPosition = _playerTransform.position;
        CreateProgressBar();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _isPlayerInRange = false;
        _playerTransform = null;
        DestroyProgressBar();
    }

    private void CreateProgressBar()
    {
        if (_progressBarPrefab == null) return;

        _progressBarInstance = Instantiate(_progressBarPrefab, 
            transform.position + _progressBarOffset, 
            Quaternion.identity, 
            transform);

        _progressFill = _progressBarInstance.GetComponentInChildren<Image>();
        UpdateProgressBar(1f);
        
        _nextHitTime = Time.time + _harvestSettings.hitDelay;
    }

    private void DestroyProgressBar()
    {
        if (_progressBarInstance != null)
        {
            Destroy(_progressBarInstance);
        }
    }

    private void ProcessHit()
    {
        _currentHits++;
        float progress = 1f - (float)_currentHits / _harvestSettings.hitsRequired;
        UpdateProgressBar(progress);

        if (_hitEffect != null)
            Instantiate(_hitEffect, transform.position, Quaternion.identity);

        if (_currentHits >= _harvestSettings.hitsRequired)
            CompleteHarvest();
    }

    private void UpdateProgressBar(float progress)
    {
        if (_progressFill != null)
        {
            _progressFill.fillAmount = progress;
            _progressFill.color = Color.Lerp(_emptyColor, _fullColor, progress);
        }

        if (_hitsText != null)
            _hitsText.text = $"{_currentHits}/{_harvestSettings.hitsRequired}";
    }

    private void CompleteHarvest()
    {
        if (_harvestEffect != null)
            Instantiate(_harvestEffect, transform.position, Quaternion.identity);

        ResourceManager.Instance.AddResource(
            _harvestSettings.resourceType,
            _harvestSettings.amount
        );
        Debug.Log($"Total: {ResourceManager.Instance.GetResourceCount( _harvestSettings.resourceType)}");

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ResourceTypeSelectorAttribute))]
    public class ResourceTypeSelectorDrawer : PropertyDrawer
    {
        private ResourcesDatabase _database;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_database == null)
                _database = Resources.Load<ResourcesDatabase>("ResourcesDatabase");

            if (_database == null || _database.allResources.Length == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            int currentIndex = GetCurrentIndex(property.objectReferenceValue as ResourceType);
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, GetResourceNames());

            if (newIndex != currentIndex)
            {
                property.objectReferenceValue = _database.allResources[newIndex];
            }
        }

        private int GetCurrentIndex(ResourceType type)
        {
            for (int i = 0; i < _database.allResources.Length; i++)
                if (_database.allResources[i] == type) return i;
            return 0;
        }

        private string[] GetResourceNames()
        {
            string[] names = new string[_database.allResources.Length];
            for (int i = 0; i < names.Length; i++)
                names[i] = _database.allResources[i].displayName;
            return names;
        }
    }
#endif
}

public class ResourceTypeSelectorAttribute : PropertyAttribute { }