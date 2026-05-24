using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FlashlightBattery : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light flashlight;
    [SerializeField] private Slider batterySlider;

    [Header("Input")]
    [SerializeField] private InputActionReference flashlightAction;

    [Header("Battery")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float drainPerSecond = 4f;
    [SerializeField] private bool startOn = true;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip toggleSound;

    private float currentBattery;
    private bool isOn;

    private void Awake()
    {
        if (flashlight == null)
            flashlight = GetComponent<Light>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        currentBattery = maxBattery;
        isOn = startOn;

        UpdateFlashlight();
        UpdateUI();
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

    private void Update()
    {
        if (!isOn)
            return;

        currentBattery -= drainPerSecond * Time.deltaTime;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

        if (currentBattery <= 0f)
            isOn = false;

        UpdateFlashlight();
        UpdateUI();
    }

    private void ToggleFlashlight(InputAction.CallbackContext context)
    {
        if (currentBattery <= 0f)
            return;

        isOn = !isOn;

        if (audioSource != null && toggleSound != null)
            audioSource.PlayOneShot(toggleSound);

        UpdateFlashlight();
    }

    public void AddBattery(float amount)
    {
        float oldBattery = currentBattery;

        currentBattery += amount;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

        Debug.Log("Battery added. Old: " + oldBattery + " New: " + currentBattery);

        if (currentBattery > 0f)
            isOn = true;

        UpdateFlashlight();
        UpdateUI();
    }

    private void UpdateFlashlight()
    {
        if (flashlight != null)
            flashlight.enabled = isOn && currentBattery > 0f;
    }

    private void UpdateUI()
    {
        if (batterySlider != null)
            batterySlider.value = currentBattery / maxBattery;
    }
}