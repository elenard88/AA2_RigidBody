using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    [Header("Physics")]

    public float mass = 1.0f;
    public float radius = 0.25f;
    public float friction = 0.4f;
    private float gravity = 9.81f;
    public float restitution = 0.8f;
    private Vector3 velocity;
    private float angularVelocity;

    private string currentSurface = "Grass";

    public float stepTime = 0.005f;

    [Header("Air")]
    public float airResistance = 0.5f;
    private bool isGrounded;
    private Vector3 groundNormal = Vector3.up;
    private float currentHeight;

    // Update is called once per frame
    void Update()
    {
        CheckGround();
        SimulatePhysics();
        CheckWallCollisions();
        DetectSurface();
    }

    void SimulatePhysics()
    {
        if (isGrounded)
        {
            SimulateGroundPhysics();
        }
        else
        {
            SimulateAirPhysics();
        }

        transform.position += velocity * stepTime;

        CorrectGroundPenetration();

        RotateBall();
    }

    void SimulateGroundPhysics()
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

        velocity += (netForce / mass) * stepTime;
    }

    void SimulateAirPhysics()
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

        velocity += ((gravityForce + dragForce) / mass) * stepTime;
    }

    void RotateBall()
    {
        // Rotación visual
        angularVelocity = velocity.magnitude / radius;
        if (velocity.magnitude > 0.01f)
        {
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, velocity.normalized);
            transform.Rotate(rotationAxis, angularVelocity * Mathf.Rad2Deg * stepTime, Space.World);
        }
    }

    void CheckGround()
    {
        RaycastHit hit;

        int layerMask = ~LayerMask.GetMask("Ball");

        if (Physics.Raycast(transform.position, Vector3.down, out hit, 10f, layerMask ))
        {
            currentHeight = transform.position.y;

            isGrounded = hit.distance <= radius + 0.05f;

            if (isGrounded)
            {
                groundNormal = hit.normal;
            }
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
            currentHeight = transform.position.y;
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

            // ignorar layer bola
            int layerMask = ~LayerMask.GetMask("Ball");

            if (Physics.Raycast(transform.position, Vector3.down, out hit, 5f, layerMask))
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


    void CheckObstacleTag(string tagName)
    {
        GameObject[] obstacles = GameObject.FindGameObjectsWithTag(tagName);

        foreach (GameObject obstacle in obstacles)
        {
            BoxCollider box = obstacle.GetComponent<BoxCollider>();

            Bounds bounds = box.bounds;

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

                float restitutionValue = 0.8f;

                switch (tagName)
                {
                    case "Metal":
                        restitutionValue = 0.95f;
                        break;

                    case "Foam":
                        restitutionValue = 0.1f;
                        break;

                    case "Wood":
                        restitutionValue = 0.5f;
                        break;

                    case "Bumper":
                        restitutionValue = 1.5f;
                        break;

                    case "Wall":
                        restitutionValue = 0.8f;
                        break;
                }
                HandleWallCollision(normal, restitutionValue);

                // rebote
                transform.position = closestPoint + normal * radius;

                break;
            }
        }
    }


     void CheckWallCollisions()
     {
        CheckObstacleTag("Wall");
        CheckObstacleTag("Metal");
        CheckObstacleTag("Foam");
        CheckObstacleTag("Wood");
        CheckObstacleTag("Bumper");
     }

    void HandleWallCollision(Vector3 normal, float restitutionValue)
    {
        velocity = Vector3.Reflect(velocity, normal);

        velocity *= restitutionValue;
    }

    public void ShootBall(Vector3 direction, float force)
    {
        velocity = direction.normalized * force / mass;
    }
    public Vector3 GetVelocity()
    {
        return velocity;
    }

    public void StopBall()
    {
        velocity = Vector3.zero;
    }

}
