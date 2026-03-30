using UnityEngine;
using UnityEngine.UI;
using System;

public class Timer : MonoBehaviour
{
    public Image timerFill;
    public Color startColor = Color.white;
    public Color middleColor = new Color(1f, 0.5f, 0f);
    public Color endColor = Color.red;
    public AudioClip warningClip;

    public event Action OnTimerComplete;

    private float elapsed;
    public bool running;
    private bool warningPlayed;
    private AudioSource audioSource;
    private float duration;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartTimer();
    }

    void Update()
    {
        if (!running) return;

        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);
        timerFill.fillAmount = 1f - t;

        Color color;
        if (t < 0.5f)
            color = Color.Lerp(startColor, middleColor, t * 2f);
        else
            color = Color.Lerp(middleColor, endColor, (t - 0.5f) * 2f);
        timerFill.color = color;

        if (!warningPlayed && t >= 0.75f)
        {
            warningPlayed = true;
            if (warningClip != null && audioSource != null)
                audioSource.PlayOneShot(warningClip);
        }

        if (elapsed >= duration)
        {
            running = false;
            HandleTimerEnd();
        }
    }

    public void StartTimer(float duration = 60f)
    {
        this.duration = duration;
        elapsed = 0f;
        running = true;
        warningPlayed = false;
        timerFill.fillAmount = 1f;
        timerFill.color = startColor;
    }

    public void StopTimer()
    {
        running = false;
    }

    private void HandleTimerEnd()
    {
        Debug.Log("BOOM!");
        OnTimerComplete?.Invoke();
    }

    public bool IsTimeUp()
    {
        return elapsed >= duration;
    }
}