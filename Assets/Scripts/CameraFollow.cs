using UnityEngine;


public class CameraFollow : MonoBehaviour
{
    public Transform target;

    // Offset dsd la bola
    public Vector3 offset = new Vector3(0, 5, -7);

    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        // posicion
        Vector3 desiredPosition = target.position + offset;

        // interpolacion
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        transform.position = smoothedPosition;

        transform.LookAt(target.position);
    }
}

