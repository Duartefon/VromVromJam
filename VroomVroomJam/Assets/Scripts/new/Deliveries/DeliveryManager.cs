using UnityEngine;


public class DeliveryManager : MonoBehaviour
{
    [Header("References")]
    public Timer timer;
    public ArrowScript arrow;
    public Delivery[] deliveries;
    public HUDManager hudManager;


    private Delivery currentDelivery;
    public Delivery CurrentDelivery => currentDelivery;

    private enum State { Idle, GoingToPickup, GoingToDeliver }
    private State state = State.Idle;
    private Zone[] zones;
    
    void Start()
    {
        arrow = GameObject.FindWithTag("Arrow").GetComponent<ArrowScript>();
        zones = FindObjectsByType<Zone>(FindObjectsSortMode.None);

        ChooseRandomDelivery();
        StartDelivery();
    }

    void Update()
    {
        
    }

    private Zone GetZone(Delivery delivery, Zone.Type type)
    {
        foreach (Zone zone in zones)
        {
            if (zone.delivery == delivery && zone.type == type)
                return zone;
        }
        return null;
    }

    public void OnPickupReached()
    {
        if (state != State.GoingToPickup) return;
        DoPickup();
    }

    public void OnDeliveryReached()
    {
        if (state != State.GoingToDeliver) return;
        CompleteDelivery();
    }


    private void StartDelivery()
    {
        if (currentDelivery == null) return;

        state = State.GoingToPickup;

        Zone pickup = GetZone(currentDelivery, Zone.Type.Pickup);
        Zone deliver = GetZone(currentDelivery, Zone.Type.Delivery);

        if (pickup != null)  pickup.SetActive(true);
        if (deliver != null) deliver.SetActive(false);

        if (arrow != null && pickup != null)
            arrow.SetTarget(pickup.transform);

        if (hudManager != null){}
            //hudManager.UpdateDeliveryDetails(currentDelivery);
    }

    private void DoPickup()
    {
        if (currentDelivery == null) return;

        state = State.GoingToDeliver;

        Zone pickup = GetZone(currentDelivery, Zone.Type.Pickup);
        Zone deliver = GetZone(currentDelivery, Zone.Type.Delivery);

        if (pickup != null)  pickup.SetActive(false);
        if (deliver != null) deliver.SetActive(true);

        if (arrow != null && deliver != null)
            arrow.SetTarget(deliver.transform);
    }

    private void CompleteDelivery()
    {
        if (currentDelivery != null)
        {
            timer.StopTimer();

            state = State.Idle;

            float payment = currentDelivery.GetTotalPayment();
            Debug.Log($"Delivery completed! Payment: {payment}");

            if (hudManager != null)
            {
                //hudManager.UpdatePaymentDisplay(payment);
            }

            // ao acabar a entrega, escolher uma nova entrega aleatoria
            ChooseRandomDelivery();
            StartDelivery();
        }
    }

    private void ChooseRandomDelivery()
    {
        if (deliveries.Length == 0) return;

        int randomIndex = Random.Range(0, deliveries.Length);
        currentDelivery = deliveries[randomIndex];
    }
}
