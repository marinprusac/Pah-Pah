using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class FollowCamera : MonoBehaviour
{


    public Transform toFollow;
    public Vector3 offset = new(0, 3.2f, 1.232f);

    private float _angle = 0;

    private InputSystem _inputSystem;
    
    private void Awake()
    {
        _inputSystem = new InputSystem();
    }

    private void OnEnable()
    {
        _inputSystem.Camera.Enable();
    }

    private void OnDisable()
    {
        _inputSystem.Camera.Disable();
    }

    void Update()
    {
        transform.position = toFollow.position + toFollow.rotation * offset;
        transform.rotation = Quaternion.Euler(_angle, toFollow.rotation.eulerAngles.y, 0);
        Movement();
    }

    private void Movement()
    {
        var value = _inputSystem.Camera.Look.ReadValue<float>();
        _angle -= value / 20f;
        _angle = Mathf.Clamp(_angle, -90, 90);
    }
}
