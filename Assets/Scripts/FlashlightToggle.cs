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
    [SerializeField] private InputActionReference lookAction;

    [Header("Battery")]
    [SerializeField] private float maxBattery = 100f;
    [SerializeField] private float drainPerSecond = 4f;
    [SerializeField] private bool startOn = true;

    [Header("Low Battery Flicker")]
    [SerializeField] private float lowBatteryPercent = 0.25f;
    [SerializeField] private float flickerSpeed = 18f;
    [SerializeField] private float minFlickerIntensity = 0.25f;

    [Header("Shake Recharge")]
    [SerializeField] private float shakeRequired = 6f;
    [SerializeField] private float shakeGainPerDirectionChange = 1f;
    [SerializeField] private float shakeDecaySpeed = 2f;
    [SerializeField] private float mouseShakeThreshold = 6f;
    [SerializeField] private float shakeRechargeAmount = 18f;
    [SerializeField] private float shakeCooldown = 1f;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip toggleSound;
    [SerializeField] private AudioClip shakeRechargeSound;

    private float currentBattery;
    private bool isOn;

    private float normalIntensity;
    private float shakeProgress;
    private float cooldownTimer;
    private int lastMouseXDirection;

    private void Awake()
    {
        if (flashlight == null)
            flashlight = GetComponent<Light>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (flashlight != null)
            normalIntensity = flashlight.intensity;

        currentBattery = maxBattery;
        isOn = startOn;

        UpdateFlashlight();
        UpdateUI();
    }

    private void OnEnable()
    {
        EnableAction(flashlightAction);
        EnableAction(lookAction);

        if (flashlightAction != null && flashlightAction.action != null)
            flashlightAction.action.performed += ToggleFlashlight;
    }

    private void OnDisable()
    {
        if (flashlightAction != null && flashlightAction.action != null)
            flashlightAction.action.performed -= ToggleFlashlight;

        DisableAction(flashlightAction);
        DisableAction(lookAction);
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (isOn)
            DrainBattery();

        HandleShakeRecharge();
        HandleLowBatteryFlicker();

        UpdateFlashlight();
        UpdateUI();
    }

    private void DrainBattery()
    {
        currentBattery -= drainPerSecond * Time.deltaTime;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

        if (currentBattery <= 0f)
            isOn = false;
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

    private void HandleShakeRecharge()
    {
        if (lookAction == null || lookAction.action == null)
            return;

        if (cooldownTimer > 0f)
            return;

        float batteryPercent = currentBattery / maxBattery;

        if (batteryPercent > lowBatteryPercent)
        {
            shakeProgress = 0f;
            return;
        }

        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        int currentDirection = 0;

        if (lookInput.x > mouseShakeThreshold)
            currentDirection = 1;
        else if (lookInput.x < -mouseShakeThreshold)
            currentDirection = -1;

        if (currentDirection != 0 && currentDirection != lastMouseXDirection)
        {
            shakeProgress += shakeGainPerDirectionChange;
            lastMouseXDirection = currentDirection;
        }

        shakeProgress -= shakeDecaySpeed * Time.deltaTime;
        shakeProgress = Mathf.Clamp(shakeProgress, 0f, shakeRequired);

        if (shakeProgress >= shakeRequired)
            RechargeFromShake();
    }

    private void RechargeFromShake()
    {
        currentBattery += shakeRechargeAmount;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

        isOn = true;
        shakeProgress = 0f;
        cooldownTimer = shakeCooldown;

        if (audioSource != null && shakeRechargeSound != null)
            audioSource.PlayOneShot(shakeRechargeSound);
    }

    private void HandleLowBatteryFlicker()
    {
        if (flashlight == null)
            return;

        float batteryPercent = currentBattery / maxBattery;

        if (!isOn || currentBattery <= 0f)
        {
            flashlight.intensity = normalIntensity;
            return;
        }

        if (batteryPercent > lowBatteryPercent)
        {
            flashlight.intensity = normalIntensity;
            return;
        }

        float flicker = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        flashlight.intensity = Mathf.Lerp(minFlickerIntensity, normalIntensity, flicker);
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

    public void AddBattery(float amount)
    {
        currentBattery += amount;
        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);

        if (currentBattery > 0f)
            isOn = true;

        UpdateFlashlight();
        UpdateUI();
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
}