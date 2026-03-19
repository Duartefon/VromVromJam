using System.Collections.Generic;
using UnityEngine;

namespace Missions
{
    [CreateAssetMenu(fileName = "NewFetchMission", menuName = "Mission System/Fetch Mission")]
    public class Mission : ScriptableObject

    {
        public string missionName = "Unnamed Fetch Quest";
        public MissionLocation location;
        public float duration;
    
        [Tooltip("The list of items the player must deliver.")]
        public List<Deliverable> ordersToBeDelivered;

        // Calculates the sum of all item prices dynamically
        public float TotalReward
        {
            get
            {
                float sum = 0f;
                foreach (var item in ordersToBeDelivered)
                {
                    sum += item.price;
                }
                return sum;
            }
        }
    }
}