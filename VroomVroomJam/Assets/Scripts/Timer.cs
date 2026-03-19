using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Image timerFill;
    public float duration;

    private float elapsed;
    private bool running;

    void Start()
    {
        StartTimer();
    }

    void Update()
    {
        if (!running) return;

        elapsed += Time.deltaTime;
        timerFill.fillAmount = 1f - Mathf.Clamp01(elapsed / duration);

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
        timerFill.fillAmount = 1f;
    }

    private void OnTimerComplete()
    {
        Debug.Log("BOOM!");
    }
}