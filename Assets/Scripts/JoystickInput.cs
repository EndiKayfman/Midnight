using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class JoystickInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Components")]
    [SerializeField] private RectTransform _handle;
    [SerializeField] private Image _background;

    [Header("Settings")]
    [SerializeField] private float _maxRadius = 100f;
    [SerializeField] private float _deadZone = 0.2f;

    public Vector2 Direction { get; private set; }
    public bool IsActive { get; private set; }

    private void Awake()
    {
        _background.raycastTarget = true;
        ResetJoystick();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        IsActive = true;
        UpdateHandlePosition(eventData);
    }

    public void OnDrag(PointerEventData eventData) 
    {
        UpdateHandlePosition(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetJoystick();
    }

    private void UpdateHandlePosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _background.rectTransform,
            eventData.position,
            null,
            out Vector2 localPoint);

        Direction = Vector2.ClampMagnitude(localPoint, _maxRadius);
        _handle.anchoredPosition = Direction;
        
        if (Direction.magnitude < _maxRadius * _deadZone)
            Direction = Vector2.zero;
        else
            Direction /= _maxRadius;
    }

    private void ResetJoystick()
    {
        Direction = Vector2.zero;
        _handle.anchoredPosition = Vector2.zero;
        IsActive = false;
    }
}