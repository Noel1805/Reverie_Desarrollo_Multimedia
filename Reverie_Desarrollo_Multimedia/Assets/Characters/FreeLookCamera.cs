using UnityEngine;

public class FreeLookCamera : MonoBehaviour
{
    public Transform target;    
    public float distance = 6f;  
    public float height = 3f;     
    public float rotationSpeed = 120f; 
    public float smoothSpeed = 10f; 

    private float yaw = 0f;
    private float pitch = 15f; 
    public float minPitch = -20f;
    public float maxPitch = 60f;

    void LateUpdate()
    {
        if (target == null) return;


        if (Input.GetMouseButton(0))
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }


        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        Vector3 desiredPosition = target.position + Vector3.up * height + offset;


        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);


        transform.LookAt(target.position + Vector3.up * height * 0.8f);
    }
}