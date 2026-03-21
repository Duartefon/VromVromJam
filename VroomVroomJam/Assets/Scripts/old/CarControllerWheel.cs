using Logitech;
using UnityEngine;

public class CarControlllerWheel : MonoBehaviour
{
    [Header("Motor")]
    public float motorTorque = 2000f;
    public float brakeTorque = 3000f;
    public float handbrakeTorque = 5000f;
    public float maxSpeed = 20f;
    public float engineBraking = 0.3f; // drag when coasting

    [Header("Air Control")]
    public float airAngularDrag = 0.5f;
    public float jumpStabilization = 2f;

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

    [Header("Fuel")]
    private bool hasFuel = true;

    [Header("Audio")]
    public AudioSource engineAudio;
    public AudioSource impactAudio;
    public AudioClip[] impactClips;
    public float minPitch = 0.5f;
    public float maxPitch = 2.5f;
    public float minVolume = 0.4f;
    public float maxVolume = 1f;
    public float impactThreshold = 10f;

    [Header("Logitech G29")]
    public int wheelIndex = 0;
    public int handbrakeButton = 22;

    private WheelControl[] wheels;
    private Rigidbody rigidBody;
    private float currentSteerAngle;
    private float lastVInput;

    public float CurrentForwardSpeed => rigidBody != null ? Vector3.Dot(transform.forward, rigidBody.linearVelocity) : 0f;

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
        float throttleRaw = 1f - (state.lY + 32768f) / 65535f;
        float brakeRaw    = 1f - (state.rglSlider[0] + 32768f) / 65535f;
        float vInput = throttleRaw - brakeRaw;

        bool handbrake = state.rgbButtons[handbrakeButton] == 128;
        Debug.Log($"Acceleration: {throttleRaw} hInput: {brakeRaw} Pedals: vInput: {vInput} stateAccPedal: {state.rglSlider[1]}");
        ApplyInputs(hInput, vInput, handbrake);
    }

    void Update()
    {
        UpdateEngineAudio(); //TODO: Audio should be a separate script
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
        lastVInput = vInput;

        float forwardSpeed = Vector3.Dot(transform.forward, rigidBody.linearVelocity);
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(forwardSpeed));

        float currentMotorTorque = Mathf.Lerp(motorTorque, 0, speedFactor);
        float targetSteerAngle = hInput * Mathf.Lerp(steeringRange, steeringRangeAtMaxSpeed, speedFactor);

        currentSteerAngle = Mathf.Lerp(currentSteerAngle, targetSteerAngle,
                                        steerSmoothing * Time.fixedDeltaTime);

        rigidBody.AddForce(-transform.up * downforce * rigidBody.linearVelocity.sqrMagnitude);

        // check how many wheels are grounded
        int groundedWheels = 0;
        foreach (var wheel in wheels)
            if (wheel.WheelCollider.isGrounded) groundedWheels++;

        bool isAirborne = groundedWheels == 0;

        if (isAirborne)
        {
            // stabilize the car in the air so it lands flat
            Quaternion targetRot = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                                                   jumpStabilization * Time.fixedDeltaTime);
            rigidBody.angularDamping = airAngularDrag;
        }
        else
        {
            rigidBody.angularDamping = angularDamping;
        }

        bool isBraking = Mathf.Abs(forwardSpeed) > 0.5f &&
                 (vInput == 0f || Mathf.Sign(vInput) != Mathf.Sign(forwardSpeed));

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
                    : 0f;
                SetSidewaysStiffness(wheel, brakeSidewaysStiffness);
            }
            else
            {
                if (wheel.motorized && hasFuel)
                    wheel.WheelCollider.motorTorque = vInput * currentMotorTorque;

                // engine braking when coasting
                if (vInput == 0f && wheel.motorized)
                {
                    wheel.WheelCollider.brakeTorque = engineBraking * brakeTorque;
                }
                else
                {
                    wheel.WheelCollider.brakeTorque = 0f;
                }

                SetSidewaysStiffness(wheel, sidewaysStiffness);
            }
        }
    }

    void UpdateEngineAudio()
    {
        float speedFactor = Mathf.InverseLerp(0, maxSpeed, Mathf.Abs(
            Vector3.Dot(transform.forward, rigidBody.linearVelocity)));

        // blend pitch between idle and max based on speed and throttle
        float targetPitch = Mathf.Lerp(minPitch, maxPitch,
            Mathf.Max(speedFactor, Mathf.Abs(lastVInput)));

        engineAudio.pitch = Mathf.Lerp(engineAudio.pitch, targetPitch, Time.deltaTime * 5f);
        engineAudio.volume = Mathf.Lerp(minVolume, maxVolume, speedFactor);

        if (!engineAudio.isPlaying)
            engineAudio.Play();
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

    void OnCollisionEnter(Collision collision)
    {
        float impactForce = collision.relativeVelocity.magnitude;

        if (impactForce < impactThreshold) return;
        if (impactClips.Length > 0)
            impactAudio.clip = impactClips[Random.Range(0, impactClips.Length)];

        // pitch and volume scale with impact force
        impactAudio.pitch = Random.Range(0.9f, 1.1f);
        impactAudio.volume = Mathf.Clamp01(impactForce / 20f);
        impactAudio.Play();
    }

    public void SetHasFuel(bool state)
    {
        hasFuel = state;
    }
}