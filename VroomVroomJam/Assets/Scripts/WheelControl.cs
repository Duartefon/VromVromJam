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

        if (skidMark != null)
            skidMark.emitting = false;
    }

    void Update()
    {
        WheelCollider.GetWorldPose(out Vector3 position, out Quaternion rotation);
        wheelModel.position = position;
        wheelModel.rotation = rotation;

        HandleSkidEffects();
    }

    void HandleSkidEffects()
    {
        if (skidSmoke == null && skidMark == null) return;

        WheelHit hit;
        bool grounded = WheelCollider.GetGroundHit(out hit);

        // ~0 = grip, ~1 = full skid
        bool isSkidding = grounded && Mathf.Abs(hit.sidewaysSlip) > skidThreshold;

        if (skidSmoke != null)
        {
            if (isSkidding && !skidSmoke.isPlaying) skidSmoke.Play();
            if (!isSkidding && skidSmoke.isPlaying) skidSmoke.Stop();
        }

        if (skidMark != null)
            skidMark.emitting = isSkidding;
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