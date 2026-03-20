using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject sceneryCamera;
    public GameObject carCamera;
    public GameObject startText;
    public GameObject carTarget;
    public Transform[] sceneryMarkers;

    private int currentMarkerIndex = 0;

    public float cameraSwitchTime = 4.0f;
    public float textBlinkTime = 0.5f;

    public float orbitSpeed = 20f;
    public float slideSpeed = 1f;

    void Start()
    {
        if (sceneryMarkers.Length > 0)
        {
            SnapToNextMarker();
        }

        InvokeRepeating("SwitchCameras", cameraSwitchTime, cameraSwitchTime);
        InvokeRepeating("BlinkText", textBlinkTime, textBlinkTime);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            SceneManager.LoadScene("NameSelectScreen");
        }

        carCamera.transform.RotateAround(carTarget.transform.position, Vector3.up, orbitSpeed * Time.deltaTime);
        sceneryCamera.transform.Translate(Vector3.forward * slideSpeed * Time.deltaTime);
    }

    void SwitchCameras()
    {
        bool isSceneryOn = sceneryCamera.activeSelf;

        if (!isSceneryOn && sceneryMarkers.Length > 0)
        {
            SnapToNextMarker();
        }

        sceneryCamera.SetActive(!isSceneryOn);
        carCamera.SetActive(isSceneryOn);
    }

    void SnapToNextMarker()
    {
        sceneryCamera.transform.position = sceneryMarkers[currentMarkerIndex].position;
        sceneryCamera.transform.rotation = sceneryMarkers[currentMarkerIndex].rotation;

        currentMarkerIndex++;

        if (currentMarkerIndex >= sceneryMarkers.Length)
        {
            currentMarkerIndex = 0;
        }
    }

    void BlinkText()
    {
        startText.SetActive(!startText.activeSelf);
    }
}