using System.Collections;
using UnityEngine;

public class FlickerLight : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] private Light targetLight;

    [Header("Startup")]
    [SerializeField] private bool flickerOnStart;
    [SerializeField] private bool startOff;

    [Header("Flicker")]
    [SerializeField] private float flickerDuration = 2f;
    [SerializeField] private float minDelay = 0.03f;
    [SerializeField] private float maxDelay = 0.15f;
    [SerializeField] private bool stayOnAfterFlicker = true;

    [Header("Looping")]
    [SerializeField] private bool loopFlicker;
    [SerializeField] private float loopDelay = 4f;

    [Header("Reveal Objects")]
    [SerializeField] private GameObject[] objectsToReveal;
    [SerializeField] private float revealExtraTime = 1f;

    [Header("Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip flickerSound;

    private Coroutine flickerRoutine;

    private void Awake()
    {
        if (targetLight == null)
            targetLight = GetComponent<Light>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (targetLight != null && startOff)
            targetLight.enabled = false;

        SetRevealObjects(false);
    }

    private void Start()
    {
        if (flickerOnStart)
            StartFlicker();
    }

    public void StartFlicker()
    {
        if (flickerRoutine != null)
            StopCoroutine(flickerRoutine);

        flickerRoutine = StartCoroutine(FlickerRoutine());
    }

    public void TurnOn()
    {
        if (targetLight != null)
            targetLight.enabled = true;
    }

    public void TurnOff()
    {
        if (targetLight != null)
            targetLight.enabled = false;
    }

    private IEnumerator FlickerRoutine()
    {
        do
        {
            if (audioSource != null && flickerSound != null)
                audioSource.PlayOneShot(flickerSound);

            SetRevealObjects(true);

            float timer = 0f;

            while (timer < flickerDuration)
            {
                if (targetLight != null)
                    targetLight.enabled = !targetLight.enabled;

                float wait = Random.Range(minDelay, maxDelay);
                timer += wait;

                yield return new WaitForSeconds(wait);
            }

            if (targetLight != null)
                targetLight.enabled = stayOnAfterFlicker;

            yield return new WaitForSeconds(revealExtraTime);

            SetRevealObjects(false);

            if (loopFlicker)
                yield return new WaitForSeconds(loopDelay);

        } while (loopFlicker);

        flickerRoutine = null;
    }

    private void SetRevealObjects(bool state)
    {
        for (int i = 0; i < objectsToReveal.Length; i++)
        {
            if (objectsToReveal[i] != null)
                objectsToReveal[i].SetActive(state);
        }
    }
}