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
    public Animator failAnimator;

    [Header("Settings")]
    public float cargoSpawnInterval = 0.3f;

    private Delivery currentDelivery;
    public Delivery CurrentDelivery => currentDelivery;

    private enum State { Idle, GoingToPickup, GoingToDeliver, Delivering }
    private State state = State.Idle;
    private Zone[] zones;

    // Coroutine guards
    private Coroutine _spawnCoroutine;
    private Coroutine _deliverCoroutine;
    private Coroutine _waitForCargoCoroutine;
    private bool isInPickupZone = false;

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

    void Update()
    {
        if (currentDelivery == null) return;

        if ((GetCurrentCargo().Count == 0 && 
            (state == State.Delivering || state == State.GoingToDeliver )
            && currentDelivery.runtimeCargo.Count > 0)  || timer.IsTimeUp())
        {
            // se o player perder a cargo toda ou se o tempo acabar, falha a entrega
            FailDelivery(); //done
            failAnimator.Play("Fail");
        }
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

        // Reset delivery state in case it was previously used
        currentDelivery.isCompleted = false;

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

        // Cancel any leftover WaitForCargo coroutine
        if (_waitForCargoCoroutine != null)
        {
            StopCoroutine(_waitForCargoCoroutine);
            _waitForCargoCoroutine = null;
        }

        _waitForCargoCoroutine = StartCoroutine(WaitForCargo(pickup, deliver));
    }

    public void SetPlayerInPickupZone(bool value)
    {
        isInPickupZone = value;

        // se o player sair, cancela tudo
        if (!value)
        {
            if (_waitForCargoCoroutine != null)
            {
                StopCoroutine(_waitForCargoCoroutine);
                _waitForCargoCoroutine = null;
            }

            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }

            Debug.Log("Player left pickup zone, canceling cargo spawn.");
        }
    }

    private IEnumerator WaitForCargo(Zone pickup, Zone deliver)
    {
        yield return new WaitForSeconds(2f);

        if (!isInPickupZone)
            yield break;

        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        _spawnCoroutine = StartCoroutine(SpawnCargo(currentDelivery));

        if (pickup != null) pickup.SetActive(false);
        if (deliver != null) deliver.SetActive(true);

        if (arrow != null && deliver != null)
            arrow.SetTarget(deliver.transform);
    }

    private void CompleteDelivery()
    {
        if (currentDelivery == null) return;

        // Already delivering, don't trigger again
        if (state == State.Delivering) return;

        state = State.Delivering;

        timer.StopTimer();

        // Cancel any in-progress spawn so we don't deliver cargo that hasn't spawned yet
        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }

        if (_deliverCoroutine != null)
        {
            StopCoroutine(_deliverCoroutine);
            _deliverCoroutine = null;
        }

        _deliverCoroutine = StartCoroutine(DeliverCargo(currentDelivery));
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
        if (delivery == null) yield break;

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
            if (dropPosition == null)
            {
                Debug.LogWarning("dropPosition is null, cannot spawn cargo.");
                yield break;
            }

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

        _spawnCoroutine = null;
    }

    private IEnumerator DeliverCargo(Delivery delivery)
    {
        if (delivery == null) yield break;

        foreach (var cargo in GetCurrentCargo())
        {
            if (cargo == null) continue;

            CargoBehaviour behaviour = cargo.GetComponent<CargoBehaviour>();
            float pay = 0f;

            if (behaviour != null && !behaviour.isBroken)
            {
                pay = behaviour.cargoData.value;
            }

            Destroy(cargo, 1f);

            if (pay > 0f)
            {
                Player.instance.AddMoney(pay);
                Debug.Log("Delivered cargo for " + pay + "$");
                
                if (hudManager != null)
                {
                    hudManager.UpdateMoneyDisplay(Player.instance.GetMoney());
                }
            }

            yield return new WaitForSeconds(cargoSpawnInterval);
        }

        delivery.runtimeCargo.Clear();

        delivery.isCompleted = true;
        _deliverCoroutine = null;

        state = State.Idle;

        ChooseRandomDelivery();
        StartDelivery();
    }

    public float GetCurrentPayment()
    {
        if (currentDelivery == null) return 0f;

        float total = 0f;

        foreach (var cargo in GetCurrentCargo())
        {
            if (cargo == null) continue;

            CargoBehaviour behaviour = cargo.GetComponent<CargoBehaviour>();

            if (behaviour != null && !behaviour.isBroken)
            {
                total += behaviour.cargoData.value;
            }
        }
        return total;
    }

    private List<GameObject> GetCurrentCargo()
    {
        if (currentDelivery == null) return new List<GameObject>();
        
        List<GameObject> unbrokenCargo = new();

        foreach (var cargo in currentDelivery.runtimeCargo)
        {
            if (cargo == null) continue;

            CargoBehaviour behaviour = cargo.GetComponent<CargoBehaviour>();

            if (behaviour != null && !behaviour.isBroken)
            {
                unbrokenCargo.Add(cargo);
            }
        }
        return unbrokenCargo;
    }

    private void FailDelivery()
    {
        if (currentDelivery == null) return;

        if (_spawnCoroutine != null)
        {
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null;
        }
        if (_deliverCoroutine != null)
        {
            StopCoroutine(_deliverCoroutine);
            _deliverCoroutine = null;
        }
        if (_waitForCargoCoroutine != null)
        {
            StopCoroutine(_waitForCargoCoroutine);
            _waitForCargoCoroutine = null;
        }

        foreach (var cargo in GetCurrentCargo())
        {
            if (cargo != null)
                Destroy(cargo);
        }
        currentDelivery.runtimeCargo.Clear();

        Zone pickup = GetZone(currentDelivery, Zone.Type.Pickup);
        Zone deliver = GetZone(currentDelivery, Zone.Type.Delivery);
        if (pickup != null) pickup.SetActive(false);
        if (deliver != null) deliver.SetActive(false);

        timer.StopTimer();

        Debug.Log("Delivery failed!");

        state = State.Idle;
        currentDelivery.isCompleted = true;
        ChooseRandomDelivery();
        StartDelivery();
    }
}