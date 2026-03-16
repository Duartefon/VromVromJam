using UnityEngine;

public class CarControl : MonoBehaviour
{
    [Header("Car Properties")]
    public float motorTorque = 2000f;
    public float brakeTorque = 2000f;
    public float maxSpeed = 20f;
    public float steeringRange = 30f;
    public float steeringRangeAtMaxSpeed = 10f;
    public float centreOfGravityOffset = -1f;
    public float linearDamping = 1f; // atrito
    public float angularDamping = 2f;

    private WheelControl[] wheels;
    private Rigidbody rigidBody;

    private CarInputActions carControls; // Reference to the new input system

    void Awake()
    {
        carControls = new CarInputActions(); // Initialize Input Actions
    }
    void OnEnable()
    {
        carControls.Enable();
    }

    void OnDisable()
    {
        carControls.Disable();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();

        //Gabriel - adicionei drag para o carro nao escorregar para sempre
        rigidBody.linearDamping = linearDamping;
        rigidBody.angularDamping = angularDamping;

        // Adjust center of mass to improve stability and prevent rolling
        Vector3 centerOfMass = rigidBody.centerOfMass;
        centerOfMass.y += centreOfGravityOffset;
        rigidBody.centerOfMass = centerOfMass;

        // Get all wheel components attached to the car
        wheels = GetComponentsInChildren<WheelControl>();
        
        SetupWheelFriction(); // adiciona fricção as rodas
    }
            
    // FixedUpdate is called at a fixed time interval
    void FixedUpdate()
    {
        // Read the Vector2 input from the new Input System
        Vector2 inputVector = carControls.Car.Movement.ReadValue<Vector2>();

        // Get player input for acceleration and steering
        float vInput = inputVector.y; // Forward/backward input
        float hInput = inputVector.x; // Steering input
        
        // Calculate current speed along the car's forward axis
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed)); // Normalized speed factor

        // Reduce motor torque and steering at high speeds for better handling
        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
        float currentSteerRange = Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        // Determine if the player is accelerating or trying to reverse
        bool isAccelerating = Mathf.Sign(vInput) == Mathf.Sign(forwardSpeed);

        foreach (var wheel in wheels)
        {
            // Apply steering to wheels that support steering
            if (wheel.steerable)
            {
                wheel.WheelCollider.steerAngle = hInput * currentSteerRange;
            }

            if (isAccelerating)
            {
                // Apply torque to motorized wheels
                if (wheel.motorized)
                {
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                }
                // Release brakes when accelerating
                wheel.WheelCollider.brakeTorque = 0f;
            }
            else
            {
                // Apply brakes when reversing direction
                wheel.WheelCollider.motorTorque = 0f;
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) * brakeTorque;
            }
        }

        
    }

    void SetupWheelFriction()
    {
        foreach (var wheel in wheels)
        {
            WheelFrictionCurve forwardFriction = wheel.WheelCollider.forwardFriction;
            forwardFriction.stiffness = 2f;
            wheel.WheelCollider.forwardFriction = forwardFriction;

            WheelFrictionCurve sidewaysFriction = wheel.WheelCollider.sidewaysFriction;
            sidewaysFriction.stiffness = 2f;
            wheel.WheelCollider.sidewaysFriction = sidewaysFriction;
        }
    }
}