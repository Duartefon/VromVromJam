using UnityEngine;

public class CarInput : MonoBehaviour
{
    [Header("Input Settings")]
    public CarInputActions carControls;

    private static CarInput instance;

    void Awake()
    {
        instance = this;
        carControls = new CarInputActions();
    }

    void OnEnable() { carControls.Enable(); }
    void OnDisable() { carControls.Disable(); }

    public static Vector2 GetMovementInput()
    {
        return instance.carControls.Car.Movement.ReadValue<Vector2>();
    }
}