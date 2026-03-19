using Singletons;
using UnityEngine;

namespace Missions
{
    public enum ZoneType 
    { 
        Pickup, 
        Destination 
    }

    [RequireComponent(typeof(Collider))]
    public class DeliveryZone : MonoBehaviour
    {
        [Tooltip("Is this where we get the goods, or drop them off?")]
        public ZoneType zoneType;
    
        [Tooltip("Must match the originID or destinationID in your Mission ScriptableObject")]
        public string zoneID;

        [Header("Stop Detection Settings")]
        [Tooltip("How many seconds the player must be stopped")]
        public float requiredStopTime = 1.0f;
        
        [Tooltip("Maximum speed allowed to still be considered 'stopped'")]
        public float stopSpeedThreshold = 0.5f;

        private float currentStopTime = 0f;
        private bool hasTriggered = false; // Prevents the event from firing 50 times a second once complete

        private void OnTriggerStay(Collider other)
        {
            // If we already successfully triggered the event, ignore further checks
            if (hasTriggered) return;

            if (other.CompareTag("Player"))
            {
                Rigidbody playerRb = other.attachedRigidbody;

                if (playerRb != null)
                {
                    // Check if the truck is moving slower than our threshold
                    // Note: Unity 6 uses linearVelocity. (Older versions used just 'velocity')
                    if (playerRb.linearVelocity.magnitude <= stopSpeedThreshold)
                    {
                        // Add the time passed since the last physics frame
                        currentStopTime += Time.fixedDeltaTime;

                        if (currentStopTime >= requiredStopTime)
                        {
                            TriggerZoneEvent();
                        }
                    }
                    else
                    {
                        // The player moved! Reset the timer.
                        currentStopTime = 0f;
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            // Reset everything if the player drives out of the zone
            if (other.CompareTag("Player"))
            {
                currentStopTime = 0f;
                hasTriggered = false; 
            }
        }

        private void TriggerZoneEvent()
        {
            hasTriggered = true;
            Debug.Log($"Player successfully parked in {zoneType} zone: {zoneID}");

            if (zoneType == ZoneType.Pickup)
            {
                MissionEventBus.RaisePlayerReachedPickup(zoneID);
            }
            else if (zoneType == ZoneType.Destination)
            {
                Debug.Log("Cheguei aqui");
                MissionEventBus.RaisePlayerReachedDestination(zoneID);
            }
        }
    }
}