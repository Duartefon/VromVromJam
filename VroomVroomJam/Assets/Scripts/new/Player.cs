using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance;
    public static float totalMoney = 0f;

    private void Awake()
    {
        instance = this;
    }

    public void AddMoney(float amount)
    {
        totalMoney += amount;
    }

    public void SpendMoney(float amount)
    {
        totalMoney -= amount;
    }

    public bool HasMoney(float amount)
    {
        return totalMoney >= amount;
    }

    public void ResetMoney()
    {
        totalMoney = 0f;
    }

    public float GetMoney()
    {
        return totalMoney;
    }
}
