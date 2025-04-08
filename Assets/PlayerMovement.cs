using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public float speed = 5;
    [SerializeField]
    private Animator animator;
    
    private InputSystem _inputSystem;

    private bool _walking = false;
    private bool _stillWalking = false;


    private void Awake()
    {
        _inputSystem = new InputSystem();
    }

    private void OnEnable()
    {
        _inputSystem.Player.Enable();
    }

    private void OnDisable()
    {
        _inputSystem.Player.Disable();
    }

    private void Update()
    {
        _stillWalking = false;
        Look();
        Movement();
        Debug.Log(_walking + " " + _stillWalking);
        if (!_walking && _stillWalking)
        {
            _walking = true;
            animator.SetTrigger("Walk");
        }
        else if (_walking && !_stillWalking)
        {
            _walking = false;
            animator.SetTrigger("Idle");
        }
    }

    private void Movement()
    {
        var movementAxes = _inputSystem.Player.Movement.ReadValue<Vector2>();
        var movementAxes3D = new Vector3(movementAxes.x, 0, movementAxes.y);
        var movementDir = transform.rotation * movementAxes3D;
        var movement = Time.deltaTime * speed * movementDir;
        transform.position += movement;
        if (movementAxes.magnitude > 0) _stillWalking = true;
    }

    private void Look()
    {
        var value = _inputSystem.Player.Look.ReadValue<float>();
        var newAngle = transform.rotation.eulerAngles.y + value / 20f;
        transform.rotation = Quaternion.Euler(0, newAngle, 0);
        if (value != 0) _stillWalking = true;

    }
}
