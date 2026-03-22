using UnityEngine;

public class CargoInstance
{
    public Cargo cargoData;
    public GameObject cargoObject;
    public bool isBroken;

    public CargoInstance(Cargo cargoData, GameObject cargoObject)
    {
        this.cargoData = cargoData;
        this.cargoObject = cargoObject;
        this.isBroken = false;
    }

    

}
