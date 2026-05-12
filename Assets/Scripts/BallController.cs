using UnityEngine;

public class BallController : MonoBehaviour
{
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




    // Update is called once per frame
    void Update()
    {
        HandleInput();
        SimulatePhysics();
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
            Vector3 rotationAxis =
                Vector3.Cross(Vector3.up, velocity.normalized);

            transform.Rotate( rotationAxis, angularVelocity * Mathf.Rad2Deg * dt, Space.World);
        }
    }

    public void ShootBall(Vector3 direction, float force)
    {
        velocity = direction.normalized * force / mass;
    }

    void HandleInput()
    {
        // button pressed
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;

            dragStartPosition =
                GetMouseWorldPosition();
        }

        // button released
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            dragEndPosition =
                GetMouseWorldPosition();

            
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


}



