using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Missions
{
    public class MissionManager : MonoBehaviour
    {
        [Header("Current Mission State")]
        public Mission currentMissionAsset;
        public bool isMissionActive = false;
        public Transform dropPosition;
        public float verticalOffsetBetweenItems = 2f;

        [Header("Timer")]
        public Timer timer;

        [SerializeField] private List<Deliverable> activeOrders = new List<Deliverable>();

        // --- EVENT BUS SUBSCRIPTIONS ---
        private void OnEnable()
        {
            MissionEventBus.OnPlayerReachedPickup += HandlePickupZone;
            MissionEventBus.OnPlayerReachedDestination += HandleDestinationZone;
        }

        private void OnDisable()
        {
            MissionEventBus.OnPlayerReachedPickup -= HandlePickupZone;
            MissionEventBus.OnPlayerReachedDestination -= HandleDestinationZone;
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

            if (timer != null)
            {
                timer.duration = mission.duration;
                timer.OnTimerComplete += HandleTimerExpired;
                timer.StartTimer();
            }

            Debug.Log($"Mission Loaded: {mission.missionName} | Time Limit: {mission.duration}s | Head to: {mission.location.originID}");
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
                    }
                }

                Debug.Log($"Goods collected! Now head to: {currentMissionAsset.location.destinationID}");
            }
        }

        private void HandleDestinationZone(string zoneID)
        {
            if (!isMissionActive) return;

            if (zoneID == currentMissionAsset.location.destinationID)
            {
                Debug.Log("Correct Destination Zone reached! Offloading goods...");

                for (int i = 0; i < activeOrders.Count; i++)
                {
                    if (activeOrders[i].state == DeliveryState.Collected)
                    {
                        Deliverable order = activeOrders[i];
                        order.state = DeliveryState.Delivered;
                        activeOrders[i] = order;
                    }
                }

                CheckMissionProgress();
            }
        }

        private void CheckMissionProgress()
        {
            bool allDelivered = true;

            foreach (var order in activeOrders)
            {
                if (order.state == DeliveryState.Destroyed)
                {
                    FailMission();
                    return;
                }
                if (order.state != DeliveryState.Delivered)
                {
                    allDelivered = false;
                }
            }

            if (allDelivered) CompleteMission();
        }

        private void CompleteMission()
        {
            isMissionActive = false;
            StopTimer();
            Debug.Log($"Mission Complete! Earned ${currentMissionAsset.TotalReward}");
        }

        private void FailMission()
        {
            isMissionActive = false;
            StopTimer();
            Debug.Log("Mission Failed! Goods were destroyed.");
        }

        // --- TIMER HELPERS ---
        private void HandleTimerExpired()
        {
            if (!isMissionActive) return;
            FailMission();
            Debug.Log("Mission Failed! Ran out of time.");
        }

        private void StopTimer()
        {
            if (timer != null)
            {
                timer.OnTimerComplete -= HandleTimerExpired;
                timer.StopTimer();
            }
        }
    }
}