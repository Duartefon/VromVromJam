using Missions;
using UnityEngine;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public TMP_Text titleText, payText;
    public MissionManager missionManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        titleText.text = missionManager.currentMissionAsset.missionName;
        payText.text = $"Pay: {missionManager.currentMissionAsset.TotalReward}$";
    }
}
