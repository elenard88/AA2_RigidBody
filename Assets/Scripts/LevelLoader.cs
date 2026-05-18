using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{

    [Header("Hole")]
    public Transform hole;
    public float holeRadius = 0.5f;
    public float maxGoalSpeed = 0.5f;

    [Header("Lose Conditions")]
    public int maxBorderContact = 2;
    public int currentBorderContacts = 0;
    public float minDistanceToRespawn = -5f;

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
        if(transform.position.y < minDistanceToRespawn && !levelComplete)
        {
            StartCoroutine(ReloadLevel(0f));
            return;
        }


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

        // bola en agujero
        if (distance <= holeRadius)
        {

            // velocidad de entrada
            if (physicsManager.GetVelocity().magnitude <= maxGoalSpeed)
            {
                levelComplete = true;
                physicsManager.StopBall();
                physicsManager.DisablePhysics();

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
            int next = SceneManager.GetActiveScene().buildIndex + 1;
            if (next >= SceneManager.sceneCountInBuildSettings)
                SceneManager.LoadScene(0);
            else
                SceneManager.LoadScene(next);
            
        }
    }

    public void RegisterBorderContact()
    {
        currentBorderContacts++;
        Debug.Log($"Rebotes: {currentBorderContacts}/{maxBorderContact}");
        if (currentBorderContacts > maxBorderContact)
            StartCoroutine(ReloadLevel(1.5f));
    }

    IEnumerator ReloadLevel(float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
