using UnityEngine;

public class WheelControl : MonoBehaviour
{
    [Header("References")]
    public Transform wheelModel;

    [Header("Settings")]
    public bool steerable;
    public bool motorized;

    [Header("Effects")]
    public ParticleSystem skidSmoke; // fumo a sair das rodas tipo areia e isso
    public TrailRenderer skidMark;       // marca no chjaoi
    public float skidThreshold = 0.4f;   // quando escorrega ativa o skidMArk

    [HideInInspector] public WheelCollider WheelCollider;

    private void Start()
    {
        WheelCollider = GetComponent<WheelCollider>();
        skidSmoke = GetComponentInChildren<ParticleSystem>();

        if (skidMark != null)
            skidMark.emitting = false;
    }

    void Update()
    {
        WheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);
        wheelModel.position = position;
        wheelModel.rotation = rotation;

        if (skidSmoke != null)
        {
            skidSmoke.transform.position = new Vector3(position.x, position.y - 0.2f, position.z);

        }

        // mete o trail no chao
        if (skidMark != null)
        {
            WheelCollider.GetGroundHit(out WheelHit hit);
            skidMark.transform.position = hit.point + Vector3.up * 0.02f;
            skidMark.transform.rotation = Quaternion.LookRotation(transform.forward, hit.normal); ;
        }

        HandleSkidEffects();
    }

    void HandleSkidEffects()
    {
        if (skidSmoke == null && skidMark == null) return;

        WheelHit hit;
        bool grounded = WheelCollider.GetGroundHit(out hit);

        bool isSkidding;

        if (motorized)
        {
            // back wheels - wheelspin e understeer
            isSkidding = grounded && (
                Mathf.Abs(hit.forwardSlip) > skidThreshold ||
                Mathf.Abs(hit.sidewaysSlip) > skidThreshold);
        }
        else
        {
            // ffront wheels — undertseer
            isSkidding = grounded && Mathf.Abs(hit.sidewaysSlip) > skidThreshold;
        }

        if (skidSmoke != null)
        {
            if (isSkidding && !skidSmoke.isPlaying) skidSmoke.Play();
            if (!isSkidding && skidSmoke.isPlaying) skidSmoke.Stop();
        }

        if (skidMark != null)
            skidMark.emitting = isSkidding && grounded;
    }

    // para dbug
    public void DrawGizmo()
    {
        if (WheelCollider == null) return;

        WheelCollider.GetWorldPose(out Vector3 pos, out _);
        Gizmos.color = WheelCollider.isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(pos, WheelCollider.radius);
    }
}