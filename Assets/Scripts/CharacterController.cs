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

    [Header("Inventory")]
    [SerializeField] private InventoryRadialUI inventoryUI;

    [Header("Pickup")]
    [SerializeField] private FlashlightBattery flashlightBattery;
    [SerializeField] private float pickupRange = 2f;
    [SerializeField] private LayerMask pickupLayers;

    [Header("Pickup Prompt")]
    [SerializeField] private GameObject pickupCanvas;
    [SerializeField] private TMP_Text pickupText;

    private CharacterController controller;
    private float cameraPitch;
    private float stepTimer;

    private PickupItem currentPickup;

    private int batteriesCollected;
    private int keysCollected;

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

        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    private void OnDisable()
    {
        DisableAction(moveAction);
        DisableAction(lookAction);
        DisableAction(interactAction);

        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    private void Update()
    {
        if (inventoryUI != null && inventoryUI.IsOpen())
            return;

        Move();
        Look();
        Footsteps();

        FindNearbyPickup();
        UpdatePickupUI();

        if (WasPressed(interactAction))
            TryPickup();
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

    private void UpdatePickupUI()
    {
        bool canPickup = currentPickup != null;

        if (pickupCanvas != null)
            pickupCanvas.SetActive(canPickup);

        if (pickupText != null && canPickup)
            pickupText.text = currentPickup.PickupMessage;
    }

    private void TryPickup()
    {
        if (currentPickup == null)
            return;

        HandlePickupEffect(currentPickup);

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
                if (flashlightBattery != null)
                    flashlightBattery.AddBattery(pickup.BatteryAmount);

                batteriesCollected++;
                Debug.Log("Picked up battery. Total batteries: " + batteriesCollected);
                break;

            case PickupType.Key:
                keysCollected++;
                Debug.Log("Picked up key: " + pickup.KeyID + ". Total keys: " + keysCollected);
                break;

            case PickupType.GenericItem:
                Debug.Log("Picked up item: " + pickup.ItemName);
                break;
        }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}