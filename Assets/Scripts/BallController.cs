using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Shoot")]
    //Potencia de tiro
    public float power = 2f;

    // Inputs
    private Vector3 dragStartPosition;
    private Vector3 dragEndPosition;

    private bool isDragging;

    private LineRenderer lineRenderer;

    private PhysicsManager physicsManager;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        physicsManager = GetComponent<PhysicsManager>();

        lineRenderer.positionCount = 2;
        lineRenderer.enabled = false;
    }


     // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        // button pressed
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;

            dragStartPosition = GetMouseWorldPosition();

            lineRenderer.enabled = true;
        }

        if (isDragging)
        {
            Vector3 currentMousePosition = GetMouseWorldPosition();

            Vector3 direction = (dragStartPosition - currentMousePosition).normalized;

            float lineLength = (dragStartPosition - currentMousePosition).magnitude;

            if (lineLength > 5f)
            {
                lineLength = 5f;
            }

            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + direction * lineLength);
        }



        // button released
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            dragEndPosition = GetMouseWorldPosition();

            lineRenderer.enabled = false;

            Vector3 dragVector = dragStartPosition - dragEndPosition;

            dragVector.y = 0;

            // fuerza magnitud
            float force = dragVector.magnitude * power;

            physicsManager.ShootBall(dragVector, force);
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // altura del plano
        float planeHeight = transform.position.y;

        float t = (planeHeight - ray.origin.y) / ray.direction.y;

        Vector3 point = ray.origin + ray.direction * t;

        return point;
    }
}