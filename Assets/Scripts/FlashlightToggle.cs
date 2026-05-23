using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightToggle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light flashlight;

    [Header("Input")]
    [SerializeField] private InputActionReference flashlightAction;

    [Header("Settings")]
    [SerializeField] private bool startOn = true;

    private bool isOn;

    private void Awake()
    {
        if (flashlight == null)
            flashlight = GetComponent<Light>();

        isOn = startOn;

        if (flashlight != null)
            flashlight.enabled = isOn;
    }

    private void OnEnable()
    {
        if (flashlightAction != null && flashlightAction.action != null)
        {
            flashlightAction.action.Enable();
            flashlightAction.action.performed += ToggleFlashlight;
        }
    }

    private void OnDisable()
    {
        if (flashlightAction != null && flashlightAction.action != null)
        {
            flashlightAction.action.performed -= ToggleFlashlight;
            flashlightAction.action.Disable();
        }
    }

    private void ToggleFlashlight(InputAction.CallbackContext context)
    {
        if (flashlight == null)
            return;

        isOn = !isOn;
        flashlight.enabled = isOn;
    }
}