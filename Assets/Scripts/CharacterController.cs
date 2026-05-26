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

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float groundedGravity = -2f;

    [Header("Footsteps")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepRate = 0.45f;

    [Header("Systems")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private InventoryRadialUI inventoryUI;
    [SerializeField] private FlashlightBattery flashlightBattery;

    [Header("Pickup")]
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private LayerMask pickupLayers;

    [Header("Pickup Prompt")]
    [SerializeField] private GameObject pickupCanvas;
    [SerializeField] private TMP_Text pickupText;

    private CharacterController controller;
    private PickupItem currentPickup;

    private float cameraPitch;
    private float stepTimer;
    private float verticalVelocity;

    private bool controlsEnabled = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (footstepSource == null)
            footstepSource = GetComponent<AudioSource>();

        if (playerInventory == null)
            playerInventory = GetComponent<PlayerInventory>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (inventoryUI == null)
            inventoryUI = FindFirstObjectByType<InventoryRadialUI>();

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

        LockCursor();
    }

    private void OnDisable()
    {
        DisableAction(moveAction);
        DisableAction(lookAction);
        DisableAction(interactAction);

        UnlockCursor();
    }

    private void Update()
    {
        if (!controlsEnabled)
        {
            ApplyGravityOnly();
            return;
        }

        if (inventoryUI != null && inventoryUI.IsOpen())
        {
            ApplyGravityOnly();

            if (pickupCanvas != null)
                pickupCanvas.SetActive(false);

            return;
        }

        Move();
        Look();
        HandleFootsteps();

        FindNearbyPickup();
        UpdatePickupPrompt();

        if (WasPressed(interactAction))
            TryPickup();
    }

    private void Move()
    {
        Vector2 input = ReadVector2(moveAction);

        Vector3 horizontalMove = transform.right * input.x + transform.forward * input.y;
        horizontalMove *= moveSpeed;

        HandleGravity();

        Vector3 finalMove = horizontalMove;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);
    }

    private void ApplyGravityOnly()
    {
        HandleGravity();

        Vector3 gravityMove = new Vector3(0f, verticalVelocity, 0f);
        controller.Move(gravityMove * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedGravity;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
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

    private void HandleFootsteps()
    {
        Vector2 input = ReadVector2(moveAction);

        if (input.magnitude < 0.1f || !controller.isGrounded)
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

    private void FindNearbyPickup()
    {
        currentPickup = null;

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange, pickupLayers);

        float closestDistance = Mathf.Infinity;

        for (int i = 0; i < hits.Length; i++)
        {
            PickupItem pickup = hits[i].GetComponentInParent<PickupItem>();

            if (pickup == null)
                continue;

            float distance = Vector3.Distance(transform.position, pickup.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                currentPickup = pickup;
            }
        }
    }

    private void UpdatePickupPrompt()
    {
        bool hasPickup = currentPickup != null;

        if (pickupCanvas != null)
            pickupCanvas.SetActive(hasPickup);

        if (pickupText != null && hasPickup)
            pickupText.text = currentPickup.PickupMessage;
    }

    private void TryPickup()
    {
        if (currentPickup == null)
            return;

        HandlePickupEffect(currentPickup);

        if (currentPickup.AddToInventory && inventoryUI != null)
            inventoryUI.AddPickupItem(currentPickup);

        if (currentPickup.PickupSound != null)
            AudioSource.PlayClipAtPoint(currentPickup.PickupSound, currentPickup.transform.position);

        currentPickup.DestroyPickup();
        currentPickup = null;

        if (pickupCanvas != null)
            pickupCanvas.SetActive(false);
    }

    private void HandlePickupEffect(PickupItem pickup)
    {
        switch (pickup.PickupType)
        {
            case PickupType.Battery:
                if (playerInventory != null)
                    playerInventory.AddStoredBattery();

                Debug.Log("Stored battery in inventory.");
                break;

            case PickupType.Key:
                if (playerInventory != null)
                    playerInventory.AddKey(pickup.KeyID);

                Debug.Log("Picked up key: " + pickup.KeyID);
                break;

            case PickupType.Bandage:
                if (playerInventory != null)
                    playerInventory.AddBandage();

                Debug.Log("Stored bandage in inventory.");
                break;

            case PickupType.GenericItem:
                Debug.Log("Picked up item: " + pickup.ItemName);
                break;
        }
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;

        if (pickupCanvas != null)
            pickupCanvas.SetActive(false);

        if (enabled)
            LockCursor();
        else
            UnlockCursor();
    }

    private Vector2 ReadVector2(InputActionReference actionReference)
    {
        if (actionReference == null || actionReference.action == null)
            return Vector2.zero;

        return actionReference.action.ReadValue<Vector2>();
    }

    private bool WasPressed(InputActionReference actionReference)
    {
        return actionReference != null &&
               actionReference.action != null &&
               actionReference.action.WasPressedThisFrame();
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

    private void LockCursor()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}