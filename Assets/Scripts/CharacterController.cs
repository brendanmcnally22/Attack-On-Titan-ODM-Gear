using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float lookSensitivity = 0.05f;

    private CharacterController controller;
    private float cameraPitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        Move();
        Look();
    }

    private void Move()
    {
        Vector2 input = moveAction.action.ReadValue<Vector2>();

        Vector3 move = transform.right * input.x + transform.forward * input.y;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    private void Look()
    {
        Vector2 input = lookAction.action.ReadValue<Vector2>();

        float mouseX = input.x * lookSensitivity;
        float mouseY = input.y * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -85f, 85f);

        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }
}