using UnityEngine;
public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(50, 56, -50); 
    public float positionSmoothSpeed = 5f;
    public float rotationSmoothSpeed = 5f;
    public bool followTargetRotation = false;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = followTargetRotation
            ? target.position + target.TransformDirection(offset)
            : target.position + offset;

        float t = 1f - Mathf.Pow(0.001f, Time.deltaTime * positionSmoothSpeed);
        transform.position = Vector3.Lerp(transform.position, desiredPosition, t);

        // Mira directamente a la pelota, sin desvío
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}