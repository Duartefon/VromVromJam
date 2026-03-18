using Logitech;
using UnityEngine;

public class CarControlllerWheel : MonoBehaviour
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

    [Header("Logitech G29")]
    public int wheelIndex = 0;
    public int handbrakeButton = 22;

    private WheelControl[] wheels;
    private Rigidbody rigidBody;
    private float currentSteerAngle;

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
        if (!LogitechGSDK.LogiUpdate() || !LogitechGSDK.LogiIsConnected(wheelIndex))
        {
            ReadKeyboard();
            return;
        }

        LogitechGSDK.DIJOYSTATE2ENGINES state = LogitechGSDK.LogiGetStateUnity(wheelIndex);

        // lX range: -32768 to 32767 → normalize to -1..1
        float hInput = state.lX / 32767f;
        hInput = Mathf.Abs(hInput) > 0.05f ? hInput : 0f; // dead zone

        // G29 pedals: 32767 = released, -32768 = fully pressed — so invert
        float throttleRaw = 1f - (state.lY+32768f) / 65535f;
        float brakeRaw    = 1f - (state.rglSlider[0] + 32768f) / 65535f;
        float vInput = throttleRaw - brakeRaw;

        bool handbrake = state.rgbButtons[handbrakeButton] == 128;
        Debug.Log($"Acceleration: {throttleRaw} hInput: {brakeRaw} Pedals: vInput: {vInput} stateAccPedal: {state.rglSlider[1]}");
        ApplyInputs(hInput, vInput, handbrake);
    }

    void ReadKeyboard()
    {
        float hInput = Input.GetAxis("Horizontal");
        float vInput = Input.GetAxis("Vertical");
        bool handbrake = Input.GetKey(KeyCode.Space);
        ApplyInputs(hInput, vInput, handbrake);
    }

    void ApplyInputs(float hInput, float vInput, bool handbrake)
    {
        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));

        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
        float targetSteerAngle = hInput * Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle,
                                        steerSmoothing * Time.fixedDeltaTime);

        rigidBody.AddForce(-transform.up * downforce * rigidBody.linearVelocity.sqrMagnitude);

        bool isBraking = vInput == 0f || Mathf.Sign(vInput) != Mathf.Sign(forwardSpeed);

        foreach (var wheel in wheels)
        {
            if (wheel.steerable)
                wheel.WheelCollider.steerAngle = currentSteerAngle;

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

            float sideways = wheel.steerable ? sidewaysStiffness * 1.2f : sidewaysStiffness;
            SetSidewaysStiffness(wheel, sideways);
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