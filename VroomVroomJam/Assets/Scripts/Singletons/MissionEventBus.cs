using System;
using UnityEngine;

public static class MissionEventBus
{
    // The events that other scripts can listen to
    public static event Action<string> OnPlayerReachedPickup;
    public static event Action<string> OnPlayerReachedDestination;
    public static event Action<int> OnDeliverableLost;

    // Helper methods for scripts to trigger the events safely
    public static void RaisePlayerReachedPickup(string zoneID )
    {
        OnPlayerReachedPickup?.Invoke(zoneID);
    }

    public static void RaisePlayerReachedDestination(string zoneID)
    {
        OnPlayerReachedDestination?.Invoke(zoneID);
    }

    public static void RaiseDeliverableLost(int index)
    {
        OnDeliverableLost?.Invoke(index);
    }
}