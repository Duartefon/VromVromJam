using UnityEngine;

public class CarControl : MonoBehaviour
{
    [Header("Motor")]
    public float motorTorque = 2000f;
    public float brakeTorque = 3000f;
    public float handbrakeTorque = 5000f;
    public float maxSpeed = 20f;

    [Header("Steering")]
    public float steeringRange = 30f;
    public float steeringRangeAtMaxSpeed = 10f;
    public float steerSmoothing = 5f;

    [Header("Stability")]
    public float centreOfGravityOffset = -1f;
    public float linearDamping = 1f;
    public float angularDamping = 2f;
    public float downforce = 50f;

    [Header("Friction")]
    public float forwardStiffness = 2f;
    public float sidewaysStiffness = 2f;
    public float brakeSidewaysStiffness = 2.5f;

    private WheelControl[] wheels;
    private Rigidbody rigidBody;
    private CarInputActions carControls;
    private float currentSteerAngle;

    void Awake() { carControls = new CarInputActions(); }
    void OnEnable() { carControls.Enable(); }
    void OnDisable() { carControls.Disable(); }

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.linearDamping = linearDamping;
        rigidBody.angularDamping = angularDamping;

        Vector3 com = rigidBody.centerOfMass;
        com.y += centreOfGravityOffset;
        rigidBody.centerOfMass = com;

        wheels = GetComponentsInChildren<WheelControl>();
        SetupWheelFriction();
    }

    void FixedUpdate()
    {
        Vector2 input = carControls.Car.Movement.ReadValue<Vector2>();
        bool handbrake = carControls.Car.Handbrake.IsPressed(); // trvao de mao para drifts

        float vInput = input.y;
        float hInput = input.x;

        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));

        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
        float targetSteerAngle = hInput * Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle,
                                        steerSmoothing * Time.fixedDeltaTime);

        // downforce
        rigidBody.AddForce(-transform.up * downforce * rigidBody.linearVelocity.sqrMagnitude);

        bool isBraking = vInput == 0f || Mathf.Sign(vInput) != Mathf.Sign(forwardSpeed);

        foreach (var wheel in wheels)
        {
            if (wheel.steerable)
                wheel.WheelCollider.steerAngle = currentSteerAngle;

            // ainda ta meio bugado
            if (handbrake && !wheel.steerable)
            {
                wheel.WheelCollider.motorTorque = 0f;
                wheel.WheelCollider.brakeTorque = handbrakeTorque;
                SetSidewaysStiffness(wheel, brakeSidewaysStiffness);
                continue;
            }

            if (isBraking)
            {
                wheel.WheelCollider.motorTorque = 0f;
                wheel.WheelCollider.brakeTorque = Mathf.Abs(vInput) > 0.1f
                    ? Mathf.Abs(vInput) * brakeTorque
                    : brakeTorque * 0.1f;
                SetSidewaysStiffness(wheel, brakeSidewaysStiffness);
            }
            else
            {
                if (wheel.motorized)
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;
                wheel.WheelCollider.brakeTorque = 0f;
                SetSidewaysStiffness(wheel, sidewaysStiffness);
            }
        }
    }

    void SetupWheelFriction()
    {
        foreach (var wheel in wheels)
        {
            WheelFrictionCurve fwd = wheel.WheelCollider.forwardFriction;
            fwd.stiffness = forwardStiffness;
            wheel.WheelCollider.forwardFriction = fwd;

            SetSidewaysStiffness(wheel, sidewaysStiffness);
        }
    }

    void SetSidewaysStiffness(WheelControl wheel, float stiffness)
    {
        WheelFrictionCurve sideways = wheel.WheelCollider.sidewaysFriction;
        sideways.stiffness = stiffness;
        wheel.WheelCollider.sidewaysFriction = sideways;
    }

    void OnDrawGizmos()
    {
        if (wheels == null) return;
        foreach (var wheel in wheels)
            wheel.DrawGizmo();
    }
}