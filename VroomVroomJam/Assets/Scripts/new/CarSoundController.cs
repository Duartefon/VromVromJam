using UnityEngine;

public class CarSoundController : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource engineAudio;
    public AudioSource impactAudio;
    public AudioSource dirtAudio;
    public AudioSource skidAudio;
    public AudioClip[] impactClips;
    public AudioClip[] skidClips;

    [Header("Engine")]
    public float minPitch = 0.5f;
    public float maxPitch = 2.5f;
    public float minVolume = 0.4f;
    public float maxVolume = 1f;
    public float maxSpeed = 50f;

    [Header("Impact")]
    public float impactThreshold = 10f;

    private Rigidbody rb;
    private Wheel[] wheels;

    void Start()
    {
        rb = GetComponentInParent<Rigidbody>();
        engineAudio.loop = true;
        engineAudio.Play();
        dirtAudio.loop = true;
        dirtAudio.Play();

        wheels = GetComponent<NewCarController>().wheels;
    }

    void Update()
    {
        UpdateEngineAudio();
        UpdateSkidAudio();
    }

    void UpdateEngineAudio()
    {
        float speed = Mathf.Abs(Vector3.Dot(transform.forward, rb.linearVelocity));
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, speed);

        float inputFactor = Mathf.Abs(CarInput.GetMovementInput().y);

        float targetPitch = Mathf.Lerp(minPitch, maxPitch,
            Mathf.Max(speedFactor, inputFactor));

        engineAudio.pitch = Mathf.Lerp(engineAudio.pitch, targetPitch, Time.deltaTime * 5f);
        engineAudio.volume = Mathf.Lerp(minVolume, maxVolume, speedFactor);

        if (speedFactor > 0.05f)
        {
            if (!dirtAudio.isPlaying)
            {
                dirtAudio.loop = true;
                dirtAudio.Play();
            }

            dirtAudio.volume = Mathf.Lerp(dirtAudio.volume, speedFactor, Time.deltaTime * 5f);

            if(dirtAudio.volume > 0.4f)
            {
                dirtAudio.volume = 0.4f;
            }
        }
        else
        {
            if (dirtAudio.isPlaying)
            {
                dirtAudio.Stop();
            }
        }
    }

    void UpdateSkidAudio()
    {
        bool isAnyWheelSkidding = false;

        foreach (var wheel in wheels)
        {
            if (wheel.isSkidding)
            {
                isAnyWheelSkidding = true;
                break;
            }
        }

        if (isAnyWheelSkidding)
        {
            if (!skidAudio.isPlaying)
            {
                skidAudio.clip = skidClips[Random.Range(0, skidClips.Length)];
                skidAudio.loop = true;
                skidAudio.Play();
            }
        }
        else
        {
            if (skidAudio.isPlaying)
            {
                skidAudio.Stop();
            }
        }
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