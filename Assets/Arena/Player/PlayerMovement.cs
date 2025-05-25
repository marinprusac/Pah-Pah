using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Arena.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : NetworkBehaviour
    { 
        [SerializeField]
        private Animator animator;


        

        [FormerlySerializedAs("Aim")] [SerializeField]
        private Transform aim;
        
        [FormerlySerializedAs("Body")] [SerializeField]
        private Transform body;
        
        public float speed = 5;
        public float turnAroundSpeed = 1;

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

            var raycastOriginOffsets = new List<Vector3>
            {
                Vector3.zero,
                Vector3.forward * _characterController.radius,
                Vector3.back * _characterController.radius,
                Vector3.left * _characterController.radius,
                Vector3.right * _characterController.radius
            };

            return raycastOriginOffsets.Any(offset => Physics.Raycast(transform.position + offset, Vector3.down, _characterController.height / 2 + 0.1f, layerMask: mask));
        }

        private void Awake()
        {
            _inputSystem = new InputSystem();
            _characterController = GetComponent<CharacterController>();
        }


        private void Start()
        {
            _movementDirection = transform.rotation * Vector3.forward;
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


        private float _grace = 1f;
        
        private void Update()
        {

            if (_grace > 0)
            {
                _grace -= Time.deltaTime;
                return;
            }
            if (IsOwner && IsSpawned)
            {
                Movement();
                Look();
            }
        }

        private bool _jumped;
        private Vector2 _lookingDirection = Vector2.zero;
        private Vector3 _movementDirection;
        private float MovementYaw => Vector3.SignedAngle(Vector3.forward, _movementDirection, Vector3.up);

        private void Movement()
        {
            _verticalVelocity -= gravity * Time.deltaTime;
            if (Grounded)
            {
                if (_jumping)
                {
                    _verticalVelocity = jumpPower;
                    _jumped = true;
                }
                else
                {
                    if(_jumped)
                        PlayAnimationRpc("Landed");
                        //animator.SetTrigger("Landed");
                    _jumped = false;

                    _verticalVelocity = 0;
                }
            }
            _characterController.Move( _verticalVelocity * Time.deltaTime * Vector3.up);
            
            var movementAxes = _inputSystem.Player.Movement.ReadValue<Vector2>();
            var movementAxes3D = new Vector3(movementAxes.x, 0, movementAxes.y);
            var yawRotation = Quaternion.Euler(0, _lookingDirection.y, 0);
            var movementDir = yawRotation * movementAxes3D;

            if (movementDir.magnitude > 0.01f)
            {
                _movementDirection = movementDir;
            }
            
            
            PlayAnimationRpc("Speed", movementAxes.magnitude);
            PlayAnimationRpc("Vertical", _verticalVelocity);
            
            // animator.SetFloat("Speed", movementAxes.magnitude);
            // animator.SetFloat("Vertical", _verticalVelocity);

            
            var movement = Time.deltaTime * speed * movementDir;
            _characterController.Move(movement);

        }
        
        private void Look()
        {
            var lookMove = _inputSystem.Player.Look.ReadValue<Vector2>() * turnAroundSpeed;
            _lookingDirection = new Vector2(Mathf.Clamp(_lookingDirection.x-lookMove.y, -80, 80), _lookingDirection.y + lookMove.x);
            aim.localPosition = Quaternion.Euler(_lookingDirection.x-30, _lookingDirection.y, 0) * (5 * Vector3.forward);
            aim.rotation = Quaternion.LookRotation(aim.localPosition, Vector3.up);
            body.rotation = Quaternion.Slerp(body.rotation, Quaternion.Euler(0, MovementYaw, 0), 1-Mathf.Pow(0.01f, Time.deltaTime));

        }

        private void StartJumping(InputAction.CallbackContext _)
        {
            _jumping = true;
        }

        private void StopJumping(InputAction.CallbackContext _)
        {
            _jumping = false;
        }


        [Rpc(SendTo.Server)]
        private void PlayAnimationRpc(string triggerName, float value=float.NaN)
        {
            if(float.IsNaN(value)) animator.SetTrigger(triggerName);
            else animator.SetFloat(triggerName, value);
        }
    }
}
