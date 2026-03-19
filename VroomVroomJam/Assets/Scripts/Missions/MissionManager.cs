using System;
using System.Collections.Generic;
using UnityEngine;

namespace Missions
{
    public class MissionManager : MonoBehaviour
    {
        [Header("Current Mission State")]
        public Mission currentMissionAsset;
        public bool isMissionActive = false;
        public Transform dropPosition;
        public float verticalOffsetBetweenItems = 2f;

        [SerializeField] private List<Deliverable> activeOrders = new List<Deliverable>();

        // --- EVENT BUS SUBSCRIPTIONS ---
        private void OnEnable()
        {
            MissionEventBus.OnPlayerReachedPickup += HandlePickupZone;
            MissionEventBus.OnPlayerReachedDestination += HandleDestinationZone;
            
            // NEW: Listen for when an item is lost/destroyed
            MissionEventBus.OnDeliverableLost += HandleDeliverableLost; 
        }

        private void OnDisable()
        {
            MissionEventBus.OnPlayerReachedPickup -= HandlePickupZone;
            MissionEventBus.OnPlayerReachedDestination -= HandleDestinationZone;
            MissionEventBus.OnDeliverableLost -= HandleDeliverableLost;
        }

        public void Start()
        {
            LoadMission(currentMissionAsset);
        }

        // --- MISSION LOGIC --- 
        public void LoadMission(Mission mission)
        {
            if (isMissionActive) return;

            currentMissionAsset = mission;
            activeOrders.Clear();

            foreach (var order in mission.ordersToBeDelivered)
            {
                Deliverable runtimeOrder = order;
                runtimeOrder.state = DeliveryState.Pending;
                activeOrders.Add(runtimeOrder);
            }

            isMissionActive = true;
            Debug.Log($"Mission Loaded. Head to Pickup Zone: {mission.location.originID}");
        }

        private void HandlePickupZone(string zoneID)
        {
            if (!isMissionActive) return;

            if (zoneID == currentMissionAsset.location.originID)
            {
                Debug.Log("Correct Pickup Zone reached! Loading goods into the truck...");
             
                for (int i = 0; i < activeOrders.Count; i++)
                {
                    if (activeOrders[i].state == DeliveryState.Pending)
                    {
                        Deliverable order = activeOrders[i];
                        order.state = DeliveryState.Collected;
                        activeOrders[i] = order;

                        GameObject orderInstance = Instantiate(order.deliverableModel);
                        orderInstance.transform.position = dropPosition.position + new Vector3(0, i * verticalOffsetBetweenItems, 0);
                        
                        // NEW: Tell the physical object which order it represents
                        PhysicalDeliverable physicalScript = orderInstance.GetComponent<PhysicalDeliverable>();
                        if (physicalScript != null)
                        {
                            physicalScript.orderIndex = i;
                        }
                    }
                }
                Debug.Log($"Goods collected! Now head to: {currentMissionAsset.location.destinationID}");
            }
        }

        // NEW: Triggers when a box falls off the truck
        private void HandleDeliverableLost(int index)
        {
            if (!isMissionActive || index < 0 || index >= activeOrders.Count) return;

            Deliverable order = activeOrders[index];
            
            // Only mark it destroyed if it was currently in transit
            if (order.state == DeliveryState.Collected)
            {
                order.state = DeliveryState.Destroyed;
                activeOrders[index] = order;
                Debug.Log($"Box {index} was lost on the road!");
                
                CheckForEarlyFailure();
            }
        }

        private void CheckForEarlyFailure()
        {
            bool allDestroyed = true;
            foreach (var order in activeOrders)
            {
                if (order.state != DeliveryState.Destroyed)
                {
                    allDestroyed = false;
                    break;
                }
            }

            // If we loop through everything and they are all destroyed, cancel the mission
            if (allDestroyed)
            {
                Debug.Log("All goods were lost on the road!");
                FailMission();
            }
        }

        private void HandleDestinationZone(string zoneID)
        {
            if (!isMissionActive) return;

            if (zoneID == currentMissionAsset.location.destinationID)
            {
                Debug.Log("Correct Destination Zone reached! Offloading goods...");

                float earnedReward = 0f;
                int deliveredCount = 0;

                // Only count items that are STILL 'Collected' (meaning they didn't fall out)
                for (int i = 0; i < activeOrders.Count; i++)
                {
                    if (activeOrders[i].state == DeliveryState.Collected)
                    {
                        Deliverable order = activeOrders[i];
                        order.state = DeliveryState.Delivered;
                        activeOrders[i] = order;

                        earnedReward += order.price;
                        deliveredCount++;
                    }
                }

                // If the player arrived with at least 1 item, they succeed partially
                if (deliveredCount > 0)
                {
                    CompleteMission(earnedReward, deliveredCount);
                }
                else
                {
                    Debug.Log("You arrived at the destination, but the truck was empty!");
                    FailMission();
                }
            }
        }

        private void CompleteMission(float finalReward, int amountDelivered)
        {
            isMissionActive = false;
            Debug.Log($"Mission Complete! Delivered {amountDelivered}/{activeOrders.Count} items. Earned ${finalReward}");
        }

        private void FailMission()
        {
            isMissionActive = false;
            Debug.Log("Mission Failed! You didn't deliver anything.");
        }
    }
}