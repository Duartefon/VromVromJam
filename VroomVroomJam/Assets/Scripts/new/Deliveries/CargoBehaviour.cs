using UnityEngine;

public class CargoBehaviour : MonoBehaviour
{
    [Header("Break Settings")]
    public bool isBroken = false;
    public float breakDelay = 0.2f;
    public Cargo cargoData;
    private float spawnTime;
    private bool hasBroken = false;

    [Header("Audio Settings")]
    public AudioClip[] boxSounds;
    private AudioSource audioSource;

    void Start()
    {
        spawnTime = Time.time;
        audioSource = GetComponentInChildren<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasBroken) return;

        audioSource.volume = Random.Range(0.3f, 0.7f);
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(boxSounds[Random.Range(0, boxSounds.Length)]);

        if (Time.time - spawnTime < breakDelay) return;

        if (collision.gameObject.CompareTag("Ground"))
        {
            BreakCargo();
        }
    }

    private void BreakCargo()
    {
        hasBroken = true;
        isBroken = true;

        Debug.Log("Cargo broken: " + gameObject.name);

        // optional feedback
        // GetComponent<Renderer>().material.color = Color.red;
    }
}