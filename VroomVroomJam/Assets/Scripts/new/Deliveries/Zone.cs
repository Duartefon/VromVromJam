using UnityEngine;

public class Zone : MonoBehaviour
{
    [Header("Settings")]
    public Type type;
    public Delivery delivery; // para saber qual entrega esta zona pertence, caso seja uma zona de pickup ou delivery

    [Header("References")]
    public DeliveryManager deliveryManager;

    public enum Type { Pickup, Delivery }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (type == Type.Pickup)
                deliveryManager.SetPlayerInPickupZone(true);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (type == Type.Pickup)
                deliveryManager.SetPlayerInPickupZone(false);
        }
    }

    public void SetActive(bool active)
    {
        GetComponent<Collider>().enabled = active;
    }
}