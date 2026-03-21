using UnityEngine;

public class Wheel : MonoBehaviour
{
    private Rigidbody rb;

    public enum Position { FrontLeft, FrontRight, RearLeft, RearRight }

    [Header("Engine")]
    public float motorForce = 1500f;
    public float maxSpeed = 30f; // m/s

    [Header("Suspension")]
    public float restLength = 0.45f;
    public float springTravel = 0.15f;
    public float springStiffness = 30_000f; // para o carro ficar bouncy, usar um valor alto tipo 50k
    public float damperStiffness = 4_000f;  // em combinação com um dampening menor, tipo 2k

    [Header("Wheel")]
    public float wheelRadius = 0.25f;
    public float steeringAngle;
    public float steerTime = 10f;
    public float tireGrip = 750f;
    public Position position;
    public float brakeForceMultiplier = 1.5f; // quantas vezes mais forte é comparado ao motor

    [Header("Mesh")]
    public Transform wheelMesh;

    [Header("Skid")]
    public ParticleSystem skidParticles;
    public float skidThreshold = 3f;
    public float skidSpinThreshold = 8f;

    private float maxLength, minLength, springLength;
    private float previousSpringLength;
    private float springForce;
    private Vector3 suspensionForce;
    private Vector3 wheelVelocity; // local space
    private float springVelocity;
    private float damperForce;
    private float wheelAngle;
    private float wheelSpinAngle;
    private bool isGrounded;

    void Start()
    {
        rb = transform.parent.GetComponent<Rigidbody>();
        skidParticles = GetComponentInChildren<ParticleSystem>();

        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
        springLength = restLength;
    }

