using Missions;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TMP_Text titleText, payText, playerMoneyText;
    public DeliveryManager deliveryManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(deliveryManager.CurrentDelivery == null)
        {
            titleText.text = "No Delivery";
            payText.text = "0$";
        } else
        {
            titleText.text = deliveryManager.CurrentDelivery.deliveryName;
            payText.text = deliveryManager.GetCurrentPayment() + "$";
        }

    }

    public void UpdateMoneyDisplay(float money)
    {
        playerMoneyText.text = money + "$";
    }
}
