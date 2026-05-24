using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform playerCamera;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference interactAction;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float lookSensitivity = 0.05f;

    [Header("Footsteps")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepRate = 0.45f;

    [Header("Battery Pickup")]
    [SerializeField] private FlashlightBattery flashlightBattery;
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private LayerMask pickupLayers;

    [Header("Pickup UI")]
    [SerializeField] private GameObject pickupCanvas;
    [SerializeField] private TMP_Text pickupText;
    [SerializeField] private string pickupMessage = "Press E to pick up battery";

    private CharacterController controller;
    private float cameraPitch;
    private float stepTimer;

    private BatteryItem currentBattery;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (footstepSource == null)
            footstepSource = GetComponent<AudioSource>();

        if (flashlightBattery == null)
            flashlightBattery = FindFirstObjectByType<FlashlightBattery>();

        if (pickupCanvas != null)
            pickupCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        EnableAction(moveAction);
        EnableAction(lookAction);
        EnableAction(interactAction);

        if (interactAction != null && interactAction.action != null)
            interactAction.action.performed += TryPickup;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
            interactAction.action.performed -= TryPickup;

        DisableAction(moveAction);
        DisableAction(lookAction);
        DisableAction(interactAction);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Update()
    {
        Move();
        Look();
        Footsteps();

        FindNearbyBattery();
        UpdatePickupUI();
    }

    private void Move()
    {
        Vector2 input = ReadVector2(moveAction);

        Vector3 move = transform.right * input.x + transform.forward * input.y;
        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    private void Look()
    {
        if (playerCamera == null)
            return;

        Vector2 input = ReadVector2(lookAction);

        float mouseX = input.x * lookSensitivity;
        float mouseY = input.y * lookSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -85f, 85f);

        playerCamera.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void Footsteps()
    {
        Vector2 input = ReadVector2(moveAction);

        if (input.magnitude < 0.1f)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;

        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = stepRate;
        }
    }

    private void PlayFootstep()
    {
        if (footstepSource == null)
            return;

        if (footstepClips == null || footstepClips.Length == 0)
            return;

        int randomIndex = Random.Range(0, footstepClips.Length);
        footstepSource.PlayOneShot(footstepClips[randomIndex]);
    }

    private void FindNearbyBattery()
    {
        currentBattery = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange, pickupLayers);

        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < hits.Length; i++)
        {
            BatteryItem battery = hits[i].GetComponentInParent<BatteryItem>();

            if (battery == null)
                continue;

            float distance = Vector3.Distance(transform.position, battery.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentBattery = battery;
            }
        }
    }

    private void UpdatePickupUI()
    {
        bool canPickup = currentBattery != null;

        if (pickupCanvas != null)
            pickupCanvas.SetActive(canPickup);

        if (pickupText != null && canPickup)
            pickupText.text = pickupMessage;
    }
    private void TryPickup(InputAction.CallbackContext context)
    {
        if (currentBattery == null)
        {
            Debug.Log("No battery nearby.");
            return;
        }

        if (flashlightBattery == null)
        {
            Debug.LogWarning("No FlashlightBattery assigned on player controller.");
            return;
        }

        Debug.Log("Picking up battery: " + currentBattery.name);

        flashlightBattery.AddBattery(currentBattery.BatteryAmount);

        if (currentBattery.PickupSound != null)
            AudioSource.PlayClipAtPoint(currentBattery.PickupSound, currentBattery.transform.position);

        currentBattery.DestroyBattery();

        currentBattery = null;

        if (pickupCanvas != null)
            pickupCanvas.SetActive(false);
    }

    private Vector2 ReadVector2(InputActionReference actionReference)
    {
        if (actionReference == null || actionReference.action == null)
            return Vector2.zero;

        return actionReference.action.ReadValue<Vector2>();
    }

    private void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
            actionReference.action.Enable();
    }

    private void DisableAction(InputActionReference actionReference)
    {
        if (actionReference != null && actionReference.action != null)
            actionReference.action.Disable();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}