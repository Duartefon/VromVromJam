using UnityEngine;

[RequireComponent(typeof(CarControl))]
public class CarFuel : MonoBehaviour
{
    [Header("Fuel Capacity")]
    public float maxFuel = 100f;
    public float currentFuel;

    [Header("Consumption Settings")]
    [Tooltip("Base fuel consumed per unit of distance traveled.")]
    public float fuelCostPerDistance = 0.05f;
    
    [Tooltip("Maximum extra multiplier added when driving at top speed. (e.g., 0.5 means 50% more fuel used at max speed)")]
    public float highSpeedPenalty = 0.5f;

    private CarControl carControl;
    private Vector3 lastPosition;

    void Start()
    {
        currentFuel = maxFuel;
        carControl = GetComponent<CarControl>();
        lastPosition = transform.position;
    }

    void Update()
    {
        if (currentFuel <= 0) return;

        // 1. Calculate how far we moved this frame
        float distanceTraveled = Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        // Only consume fuel if we are actually moving
        if (distanceTraveled > 0.01f)
        {
            // 2. Get the current speed and see how close we are to max speed (0 to 1)
            float currentSpeed = Mathf.Abs(carControl.CurrentForwardSpeed);
            float speedFactor = Mathf.Clamp01(currentSpeed / carControl.maxSpeed);

            // 3. Calculate the cost: base distance cost + extra penalty for going fast
            float speedMultiplier = 1f + (speedFactor * highSpeedPenalty);
            float costThisFrame = distanceTraveled * fuelCostPerDistance * speedMultiplier;

            // 4. Drain the fuel
            currentFuel -= costThisFrame;

            // 5. Check if we ran out
            if (currentFuel <= 0)
            {
                currentFuel = 0;
                carControl.SetHasFuel(false); // Tell the engine to cut power
                Debug.Log("Out of fuel! Coasting to a stop.");
            }
        }
    }

    /// <summary>
    /// Call this from a gas station trigger or pickup item.
    /// </summary>
    public void AddFuel(float amount)
    {
        currentFuel = Mathf.Clamp(currentFuel + amount, 0, maxFuel);
        if (currentFuel > 0)
        {
            carControl.SetHasFuel(true);
        }
    }
}