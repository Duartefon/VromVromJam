using UnityEngine;

[CreateAssetMenu(fileName = "Delivery", menuName = "Deliveries/Delivery")]
public class Delivery : ScriptableObject
{
    [Header("Delivery Details")]
    public string deliveryName;
    public float deliveryDuration;
    public Cargo[] cargo;

    private float totalPayment;

    public float GetTotalPayment()
    {
        totalPayment = 0f;
        foreach (Cargo item in cargo)
        {
            totalPayment += item.value;
        }
        return totalPayment;
    }
}
