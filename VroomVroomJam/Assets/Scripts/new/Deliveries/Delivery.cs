using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Delivery", menuName = "Deliveries/Delivery")]
public class Delivery : ScriptableObject
{
    [Header("Delivery Details")]
    public string deliveryName;
    public float deliveryDuration;
    public Cargo[] cargoData;
    public List<GameObject> runtimeCargo = new List<GameObject>();
    public bool isCompleted;

    private float totalPayment;

    public float GetTotalPayment()
    {
        totalPayment = 0f;
        foreach (Cargo item in cargoData)
        {
            totalPayment += item.value;
        }
        return totalPayment;
    }
}
