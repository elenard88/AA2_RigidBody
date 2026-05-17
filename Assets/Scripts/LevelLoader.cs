using UnityEngine;

public class LevelLoader : MonoBehaviour
{

    [Header("Hole")]
    public Transform hole;
    public float holeRadius = 0.5f;
    public float maxGoalSpeed = 0.5f;

    private bool levelComplete;

    private PhysicsManager physicsManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        physicsManager = GetComponent<PhysicsManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (levelComplete)
        {
            Debug.Log("ANIMATING");
            AnimateHoleEntry();
            return;
        }

        CheckHole();
    }

    void CheckHole()
    { // distancia hacia el agujero
        float distance = Vector3.Distance(transform.position, hole.position);

        // Bola en agujero
        if (distance <= holeRadius)
        {

            // velocidad de entrada
            if (physicsManager.GetVelocity().magnitude <= maxGoalSpeed)
            {
                levelComplete = true;
                physicsManager.StopBall();
                
                Debug.Log("Level Complete");
            }
        }
    }

    void AnimateHoleEntry()
    {
        Vector3 targetPosition = new Vector3(hole.position.x, transform.position.y, hole.position.z);
        Vector3 direction = targetPosition - transform.position; 
        direction.y = 0;
        float distance = direction.magnitude;
        float speed = 5f;

        if (distance > 0.05f)
        {
            direction.Normalize();
            transform.position += direction.normalized * speed * Time.deltaTime;
        }

        //desaparece la bola
        transform.position += Vector3.down * 1.5f * Time.deltaTime;
       
        if (transform.position.y <= -2f)
        {
            gameObject.SetActive(false);
            
        }
    }
}
