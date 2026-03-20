using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class NameEntryController : MonoBehaviour
{
    public TMP_Text[] letterSlots;

    private string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private int[] letterIndexes = new int[4];

    private int activeSlot = 0;

    void Start()
    {
        UpdateScreen();

        InvokeRepeating("BlinkLetter", 0.4f, 0.4f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            letterSlots[activeSlot].gameObject.SetActive(true);

            activeSlot++;

            UpdateScreen();

            if (activeSlot > 3)
            {
                StartGame();
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            letterIndexes[activeSlot]++;
            if (letterIndexes[activeSlot] > 25)
            {
                letterIndexes[activeSlot] = 0;
            }
            UpdateScreen();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            letterIndexes[activeSlot]--;
            if (letterIndexes[activeSlot] < 0)
            {
                letterIndexes[activeSlot] = 25;
            }
            UpdateScreen();
        }
    }

    void UpdateScreen()
    {
        for (int i = 0; i < 4; i++)
        {
            if (i <= activeSlot)
            {
                letterSlots[i].text = alphabet[letterIndexes[i]].ToString();
            }
            else
            {
                letterSlots[i].text = "_";
            }
        }
    }

    void BlinkLetter()
    {
        GameObject currentLetter = letterSlots[activeSlot].gameObject;

        currentLetter.SetActive(!currentLetter.activeSelf);
    }

    void StartGame()
    {
        string playerName = "";

        for (int i = 0; i < 4; i++)
        {
            playerName += letterSlots[i].text;
        }

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save();

        SceneManager.LoadScene("GameWorld");
    }
}