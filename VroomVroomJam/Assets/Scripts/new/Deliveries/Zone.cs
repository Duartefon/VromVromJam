using UnityEngine;

public class Zone : MonoBehaviour
{
    [Header("Settings")]
    public Type type;
    public Delivery delivery; // para saber qual entrega esta zona pertence, caso seja uma zona de pickup ou delivery

    [Header("References")]
    public DeliveryManager deliveryManager;

    public enum Type { Pickup, Delivery }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Zone entered. " + other.name + " in zone " + name + " other tag: " + other.tag);
        if (!other.CompareTag("Player")) return;
        Debug.Log("Player in zone. " + other.name);
        if (delivery != deliveryManager.CurrentDelivery) return;
        Debug.Log("Correct delivery.");
        if (type == Type.Pickup)
        {
            Debug.Log("Pickup reached.");
            deliveryManager.OnPickupReached();
        }
        else if (type == Type.Delivery){
            Debug.Log("Delivery reached.");
            deliveryManager.OnDeliveryReached();
        }
    }

    public void SetActive(bool active)
    {
        GetComponent<Collider>().enabled = active;
    }
}