using System.Collections;
using TMPro;
using UnityEngine;

public class NarratorSystem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject subtitleCanvas;
    [SerializeField] private TMP_Text subtitleText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Settings")]
    [SerializeField] private float defaultDisplayTime = 4f;

    private Coroutine lineRoutine;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        ClearLine();
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
        if (subtitleText != null)
            subtitleText.text = "";

        if (subtitleCanvas != null)
            subtitleCanvas.SetActive(false);
    }

    private IEnumerator LineRoutine(string line, AudioClip voiceClip, float displayTime)
    {
        if (subtitleCanvas != null)
            subtitleCanvas.SetActive(true);

        if (subtitleText != null)
            subtitleText.text = line;

        if (audioSource != null && voiceClip != null)
            audioSource.PlayOneShot(voiceClip);

        yield return new WaitForSeconds(displayTime);

        ClearLine();
        lineRoutine = null;
    }
}