using UnityEngine;

public class RoadEventTrigger : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private bool triggerOnce = true;

    [Header("Objects")]
    [SerializeField] private GameObject[] objectsToEnable;
    [SerializeField] private GameObject[] objectsToDisable;

    [Header("Lights")]
    [SerializeField] private FlickerLight[] lightsToFlicker;
    [SerializeField] private FlickerLight[] lightsToTurnOn;
    [SerializeField] private FlickerLight[] lightsToTurnOff;

    [Header("Narrator")]
    [SerializeField] private NarratorSystem narratorSystem;
    [TextArea(2, 5)]
    [SerializeField] private string narratorLine;
    [SerializeField] private AudioClip narratorVoiceClip;
    [SerializeField] private float narratorDisplayTime = 4f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip eventSound;

    [Header("Health")]
    [SerializeField] private int damagePlayerAmount;
    [SerializeField] private int healPlayerAmount;

    [Header("Road Sector")]
    [SerializeField] private bool changeRoadSector;
    [SerializeField] private RoadStateManager roadStateManager;
    [SerializeField] private RoadSector sectorToSet = RoadSector.None;

    private bool hasTriggered;

    private void Awake()
    {
        if (narratorSystem == null)
            narratorSystem = FindFirstObjectByType<NarratorSystem>();

        if (roadStateManager == null)
            roadStateManager = FindFirstObjectByType<RoadStateManager>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
            return;

        if (other.GetComponent<CharacterController>() == null)
            return;

        hasTriggered = true;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        RunEvent(playerHealth);
    }

    public void RunEvent(PlayerHealth playerHealth = null)
    {
        HandleObjects();
        HandleLights();
        HandleNarrator();
        HandleAudio();
        HandleHealth(playerHealth);
        HandleRoadSector();
    }

    private void HandleObjects()
    {
        for (int i = 0; i < objectsToEnable.Length; i++)
        {
            if (objectsToEnable[i] != null)
                objectsToEnable[i].SetActive(true);
        }

        for (int i = 0; i < objectsToDisable.Length; i++)
        {
            if (objectsToDisable[i] != null)
                objectsToDisable[i].SetActive(false);
        }
    }

    private void HandleLights()
    {
        for (int i = 0; i < lightsToFlicker.Length; i++)
        {
            if (lightsToFlicker[i] != null)
                lightsToFlicker[i].StartFlicker();
        }

        for (int i = 0; i < lightsToTurnOn.Length; i++)
        {
            if (lightsToTurnOn[i] != null)
                lightsToTurnOn[i].TurnOn();
        }

        for (int i = 0; i < lightsToTurnOff.Length; i++)
        {
            if (lightsToTurnOff[i] != null)
                lightsToTurnOff[i].TurnOff();
        }
    }

    private void HandleNarrator()
    {
        if (narratorSystem == null)
            return;

        if (string.IsNullOrWhiteSpace(narratorLine) && narratorVoiceClip == null)
            return;

        narratorSystem.PlayLine(narratorLine, narratorVoiceClip, narratorDisplayTime);
    }

    private void HandleAudio()
    {
        if (audioSource != null && eventSound != null)
            audioSource.PlayOneShot(eventSound);
    }

    private void HandleHealth(PlayerHealth playerHealth)
    {
        if (playerHealth == null)
            return;

        if (damagePlayerAmount > 0)
            playerHealth.TakeDamage(damagePlayerAmount);

        if (healPlayerAmount > 0)
            playerHealth.Heal(healPlayerAmount);
    }

    private void HandleRoadSector()
    {
        if (!changeRoadSector)
            return;

        if (roadStateManager != null)
            roadStateManager.SetSector(sectorToSet);
    }
}