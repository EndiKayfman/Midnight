using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 10f;

    [Header("References")]
    [SerializeField] private JoystickInput _joystick;
    [SerializeField] private Transform _cameraTransform;

    private Rigidbody _rigidbody;
    private Vector3 _moveDirection;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
    }

    private void Update()
    {
        if (_joystick.IsActive)
        {
            UpdateMovementDirection();
        }
        else
        {
            _moveDirection = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        MoveCharacter();
        RotateCharacter();
    }

    private void UpdateMovementDirection()
    {
        Vector2 input = _joystick.Direction;
        Vector3 forward = _cameraTransform.forward;
        Vector3 right = _cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        _moveDirection = forward * input.y + right * input.x;
    }

    private void MoveCharacter()
    {
        if (_moveDirection.magnitude > 0.1f)
        {
            Vector3 movement = _moveDirection.normalized * _moveSpeed * Time.fixedDeltaTime;
            _rigidbody.MovePosition(_rigidbody.position + movement);
        }
    }

    private void RotateCharacter()
    {
        if (_moveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_moveDirection);
            _rigidbody.rotation = Quaternion.Slerp(
                _rigidbody.rotation,
                targetRotation,
                _rotationSpeed * Time.fixedDeltaTime);
        }
    }
}