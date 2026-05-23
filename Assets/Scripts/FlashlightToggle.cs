using UnityEngine;
using UnityEngine.InputSystem;

public class FlashlightToggle : MonoBehaviour
{
    [SerializeField] private Light flashlight;
    [SerializeField] private InputActionReference flashlightAction;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip toggleSound;

    private void Awake()
    {
        if (flashlight == null)
            flashlight = GetComponent<Light>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        flashlightAction.action.Enable();
        flashlightAction.action.performed += ToggleFlashlight;
    }

    private void OnDisable()
    {
        flashlightAction.action.performed -= ToggleFlashlight;
        flashlightAction.action.Disable();
    }

    private void ToggleFlashlight(InputAction.CallbackContext context)
    {
        flashlight.enabled = !flashlight.enabled;

        if (audioSource != null && toggleSound != null)
            audioSource.PlayOneShot(toggleSound);
    }
}