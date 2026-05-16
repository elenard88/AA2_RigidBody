using UnityEngine;

public class BallController : MonoBehaviour
{
    private Vector3 groundNormal = Vector3.up; 
    private bool isGrounded = false;
    private float currentHeight = 0f;

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
        DetectSurface();
        SimulatePhysics();
        CheckWallCollisions();
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

        if (isGrounded)
        {
            // Cancelar velocidad vertical al estar en suelo
            Vector3 verticalVelocity = Vector3.Dot(velocity, groundNormal) * groundNormal;
            if (Vector3.Dot(verticalVelocity, groundNormal) < 0)
            {
                velocity -= verticalVelocity; // eliminar componente que penetra el suelo
            }

            Vector3 gravityFull = Vector3.down * gravity * mass;
            Vector3 normalForce = groundNormal * Vector3.Dot(gravityFull, groundNormal);
            Vector3 gravityParallel = gravityFull - normalForce;

            float normalMagnitude = Vector3.Dot(gravityFull, groundNormal) * -1f;
            float frictionMagnitude = friction * normalMagnitude;

            Vector3 netForce;

            if (velocity.magnitude < 0.01f)
            {
                if (gravityParallel.magnitude <= frictionMagnitude)
                {
                    velocity = Vector3.zero;
                    return;
                }
                else
                {
                    netForce = gravityParallel;
                }
            }
            else
            {
                Vector3 frictionForce = -velocity.normalized * frictionMagnitude;
                netForce = gravityParallel + frictionForce;
            }

            velocity += (netForce / mass) * dt;
        }
        else
        {
            // EN EL AIRE
            Vector3 gravityForce = Vector3.down * gravity * mass;
            Vector3 dragForce = Vector3.zero;

            if (currentHeight > 1f)
            {
                float rho = 1.225f;
                float Cd = 0.47f;
                float A = Mathf.PI * radius * radius;
                float speed = velocity.magnitude;
                dragForce = -velocity.normalized * (0.5f * rho * speed * speed * Cd * A);
            }

            velocity += ((gravityForce + dragForce) / mass) * dt;
        }

        transform.position += velocity * dt;

        // Corrección de posición — evitar que la bola se hunda en el suelo
        CorrectGroundPenetration();

        // Rotación visual
        angularVelocity = velocity.magnitude / radius;
        if (velocity.magnitude > 0.01f)
        {
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, velocity.normalized);
            transform.Rotate(rotationAxis, angularVelocity * Mathf.Rad2Deg * dt, Space.World);
        }
    }

    void CorrectGroundPenetration()
    {
        RaycastHit hit;
        int layerMask = ~LayerMask.GetMask("Ball");

        // Solo corregir si estamos muy cerca del suelo
        if (Physics.Raycast(transform.position, Vector3.down, out hit, radius * 1.1f, layerMask))
        {
            float penetration = radius - hit.distance;
            if (penetration > 0)
            {
                transform.position += hit.normal * penetration;

                // Cancelar velocidad hacia el suelo al corregir
                if (Vector3.Dot(velocity, hit.normal) < 0)
                {
                    velocity -= Vector3.Dot(velocity, hit.normal) * hit.normal;
                }
            }
        }
    }

    void DetectSurface()
    {
        RaycastHit hit;
        int layerMask = ~LayerMask.GetMask("Ball");

        // Rango largo para detectar suelo lejano
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, layerMask))
        {
            currentHeight = transform.position.y;

            // Solo isGrounded si estamos físicamente tocando el suelo
            isGrounded = hit.distance <= radius + 0.05f;

            if (isGrounded)
            {
                groundNormal = hit.normal;
            }

            currentSurface = hit.collider.tag;
            switch (currentSurface)
            {
                case "Grass": friction = 0.4f; break;
                case "Ice": friction = 0.1f; break;
                case "Sand": friction = 0.6f; break;
            }
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
            currentHeight = transform.position.y;
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