using UnityEngine;

public class NewCarController : MonoBehaviour
{
    public Wheel[] wheels;

    [Header("Car Settings")]
    public float wheelBase = 2.1f; // metros
    public float rearTrackWidth = 1.2f; // metros
    public float turnRadius = 9f; // metros

    private float ackermanAngleLeft, ackermanAngleRight;

    void Start()
    {
        
    }

    void Update()
    {
        float steerInput = CarInput.GetMovementInput().x;
        
        if ( steerInput > 0 )
        {
            // virar para a direita
            ackermanAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrackWidth / 2)) * steerInput;
            ackermanAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrackWidth / 2)) * steerInput;
        }
        else if ( steerInput < 0 )
        {
            // virar para a esquerda
            ackermanAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - rearTrackWidth / 2)) * steerInput;
            ackermanAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + rearTrackWidth / 2)) * steerInput;
        }
        else
        {
            // ir em linha reta
            ackermanAngleLeft = 0;
            ackermanAngleRight = 0;
        }

        foreach (var wheel in wheels)
        {
            if(wheel.position == Wheel.Position.FrontLeft)
                wheel.steeringAngle = ackermanAngleLeft;
            else if(wheel.position == Wheel.Position.FrontRight)
                wheel.steeringAngle = ackermanAngleRight;
        }
    }
}
