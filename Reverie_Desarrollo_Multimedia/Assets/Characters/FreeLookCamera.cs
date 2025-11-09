using UnityEngine;

public class FreeLookCamera : MonoBehaviour
{
    public Transform target;        // Personaje a seguir
    public float distance = 6f;     // Distancia de la cámara al personaje
    public float height = 3f;       // Altura sobre el personaje
    public float rotationSpeed = 120f; // Velocidad de rotación
    public float smoothSpeed = 10f; // Suavizado de movimiento

    private float yaw = 0f;
    private float pitch = 15f; // Inclinación inicial de la cámara
    public float minPitch = -20f;
    public float maxPitch = 60f;

    void LateUpdate()
    {
        if (target == null) return;

        // Solo rotar si se mantiene presionado el botón izquierdo del mouse
        if (Input.GetMouseButton(0))
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }

        // Calcula la posición de la cámara en base a la rotación
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        Vector3 desiredPosition = target.position + Vector3.up * height + offset;

        // Movimiento suave
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

        // Que mire siempre al personaje
        transform.LookAt(target.position + Vector3.up * height * 0.8f);
    }
}