using Unity.Entities;
using Unity.Vehicles;
using Unity.Vehicles.Samples;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(VehicleControlSystem))]
public partial struct MinimalVehicleControlSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        MinimalInputActions.DefaultMapActions defaultMapActions = MinimalInputResources.InputActions.DefaultMap;

        foreach (var (controller, vehicleControl) in
                 SystemAPI.Query<MinimalPlayerController, RefRW<VehicleControl>>())
        {
            vehicleControl.ValueRW.RawSteeringInput = defaultMapActions.Steering.ReadValue<float>();
            vehicleControl.ValueRW.RawThrottleInput = defaultMapActions.Throttle.ReadValue<float>();
            vehicleControl.ValueRW.RawBrakeInput = defaultMapActions.Brake.ReadValue<float>();
            vehicleControl.ValueRW.HandbrakeInput = defaultMapActions.Handbrake.ReadValue<float>();
            vehicleControl.ValueRW.ShiftUpInput = default;
            if (defaultMapActions.ShiftUp.WasPressedThisFrame())
            {
                vehicleControl.ValueRW.ShiftUpInput = true;
            }

            vehicleControl.ValueRW.ShiftDownInput = default;
            if (defaultMapActions.ShiftDown.WasPressedThisFrame())
            {
                vehicleControl.ValueRW.ShiftDownInput = true;
            }

            vehicleControl.ValueRW.EngineStartStopInput = default;
            if (defaultMapActions.EngineStartStop.WasPressedThisFrame())
            {
                vehicleControl.ValueRW.EngineStartStopInput = true;
            }
        }
    }
}