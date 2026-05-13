using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Collision")]
    public float restitution = 0.8f;
    private bool isColliding;

    [Header("Hole")]
    public Transform hole;
    public float holeRadius = 0.5f;
    public float maxGoalSpeed = 0.5f;


    public float mass = 1.0f;
    public float radius = 0.25f;
    public float friction = 0.4f;
    private float gravity = 9.81f;

    private Vector3 velocity;
    private float angularVelocity;

    // Inputs
    private Vector3 dragStartPosition;
    private Vector3 dragEndPosition;

    private bool isDragging;

    //Potencia de tiro
    public float power = 2f;

    private LineRenderer lineRenderer;

    private string currentSurface = "Grass";

    private bool levelComplete;


    // Update is called once per frame
    void Update()
    {

        if(levelComplete)
        {
            AnimateHoleEntry();
            return;
        }

        HandleInput();
        SimulatePhysics();
        CheckWallCollisions();
        DetectSurface();
        CheckHole();
    }

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.enabled = false;
    }

    void SimulatePhysics()
    {
        float dt = Time.deltaTime;

        // parar movimiento
        if (velocity.magnitude < 0.01f)
        {
            velocity = Vector3.zero;
            return;
        }

        // fuerza friccion
        float frictionMagnitude = friction * mass * gravity;

        Vector3 frictionForce = -velocity.normalized * frictionMagnitude;

        // segunda ley newton
        Vector3 acceleration = frictionForce / mass;

        // actualizar velocity
        velocity += acceleration * dt;

        // la bola no se puede mover hacia atras
        if (Vector3.Dot(velocity, velocity + acceleration * dt) < 0)
        {
            velocity = Vector3.zero;
        }

        // actualizar position
        transform.position += velocity * dt;

        angularVelocity = velocity.magnitude / radius;

        // rotacion
        if (velocity.magnitude > 0.01f)
        {
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, velocity.normalized);

            transform.Rotate( rotationAxis, angularVelocity * Mathf.Rad2Deg * dt, Space.World);
        }
    }

    void DetectSurface()
    {
        RaycastHit hit;

        // Ignore Ball layer
        int layerMask = ~LayerMask.GetMask("Ball");

        if (Physics.Raycast( transform.position, Vector3.down, out hit, 5f, layerMask))
        {
            Debug.Log(hit.collider.name);

            currentSurface = hit.collider.tag;

            switch (currentSurface)
            {
                case "Grass":
                    friction = 0.4f;
                    break;

                case "Ice":
                    friction = 0.1f;
                    break;

                case "Sand":
                    friction = 0.6f;
                    break;
            }
        }
    }

    void CheckWallCollisions()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius + 0.05f);

        bool touchingWall = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Wall"))
            {
                touchingWall = true;

                if (!isColliding)
                {
                    // punto mas cercano pared
                    Vector3 closestPoint = hit.ClosestPoint(transform.position);

                    // colision normal
                    Vector3 normal = (transform.position - closestPoint).normalized;

                    HandleWallCollision(normal);

                    // rebote
                    transform.position = closestPoint + normal * (radius + 0.01f);

                    isColliding = true;
                }

                break;
            }
        }

        // reseteo colision
        if (!touchingWall)
        {
            isColliding = false;
        }
    }


    void HandleWallCollision(Vector3 normal)
    {
        velocity = Vector3.Reflect(velocity, normal);

        velocity *= restitution;
    }

    public void ShootBall(Vector3 direction, float force)
    {
        velocity = direction.normalized * force / mass;
    }

    void HandleInput()
    {

        if (levelComplete)
        {
            return;
        }

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

            float lineLength = Mathf.Clamp((dragStartPosition - currentMousePosition).magnitude, 0f, 5f);

            lineRenderer.SetPosition( 0, transform.position);
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

            ShootBall(dragVector, force);
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

    void CheckHole()
    {
        // distancia hacia el agujero
        float distance = Vector3.Distance(transform.position, hole.position);

        // Bola en agujero
        if (distance <= holeRadius)
        {
            // velocidad de entrada
            if (velocity.magnitude <= maxGoalSpeed)
            {
                levelComplete = true;

                velocity = Vector3.zero;
            }
        }
    }

    void AnimateHoleEntry()
    {
        Vector3 targetPosition = new Vector3(hole.position.x, transform.position.y, hole.position.z);

        transform.position = Vector3.Lerp( transform.position, targetPosition, 5f * Time.deltaTime);

        transform.position += Vector3.down * 1.5f * Time.deltaTime;

         if (transform.position.y <= -2f)
    {
        gameObject.SetActive(false);

        Debug.Log("LEVEL COMPLETE");
    }
    }

}



