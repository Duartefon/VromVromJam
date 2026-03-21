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
    public float springStiffness = 30_000f;
    public float damperStiffness = 4_000f;

    [Header("Wheel")]
    public float wheelRadius = 0.25f;
    public float steeringAngle;
    public float steerTime = 10f;
    public float tireGrip = 750f;
    public Position position;

    [Header("Mesh")]
    public Transform wheelMesh;

    private float maxLength, minLength, springLength;
    private float previousSpringLength;
    private float springForce;
    private Vector3 suspensionForce;
    private Vector3 wheelVelocity; // local space
    private float springVelocity;
    private float damperForce;
    private float wheelAngle;
    private float wheelSpinAngle;

    void Start()
    {
        rb = transform.parent.GetComponent<Rigidbody>();

        minLength = restLength - springTravel;
        maxLength = restLength + springTravel;
        springLength = restLength;
    }

    void Update()
    {
        UpdateWheelRotation();
    }

    void FixedUpdate()
    {
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, maxLength + wheelRadius))
        {
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
            UpdateWheelMesh();
        }
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
        return forwardDir * Mathf.Abs(sidewaysVel) * 50f;
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