using UnityEngine;

public class LivingEyesTeleporter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] escapePoints;
    [SerializeField] private Light[] eyeLights;

    [Header("Activation")]
    [SerializeField] private float escapeRange = 7f;
    [SerializeField] private float escapeCooldown = 1.5f;

    [Header("Shake")]
    [SerializeField] private float shakeAmount = 0.04f;
    [SerializeField] private float shakeSpeed = 30f;

    [Header("Light Pulse")]
    [SerializeField] private float minIntensity = 1.5f;
    [SerializeField] private float maxIntensity = 4f;
    [SerializeField] private float pulseSpeed = 4f;

    private Vector3 basePosition;
    private float cooldownTimer;
    private int lastEscapeIndex = -1;

    private void Start()
    {
        basePosition = transform.position;
    }

    private void Update()
    {
        ShakeAlways();
        PulseLights();

        cooldownTimer -= Time.deltaTime;

        if (player == null)
            return;

        float distance = Vector3.Distance(player.position, basePosition);

        if (distance <= escapeRange && cooldownTimer <= 0f)
            EscapeToNewPoint();
    }

    private void ShakeAlways()
    {
        Vector3 shake = new Vector3(
            Mathf.Sin(Time.time * shakeSpeed) * shakeAmount,
            Mathf.Sin(Time.time * shakeSpeed * 1.3f) * shakeAmount,
            0f
        );

        transform.position = basePosition + shake;
    }

    private void PulseLights()
    {
        float pulse = Mathf.Lerp(
            minIntensity,
            maxIntensity,
            Mathf.PingPong(Time.time * pulseSpeed, 1f)
        );

        for (int i = 0; i < eyeLights.Length; i++)
        {
            if (eyeLights[i] != null)
                eyeLights[i].intensity = pulse;
        }
    }

    private void EscapeToNewPoint()
    {
        if (escapePoints == null || escapePoints.Length == 0)
            return;

        int newIndex = Random.Range(0, escapePoints.Length);

        if (escapePoints.Length > 1)
        {
            while (newIndex == lastEscapeIndex)
                newIndex = Random.Range(0, escapePoints.Length);
        }

        lastEscapeIndex = newIndex;

        basePosition = escapePoints[newIndex].position;
        transform.position = basePosition;

        cooldownTimer = escapeCooldown;
    }
}