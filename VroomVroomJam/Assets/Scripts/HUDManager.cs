using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TMP_Text titleText, payText, playerMoneyText;
    public Image accelBar, brakeBar;
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

        accelBar.fillAmount = Mathf.Lerp(accelBar.fillAmount, Mathf.Abs(CarInput.GetMovementInput().y), Time.deltaTime * 20f);
        brakeBar.fillAmount = Mathf.Lerp(brakeBar.fillAmount, CarInput.GetBrakeInput(), Time.deltaTime * 20f);

    }

    public void UpdateMoneyDisplay(float money)
    {
        playerMoneyText.text = money + "$";
        playerMoneyText.GetComponent<Animator>().SetTrigger("Bounce");
    }
}
