using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DeliveryManager : MonoBehaviour
{
    [Header("References")]
    public Timer timer;
    public ArrowScript arrow;
    public Delivery[] deliveries;
    public HUDManager hudManager;
    public Transform dropPosition;

    [Header("Settings")]
    public float cargoSpawnInterval = 0.3f;

    private Delivery currentDelivery;
    public Delivery CurrentDelivery => currentDelivery;

    private enum State { Idle, GoingToPickup, GoingToDeliver }
    private State state = State.Idle;
    private Zone[] zones;

    void Start()
    {
        GameObject arrowObj = GameObject.FindWithTag("Arrow");
        if (arrowObj != null)
        {
            arrow = arrowObj.GetComponent<ArrowScript>();
            arrow.Enable();
        }
        else
        {
            Debug.LogWarning("Arrow object not found.");
        }

        zones = FindObjectsByType<Zone>(FindObjectsSortMode.None);

        ChooseRandomDelivery();
        StartDelivery();
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

        timer.StartTimer(currentDelivery.deliveryDuration);

        Zone pickup = GetZone(currentDelivery, Zone.Type.Pickup);
        Zone deliver = GetZone(currentDelivery, Zone.Type.Delivery);

        if (pickup != null) pickup.SetActive(true);
        if (deliver != null) deliver.SetActive(false);

        if (arrow != null && pickup != null)
            arrow.SetTarget(pickup.transform);

        if (hudManager != null)
        {
            // hudManager.UpdateMissionDisplay(currentDelivery);
        }
    }

    private void DoPickup()
    {
        if (currentDelivery == null) return;

        state = State.GoingToDeliver;
        Zone pickup = GetZone(currentDelivery, Zone.Type.Pickup);
        Zone deliver = GetZone(currentDelivery, Zone.Type.Delivery);

        StartCoroutine(WaitForCargo(pickup, deliver));
    }

    private IEnumerator WaitForCargo(Zone pickup, Zone deliver)
    {
        yield return new WaitForSeconds(2f);

        StartCoroutine(SpawnCargo(currentDelivery));

        if (pickup != null) pickup.SetActive(false);
        if (deliver != null) deliver.SetActive(true);

        if (arrow != null && deliver != null)
            arrow.SetTarget(deliver.transform);

        if (hudManager != null)
        {
        }
    }

    private void CompleteDelivery()
    {
        if (currentDelivery == null) return;

        timer.StopTimer();

        StartCoroutine(DeliverCargo(currentDelivery));
    }

    private void ChooseRandomDelivery()
    {
        var available = System.Array.FindAll(deliveries, d => !d.isCompleted);
        if (available.Length == 0)
        {
            Debug.Log("All deliveries completed!");
            currentDelivery = null;
            if (arrow != null) arrow.Disable();
            return;
        }

        arrow.Enable();
        currentDelivery = available[Random.Range(0, available.Length)];
    }

    private IEnumerator SpawnCargo(Delivery delivery)
    {
        if (delivery.runtimeCargo == null)
            delivery.runtimeCargo = new List<GameObject>();
        else
        {
            foreach (var old in delivery.runtimeCargo)
            {
                if (old != null)
                    Destroy(old);
            }
            delivery.runtimeCargo.Clear();
        }

        foreach (Cargo data in delivery.cargoData)
        {
            GameObject cargo = Instantiate(data.cargoPrefab, dropPosition.position, Quaternion.identity);

            CargoBehaviour behaviour = cargo.GetComponent<CargoBehaviour>();
            if (behaviour != null)
            {
                behaviour.cargoData = data;
            }
            else
            {
                Debug.LogWarning("Cargo prefab missing CargoBehaviour!");
            }

            delivery.runtimeCargo.Add(cargo);

            yield return new WaitForSeconds(cargoSpawnInterval);
        }
    }

    private IEnumerator DeliverCargo(Delivery delivery)
    {
        float totalPayment = 0f;

        foreach (var cargo in delivery.runtimeCargo)
        {
            if (cargo == null) continue;

            CargoBehaviour behaviour = cargo.GetComponent<CargoBehaviour>();

            if (behaviour != null)
            {
                if (!behaviour.isBroken)
                {
                    totalPayment += behaviour.cargoData.value;
                    Destroy(cargo, 1f);
                }
                else
                {
                    // fica sem pagamento, mas ainda é destruído
                    Destroy(cargo, 1f);
                }
            }

            yield return new WaitForSeconds(cargoSpawnInterval);
        }

        Debug.Log($"Delivery completed! Payment: {totalPayment}");

        if (hudManager != null)
        {
            // hudManager.UpdatePaymentDisplay(totalPayment);
        }

        delivery.isCompleted = true;

        ChooseRandomDelivery();
        StartDelivery();
    }
}