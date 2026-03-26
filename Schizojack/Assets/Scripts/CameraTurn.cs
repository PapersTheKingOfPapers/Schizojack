using UnityEngine;
using UnityEngine.InputSystem;

public class CameraTurn : MonoBehaviour //Change to NetworksBehaviour when MP
{
    [Header("Settings")]
    [SerializeField] private Transform _Head;
    [SerializeField] private float _turnForce = 1f;
    [SerializeField] private float _SmoothTime = 0.15f;
    [Header("Input")]
    [SerializeField] private InputActionAsset Assets;
    private InputAction _look;

    private Vector2 look;
    private Vector3 _baseRotation;
    private Vector3 _currentRotation;
    private Vector3 _currentVelocity;

    private void OnDisable()
    {
        if (_look != null)
            _look.Disable();
    }
    //Change into public override void OnNetworkSpawn() when mp
    void Start()
    {
        //Checks if the client is the owner of this script
        //If not, it returns and disables the camera so it won't use it
        /*if (!IsOwner)
        {
            _Camera.gameObject.SetActive(false);
            return;
        }*/

        //Checks for gameObjects with a tag

        //Makes the target the Gameobject with tag
        
        //Inputs
        _look = Assets.FindAction("Player/Look");
        _look.Enable();

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    void Update()
    {
        //Checks if the client is the owner of this script
        //If not, it returns
        //if (!IsOwner) return;

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
    }
}
