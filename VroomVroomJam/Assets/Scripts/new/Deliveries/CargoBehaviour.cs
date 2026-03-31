using UnityEngine;
using UnityEngine.VFX;

public class CargoBehaviour : MonoBehaviour
{
    [Header("Break Settings")]
    public bool isBroken = false;
    public float breakDelay = 0.2f;
    public Cargo cargoData;
    private float spawnTime;
    public GameObject explosionPrefab;

    [Header("Audio Settings")]
    public AudioClip[] boxSounds;
    public AudioClip explosionSound;
    private AudioSource audioSource;

    void Start()
    {
        spawnTime = Time.time;
        audioSource = GetComponentInChildren<AudioSource>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isBroken) return;

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
        isBroken = true;
        Explode();
        Debug.Log("Cargo broken: " + gameObject.name);
    }

    private void Explode()
    {
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        audioSource.PlayOneShot(explosionSound);
        Destroy(gameObject, 0.5f);}
}