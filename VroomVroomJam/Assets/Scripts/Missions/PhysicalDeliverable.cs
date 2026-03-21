using Singletons;
using UnityEngine;

namespace Missions
{
    public class PhysicalDeliverable : MonoBehaviour
    {
        [HideInInspector]
        public int orderIndex;

        [Header("Bed Attraction")]
        public float attractionForce = 50f;
        public float attractionRange = 2f;
        public float maxAttractionSpeed = 5f;

        [Header("Stability")]
        public float velocityMatchStrength = 0.1f;
        public float dampingFactor = 0.98f;
        public float stickDownForce = 50f;

        [Header("Box Cohesion")]
        public float cohesionForce = 20f;
        public float cohesionRange = 1.5f;
        public float cohesionDamping = 1f;

        private Rigidbody rb;
        private Transform bedTransform;
        private Rigidbody bedRb;
        private bool inBed;

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.linearDamping = 3f;
            rb.angularDamping = 5f;
            rb.mass = 0.5f;
        }

        void FixedUpdate()
        {
            if (bedTransform == null) return;

            ApplyAttraction();
            ApplyVelocityMatching();
            ApplyStabilityForces();
            ApplyCohesion();
        }

        void ApplyAttraction()
        {
            Vector3 toTarget = bedTransform.position - transform.position;
            float distance = toTarget.magnitude;

            if (distance > attractionRange) return;

            float strength = Mathf.InverseLerp(attractionRange, 0f, distance);
            Vector3 force = toTarget.normalized * attractionForce * strength;

            rb.AddForce(force, ForceMode.Acceleration);
        }

        void ApplyVelocityMatching()
        {
            if (!inBed || bedRb == null) return;
            Vector3 targetVelocity = bedRb.linearVelocity;

            rb.linearVelocity = Vector3.Lerp(
                rb.linearVelocity,
                targetVelocity,
                velocityMatchStrength
            );

            Vector3 relativeVel = rb.linearVelocity - bedRb.linearVelocity;

            if (relativeVel.magnitude > maxAttractionSpeed)
            {
                rb.linearVelocity = bedRb.linearVelocity + relativeVel.normalized * maxAttractionSpeed;
            }
        }

        void ApplyStabilityForces()
        {
            rb.linearVelocity *= dampingFactor;
            if (inBed)
            {
                rb.AddForce(-bedTransform.up * stickDownForce, ForceMode.Acceleration);
            }
        }

        void ApplyCohesion()
        {
            if (!inBed) return;

            Collider[] nearby = Physics.OverlapSphere(transform.position, cohesionRange);

            foreach (var col in nearby)
            {
                if (col.attachedRigidbody == null) continue;
                if (col.attachedRigidbody == rb) continue;

                PhysicalDeliverable other = col.GetComponent<PhysicalDeliverable>();
                if (other == null) continue;

                Vector3 toOther = col.transform.position - transform.position;
                float distance = toOther.magnitude;

                if (distance < 0.01f) continue;

                float strength = Mathf.InverseLerp(cohesionRange, 0f, distance);

                Vector3 force = toOther.normalized * cohesionForce * strength;

                Vector3 relativeVel = rb.linearVelocity - col.attachedRigidbody.linearVelocity;

                rb.AddForce(force - relativeVel * cohesionDamping, ForceMode.Acceleration);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
            {
                MissionEventBus.RaiseDeliverableLost(orderIndex);
                Destroy(gameObject);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Bed"))
            {
                bedTransform = other.transform;
                bedRb = other.attachedRigidbody;
                inBed = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Bed"))
            {
                bedTransform = null;
                bedRb = null;
                inBed = false;
            }
        }
    }
}