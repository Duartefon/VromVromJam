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
        if (!other.CompareTag("Player")) return;

        if (delivery != deliveryManager.CurrentDelivery) return;

        if (type == Type.Pickup)
            deliveryManager.OnPickupReached();
        else if (type == Type.Delivery)
            deliveryManager.OnDeliveryReached();
    }

    public void SetActive(bool active)
    {
        GetComponent<Collider>().enabled = active;
        GetComponent<MeshRenderer>().enabled = active;
    }
}