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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        SimulatePhysics();
        CheckWallCollisions();
        DetectSurface();
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

            transform.Rotate(rotationAxis, angularVelocity * Mathf.Rad2Deg * dt, Space.World);
        }
    }

    void DetectSurface()
    {
        RaycastHit hit;

        // Ignore Ball layer
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
                        restitutionValue = 0.9f;
                        break;

                    case "Foam":
                        restitutionValue = 0.2f;
                        break;

                    case "Wood":
                        restitutionValue = 0.5f;
                        break;

                    case "Bumper":
                        restitutionValue = 1.2f;
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

    void HandleWallCollision(Vector3 normal, float restituitionValue)
    {
        velocity = Vector3.Reflect(velocity, normal);

        velocity *= restitution;
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
