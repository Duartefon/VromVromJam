using UnityEngine;
using System;
using System.Collections.Generic;

namespace Missions
{
    public enum ItemType
    {
        Crate,
        PizzaBox
 
    }

    public enum DeliveryState
    {
        Pending,
        Collected,
        Delivered,
        Destroyed // Triggers a mission failure
    }

    [Serializable]
    public struct Deliverable
    {
        public ItemType itemType;
        public float price;
        public DeliveryState state;
        public GameObject deliverableModel;
    }

    [Serializable]
    public struct MissionLocation
    {
        [Tooltip("The ID of the pickup location (e.g., 'Warehouse_1')")]
        public string originID;

        [Tooltip("The ID of the drop-off location (e.g., 'City_Center')")]
        public string destinationID;
    }
}