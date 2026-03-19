using Singletons;
using UnityEngine;

namespace Missions
{
    public class PhysicalDeliverable : MonoBehaviour
    {
        [HideInInspector] 
        public int orderIndex; // Set by the MissionManager upon instantiation

        private void OnCollisionEnter(Collision collision)
        {   
            Debug.Log(collision);
            // If the box falls and hits the road/ground, it's considered lost
            if (collision.gameObject.CompareTag("Ground"))
            {
                MissionEventBus.RaiseDeliverableLost(orderIndex);
                
                // Destroy the physical object so it disappears from the road
                Destroy(gameObject); 
            }
        }
    }
}