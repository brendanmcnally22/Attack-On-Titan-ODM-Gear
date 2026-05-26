using System.Collections;
using TMPro;
using UnityEngine;

public class NarratorSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject subtitleCanvas;
    [SerializeField] private CanvasGroup subtitleCanvasGroup;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Settings")]
    [SerializeField] private float defaultDisplayTime = 4f;
    [SerializeField] private float fadeInTime = 0.35f;
    [SerializeField] private float fadeOutTime = 0.5f;

    private Coroutine lineRoutine;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (subtitleCanvasGroup == null && subtitleCanvas != null)
            subtitleCanvasGroup = subtitleCanvas.GetComponent<CanvasGroup>();

        if (subtitleCanvasGroup == null && subtitleCanvas != null)
            subtitleCanvasGroup = subtitleCanvas.AddComponent<CanvasGroup>();

        ClearLineInstant();
    }

    public void PlayLine(string line)
    {
        PlayLine(line, null, defaultDisplayTime);
    }

    public void PlayLine(string line, AudioClip voiceClip)
    {
        float displayTime = voiceClip != null ? voiceClip.length + 0.5f : defaultDisplayTime;
        PlayLine(line, voiceClip, displayTime);
    }

    public void PlayLine(string line, AudioClip voiceClip, float displayTime)
    {
        if (lineRoutine != null)
            StopCoroutine(lineRoutine);

        lineRoutine = StartCoroutine(LineRoutine(line, voiceClip, displayTime));
    }

    public void ClearLine()
    {
        if (lineRoutine != null)
            StopCoroutine(lineRoutine);

        lineRoutine = StartCoroutine(ClearRoutine());
    }

    private IEnumerator LineRoutine(string line, AudioClip voiceClip, float displayTime)
    {
        if (subtitleCanvas != null)
            subtitleCanvas.SetActive(true);

        if (subtitleText != null)
            subtitleText.text = line;

        if (subtitleCanvasGroup != null)
            subtitleCanvasGroup.alpha = 0f;

        if (audioSource != null && voiceClip != null)
            audioSource.PlayOneShot(voiceClip);

        yield return FadeCanvas(0f, 1f, fadeInTime);

        yield return new WaitForSeconds(displayTime);

        yield return FadeCanvas(1f, 0f, fadeOutTime);

        ClearLineInstant();

        lineRoutine = null;
    }

    private IEnumerator ClearRoutine()
    {
        yield return FadeCanvas(1f, 0f, fadeOutTime);
        ClearLineInstant();
        lineRoutine = null;
    }

    private IEnumerator FadeCanvas(float from, float to, float duration)
    {
        if (subtitleCanvasGroup == null)
            yield break;

        if (duration <= 0f)
        {
            subtitleCanvasGroup.alpha = to;
            yield break;
        }

        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);

            subtitleCanvasGroup.alpha = Mathf.Lerp(from, to, t);

            yield return null;
        }

        subtitleCanvasGroup.alpha = to;
    }

    private void ClearLineInstant()
    {
        if (subtitleText != null)
            subtitleText.text = "";

        if (subtitleCanvasGroup != null)
            subtitleCanvasGroup.alpha = 0f;

        if (subtitleCanvas != null)
            subtitleCanvas.SetActive(false);
    }
}