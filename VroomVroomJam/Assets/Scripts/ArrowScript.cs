using UnityEngine;

public class ArrowScript : MonoBehaviour
{
    [Header("Config")]
    public Transform target;
    public float rotationSpeed = 5f;
    public Color closeColor, mediumColor, farColor;
    public float maxDistance = 50f;

    private Renderer arrowRenderer;

    void Start()
    {
        arrowRenderer = GetComponent<Renderer>();
    }

    void Update()
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        Quaternion smoothed = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        transform.rotation = smoothed;

        float distance = direction.magnitude;
        float t = Mathf.Clamp01(distance / maxDistance);

        Color color;
        if (t < 0.5f)
            color = Color.Lerp(closeColor, mediumColor, t * 2f);
        else
            color = Color.Lerp(mediumColor, farColor, (t - 0.5f) * 2f);

        arrowRenderer.material.color = color;
    }
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void Disable()
    {
        target = null;
        gameObject.SetActive(false);
    }

    public void Enable()
    {
        gameObject.SetActive(true);
    }
}