using UnityEngine;

[CreateAssetMenu(fileName = "Cargo", menuName = "Deliveries/Cargo")]
public class Cargo : ScriptableObject
{
    [Header("Cargo Details")]
    public float value;
    public GameObject cargoPrefab;

}
