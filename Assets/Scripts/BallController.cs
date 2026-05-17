using UnityEngine;

public class BallController : MonoBehaviour
{
    private Vector3 groundNormal = Vector3.up; 
    private bool isGrounded = false;
    private float currentHeight = 0f;

    [Header("Collision")]
    
    private bool isColliding;

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
        lineRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
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
    
    void HandleInput()
    {
        Debug.Log(lineRenderer);
        Debug.Log(physicsManager);
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