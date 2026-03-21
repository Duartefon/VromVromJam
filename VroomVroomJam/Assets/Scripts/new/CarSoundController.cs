using UnityEngine;

public class CarSoundController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource engineAudio;
    public AudioSource impactAudio;
    public AudioClip[] impactClips;

    [Header("Engine")]
    public float minPitch = 0.5f;
    public float maxPitch = 2.5f;
    public float minVolume = 0.4f;
    public float maxVolume = 1f;
    public float maxSpeed = 50f;

    [Header("Impact")]
    public float impactThreshold = 10f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        engineAudio.loop = true;
        engineAudio.Play();
    }

    void Update()
    {
        UpdateEngineAudio();
    }

    void UpdateEngineAudio()
    {
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(
            Vector3.Dot(transform.forward, rb.linearVelocity)));

        float targetPitch = Mathf.Lerp(minPitch, maxPitch,
            Mathf.Max(speedFactor, Mathf.Abs(CarInput.GetMovementInput().y)));

        engineAudio.pitch = Mathf.Lerp(engineAudio.pitch, targetPitch, Time.deltaTime * 5f);
        engineAudio.volume = Mathf.Lerp(minVolume, maxVolume, speedFactor);
    }

    void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce < impactThreshold || impactClips.Length == 0) return;

        AudioClip clip = impactClips[Random.Range(0, impactClips.Length)];
        float volume = Mathf.InverseLerp(impactThreshold, impactThreshold * 3f, impactForce);

        impactAudio.PlayOneShot(clip, volume);
    }
}