using System.Xml;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class NewMove : MonoBehaviour
{
    public float speed = 5;
    public float turnAroundSpeed = 5;

    public float gravity;
    public float jumpPower;
    
    private InputSystem _inputSystem;
    private CharacterController _characterController;
    private float _verticalVelocity;

    private bool _jumping;

    private bool Grounded => CheckIfGrounded();


    private bool CheckIfGrounded()
    {
        LayerMask mask = LayerMask.GetMask("Default");
        return Physics.Raycast(transform.position, Vector3.down, _characterController.height/2 + 0.1f, layerMask:mask);
    }
    
    

    private void Awake()
    {
        _inputSystem = new InputSystem();
        _characterController = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        _inputSystem.Player.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _inputSystem.Player.Jump.started += StartJumping;
        _inputSystem.Player.Jump.canceled += StopJumping;
    }

    private void OnDisable()
    {
        _inputSystem.Player.Disable();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _inputSystem.Player.Jump.started -= StartJumping;
        _inputSystem.Player.Jump.canceled -= StopJumping;
    }

    private void Update()
    {
        Movement();
        Look();
        if(Grounded)
            print(Grounded);
    }

    private void Movement()
    {
        if (_jumping && Grounded) _verticalVelocity = jumpPower;
        _verticalVelocity -= gravity * Time.deltaTime;
        _characterController.Move( _verticalVelocity * Time.deltaTime * Vector3.up);
        var movementAxes = _inputSystem.Player.Movement.ReadValue<Vector2>();
        var movementAxes3D = new Vector3(movementAxes.x, 0, movementAxes.y);
        var movementDir = transform.rotation * movementAxes3D;
        var movement = Time.deltaTime * speed * movementDir;
        _characterController.Move(movement);

    }

    private void Look()
    {
        var yaw = _inputSystem.Player.Look.ReadValue<float>() * Time.deltaTime * turnAroundSpeed;
        transform.rotation = Quaternion.Euler( transform.rotation.eulerAngles + Vector3.up * yaw);
    }

    private void StartJumping(InputAction.CallbackContext _)
    {
        _jumping = true;
    }

    private void StopJumping(InputAction.CallbackContext _)
    {
        _jumping = false;
    }
}
