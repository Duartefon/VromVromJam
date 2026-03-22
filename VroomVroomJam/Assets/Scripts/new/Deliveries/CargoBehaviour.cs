using UnityEngine;

public class CargoBehaviour : MonoBehaviour
{
    [Header("Break Settings")]
    public bool isBroken = false;
    public float breakDelay = 0.2f;
    public Cargo cargoData;
    private float spawnTime;
    private bool hasBroken = false;

    void Start()
    {
        spawnTime = Time.time;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasBroken) return;

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