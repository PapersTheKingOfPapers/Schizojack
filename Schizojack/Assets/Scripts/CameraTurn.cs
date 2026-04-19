using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTurn : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private Transform _Head;
    [SerializeField] private float _turnForce = 1f;
    [SerializeField] private float _SmoothTime = 0.15f;
    [SerializeField] private Camera _Camera;
    [Header("Input")]
    [SerializeField] private int _defaultFov = 80;
    [SerializeField] private int _zoomedFov = 50;
    [SerializeField] private InputActionAsset Assets;
    private InputAction _look;
    private InputAction _zoom;

    private Vector2 look;
    private Vector3 _baseRotation;
    private Vector3 _currentRotation;
    private Vector3 _currentVelocity;

    public override void OnNetworkDespawn()
    {
        if (_look != null)
            _look.Disable();
    }
    public override void OnNetworkSpawn()
    {
        //Checks if the client is the owner of this script
        //If not, it returns and disables the camera so it won't use it
        _Camera = _Head.GetComponentInChildren<Camera>();

        if (!IsOwner)
        {
            //_Head.gameObject.SetActive(false);
            _Camera.gameObject.SetActive(false);
            return;
        }

        _Camera.fieldOfView = _defaultFov;

        _baseRotation = _Head.transform.forward;

        //Inputs
        _look = Assets.FindAction("Player/Look");
        _look.Enable();

        _zoom = Assets.FindAction("Player/Space");
        _zoom.Enable();

        Cursor.lockState = CursorLockMode.Confined;
        _baseRotation = _Head.transform.rotation.eulerAngles;
    }

    void LateUpdate()
    {
        //Checks if the client is the owner of this script
        //If not, it returns
        if (!IsOwner) return;

        look = _look.ReadValue<Vector2>();

        //Sets the Target Rotation
        Vector3 targetRotation = _baseRotation + new Vector3
            (((look.y - (Screen.height/2)) / Screen.height) * -_turnForce,
            ((look.x - (Screen.width / 2)) / Screen.width) * _turnForce,
            0);

        //Smooths the Camera Rotation
        _currentRotation = Vector3.SmoothDamp(
            _currentRotation,
            targetRotation,
            ref _currentVelocity,
            _SmoothTime);

        //Applies the Camera Rotation
        _Head.rotation = Quaternion.Euler(_currentRotation);

        //Zoom the camera
        _Camera.fieldOfView = _zoom.IsPressed() ? _zoomedFov : _defaultFov;
    }
}
