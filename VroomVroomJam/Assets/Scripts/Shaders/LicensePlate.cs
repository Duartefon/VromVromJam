using UnityEngine;
using TMPro;

public class LicensePlate : MonoBehaviour
{
    public static LicensePlate Instance { get; private set; }
    public static string CurrentPlateText { get; private set; } = "0000000"; // funciona como singleton para guardar a 
                                                                            // placa atual e ser acessada por outros scripts
    public Renderer plateRenderer;
    public TMP_InputField inputField;
    private Material mat;
    private const string ATLAS_CHARS = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    void Start()
    {
        plateRenderer = GameObject.Find("LicensePlate").GetComponent<Renderer>();
        mat = plateRenderer.material;
        if(inputField != null)
        {
            inputField.onValueChanged.AddListener(OnTextChanged);
            inputField.characterLimit = 7;
        }
        
    }

    void OnTextChanged(string text)
    {
        SetPlateText(text);
    }

    public void SetPlateText(string text)
    {
        text = text.ToUpper();
        CurrentPlateText = text;
        for (int i = 0; i < 7; i++)
        {
            char c = i < text.Length ? text[i] : '0';
            int sliceIndex = ATLAS_CHARS.IndexOf(c);
            if (sliceIndex == -1) sliceIndex = 0;
            mat.SetInt("_Char" + i, sliceIndex);
        }
    }
}