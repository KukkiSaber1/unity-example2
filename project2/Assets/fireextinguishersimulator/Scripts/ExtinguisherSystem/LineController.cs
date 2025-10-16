using UnityEngine;
public class LineController : MonoBehaviour
{
    public Transform extinguisherPoint;
    public Transform nozzlePoint;
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        if (lineRenderer != null && extinguisherPoint != null && nozzlePoint != null)
        {
            lineRenderer.SetPosition(0, extinguisherPoint.position);
            lineRenderer.SetPosition(1, nozzlePoint.position);
        }
    }
}
