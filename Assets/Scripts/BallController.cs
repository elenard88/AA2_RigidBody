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
            Debug.Log("ANIMATING");
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
        float speed = velocity.magnitude;

        speed -= acceleration.magnitude * dt;

        if (speed < 0)
        {
            speed = 0;
        }

        velocity = velocity.normalized * speed;

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
        GameObject[] walls = GameObject.FindGameObjectsWithTag("Wall");

        foreach (GameObject wall in walls)
        {
            BoxCollider box = wall.GetComponent<BoxCollider>();

            Bounds bounds =  box.bounds;

            // punto mas cercano aabb
            Vector3 closestPoint;

            float sphereX = transform.position.x;

            if (sphereX < bounds.min.x)
            {
                closestPoint.x = bounds.min.x;
            }
            else if (sphereX > bounds.max.x)
            {
                closestPoint.x = bounds.max.x;
            }
            else
            {
                closestPoint.x = sphereX;
            }

            float sphereY = transform.position.y;

            if (sphereY < bounds.min.y)
            {
                closestPoint.y = bounds.min.y;
            }
            else if (sphereY > bounds.max.y)
            {
                closestPoint.y = bounds.max.y;
            }
            else
            {
                closestPoint.y = sphereY;
            }

            float sphereZ = transform.position.z;

            if (sphereZ < bounds.min.z)
            {
                closestPoint.z = bounds.min.z;
            }
            else if (sphereZ > bounds.max.z)
            {
                closestPoint.z = bounds.max.z;
            }
            else
            {
                closestPoint.z = sphereZ;
            }

            // punto mas cercano bola
            Vector3 difference = transform.position - closestPoint;
            float distance = difference.magnitude;

            // colision
            if (distance < radius)
            {
                Vector3 normal = difference.normalized;

                HandleWallCollision(normal);

                // rebote
                transform.position = closestPoint + normal * radius;

                break;
            }
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

            float lineLength = (dragStartPosition - currentMousePosition).magnitude;

            if (lineLength > 5f)
            {
                lineLength = 5f;
            }

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
    { // distancia hacia el agujero
      float distance = Vector3.Distance(transform.position, hole.position); 

        // Bola en agujero
        if (distance <= holeRadius) { 
            
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
        Vector3 direction = targetPosition - transform.position; direction.y = 0; float speed = 5f; 
        transform.position += direction.normalized * speed * Time.deltaTime; 
        //desaparece la bola
        transform.position += Vector3.down * 1.5f * Time.deltaTime; 
        if (transform.position.y <= -2f) 
        { 
            gameObject.SetActive(false); 
            Debug.Log("LEVEL COMPLETE"); 
        } 
    }
}





