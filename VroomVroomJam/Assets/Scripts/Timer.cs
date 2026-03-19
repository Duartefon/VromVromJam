using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Image timerFill;
    public float duration;
    public Color startColor = Color.white;
    public Color middleColor = new Color(1f, 0.5f, 0f);
    public Color endColor = Color.red;
    public AudioClip warningClip;

    private float elapsed;
    private bool running;
    private bool warningPlayed;
    private AudioSource audioSource;

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
            OnTimerComplete();
        }
    }

    public void StartTimer()
    {
        elapsed = 0f;
        running = true;
        warningPlayed = false;
        timerFill.fillAmount = 1f;
        timerFill.color = startColor;
    }

    private void OnTimerComplete()
    {
        Debug.Log("BOOM!");
    }
}