    void Update()
    {
        UpdateWheelRotation();

        //Debug.Log($"Is Grounded: {isGrounded}");
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius))
        {
            isGrounded = true;

            UpdateSuspension(hit);
            UpdateWheelVelocity(hit);

            Vector3 forwardDir, rightDir;
            GetSteeringDirections(out forwardDir, out rightDir);

            float accelInput = CarInput.GetMovementInput().y;
            float sidewaysVel = wheelVelocity.x;
            float forwardVel = wheelVelocity.z;

            Vector3 lateralForce = CalculateLateralForce(rightDir, sidewaysVel);
            Vector3 rollingResistance = CalculateRollingResistance(forwardDir, forwardVel, accelInput);
            Vector3 driveForce = CalculateDriveForce(forwardDir, accelInput, sidewaysVel);
            Vector3 turnAssist = CalculateTurnAssist(forwardDir, sidewaysVel);
            Vector3 alignForce = CalculateAlignForce(rightDir, sidewaysVel);

            ApplyAutoStop(accelInput);

            float currentSpeed = rb.linearVelocity.magnitude;
            if (currentSpeed > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }

            rb.AddForceAtPosition(
                suspensionForce
                + driveForce
                + lateralForce
                + turnAssist
                + alignForce
                + rollingResistance,
                hit.point
            );

            ApplyDownforce();
            ApplyBrake(forwardDir, forwardVel);
            UpdateWheelMesh();
            UpdateSkidParticles(hit, sidewaysVel, forwardVel, accelInput);

            if (!isGrounded)
            {
                // controlar o carro no ar
            }
        }
        else
        {
            isGrounded = false;
            StopSkid();
        }
    }

    void UpdateSkidParticles(RaycastHit hit, float sidewaysVel, float forwardVel, float accelInput)
    {
        if (skidParticles == null) return;

        bool lateralSkid = Mathf.Abs(sidewaysVel) > skidThreshold;
        bool spinSkid = Mathf.Abs(accelInput) > 0.8f && Mathf.Abs(forwardVel) < skidSpinThreshold;

        if (lateralSkid || spinSkid)
        {
            skidParticles.transform.position = hit.point;

            Vector3 wheelBack = -(Quaternion.AngleAxis(wheelAngle, transform.up) * transform.forward);
            skidParticles.transform.rotation = Quaternion.LookRotation(wheelBack, hit.normal);

            float slipIntensity = Mathf.Max(
                Mathf.InverseLerp(skidThreshold, skidThreshold * 3f, Mathf.Abs(sidewaysVel)),
                Mathf.InverseLerp(0f, skidSpinThreshold, Mathf.Abs(forwardVel))
            );

            var emission = skidParticles.emission;
            //emission.rateOverTime = Mathf.Lerp(10f, 60f, slipIntensity);

            if (!skidParticles.isPlaying)
                skidParticles.Play();
        }
        else
        {
            StopSkid();
        }
    }

    void StopSkid()
    {
        if (skidParticles == null) return;
        if (skidParticles.isPlaying)
            skidParticles.Stop();
    }

    void UpdateWheelMesh()
    {
        if (wheelMesh == null) return;

        wheelMesh.position = transform.position - transform.up * springLength;

        float forwardSpeed = wheelVelocity.z;
        float rotationDegrees = (forwardSpeed / (2f * Mathf.PI * wheelRadius)) * 360f * Time.fixedDeltaTime;
        wheelSpinAngle += rotationDegrees;

        if (position == Position.FrontLeft || position == Position.FrontRight)
            wheelMesh.localRotation = Quaternion.Euler(wheelSpinAngle, wheelAngle, 0f);
        else
            wheelMesh.localRotation = Quaternion.Euler(wheelSpinAngle, 0f, 0f);
    }

    void UpdateWheelRotation()
    {
        wheelAngle = Mathf.Lerp(wheelAngle, steeringAngle, Time.deltaTime * steerTime);
        transform.localRotation = Quaternion.Euler(Vector3.up * wheelAngle);
    }

    void UpdateSuspension(RaycastHit hit)
    {
        previousSpringLength = springLength;
        springLength = Mathf.Clamp(hit.distance - wheelRadius, minLength, maxLength);

        springVelocity = (previousSpringLength - springLength) / Time.fixedDeltaTime;
        damperForce = damperStiffness * springVelocity;

        springForce = springStiffness * (restLength - springLength);
        suspensionForce = transform.up * (springForce + damperForce);
    }

    void UpdateWheelVelocity(RaycastHit hit)
    {
        wheelVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(hit.point));
    }

    void GetSteeringDirections(out Vector3 forwardDir, out Vector3 rightDir)
    {
        Quaternion steerRot = Quaternion.AngleAxis(wheelAngle, transform.up);
        forwardDir = steerRot * transform.forward;
        rightDir = steerRot * transform.right;
    }

    Vector3 CalculateLateralForce(Vector3 rightDir, float sidewaysVel)
    {
        return -rightDir * sidewaysVel * tireGrip;
    }

    Vector3 CalculateRollingResistance(Vector3 forwardDir, float forwardVel, float accelInput)
    {
        float resistance = (Mathf.Abs(accelInput) < 0.1f) ? 2f : 0.5f;
        return -forwardDir * forwardVel * resistance;
    }

    Vector3 CalculateDriveForce(Vector3 forwardDir, float accelInput, float sidewaysVel)
    {
        float forwardStability = Mathf.Clamp01(1f - Mathf.Abs(sidewaysVel) * 0.02f);
        return forwardDir * accelInput * motorForce * forwardStability;
    }

    Vector3 CalculateTurnAssist(Vector3 forwardDir, float sidewaysVel)
    {
        float speed = rb.linearVelocity.magnitude;
        float speedFactor = Mathf.InverseLerp(20f, 0f, speed);

        float turnStrength = Mathf.Lerp(20f, 120f, speedFactor);

        return forwardDir * Mathf.Abs(sidewaysVel) * turnStrength;
    }

    Vector3 CalculateAlignForce(Vector3 rightDir, float sidewaysVel)
    {
        return -rightDir * sidewaysVel * 2f;
    }

    void ApplyAutoStop(float accelInput)
    {
        float speed = rb.linearVelocity.magnitude;
        if (speed < 1f && Mathf.Abs(accelInput) < 0.1f)
        {
            rb.linearVelocity *= 0.9f;
        }
    }

    void ApplyDownforce()
    {
        float speed = rb.linearVelocity.magnitude;
        float suspensionRatio = Mathf.InverseLerp(minLength, maxLength, springLength);
        float downforceFactor = Mathf.Clamp01(suspensionRatio);

        Vector3 downforce = -transform.up * speed * speed * 5f * downforceFactor;
        rb.AddForce(downforce);
    }

    void ApplyBrake(Vector3 forwardDir, float forwardVel)
    {
        float brakeInput = CarInput.GetBrakeInput();
        if (brakeInput < 0.1f) return;

        float brakeForceMag = brakeInput * motorForce * brakeForceMultiplier;
        Vector3 brakeForce = -forwardDir * Mathf.Sign(forwardVel) * brakeForceMag;

        rb.AddForceAtPosition(brakeForce, transform.position);

        if (rb.linearVelocity.magnitude < 2f)
            rb.linearVelocity *= 1f - brakeInput * 0.2f;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        float currentSpring = Application.isPlaying ? springLength : restLength;
        Vector3 wheelCenter = transform.position - transform.up * currentSpring;

        int segments = 36;
        Vector3 prevPoint = wheelCenter + transform.up * wheelRadius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;

            Vector3 nextPoint = wheelCenter
                + transform.up * Mathf.Cos(angle) * wheelRadius
                + transform.forward * Mathf.Sin(angle) * wheelRadius;

            Gizmos.DrawLine(prevPoint, nextPoint);
            prevPoint = nextPoint;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(wheelCenter, wheelCenter + transform.forward * wheelRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(wheelCenter, wheelCenter + transform.up * wheelRadius);
    }
}