using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Основные настройки")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _offset = new Vector3(0f, 2f, -5f);
    [SerializeField] private float _smoothSpeed = 5f;

    private void LateUpdate()
    {
        if (_target == null)
        {
            Debug.LogWarning("Target не назначен!");
            return;
        }
        
        Vector3 desiredPosition = _target.position + _offset;
        
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            _smoothSpeed * Time.deltaTime
        );
        
        transform.position = smoothedPosition;
        
        transform.LookAt(_target);
    }
    
    public void SetTarget(Transform newTarget) => _target = newTarget;
}