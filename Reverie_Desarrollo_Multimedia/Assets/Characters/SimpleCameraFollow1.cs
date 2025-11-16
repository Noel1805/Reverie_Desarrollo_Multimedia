using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SimpleFollowCamera : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Transform que la cámara seguirá (puede ser el Player o CameraTarget).")]
    public Transform target;

    [Header("Offset")]
    [Tooltip("Offset relativo al target. Z negativo = detrás del personaje.")]
    public Vector3 offset = new Vector3(0f, 1.7f, -2f);

    [Header("Suavizado")]
    public float followSmooth = 10f;

    void LateUpdate()
    {
        if (target == null) return;

        // Posición deseada: target + offset en el espacio del target
        Vector3 desiredPosition = target.position + target.rotation * offset;

        // Mover la cámara suavemente hacia esa posición
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSmooth * Time.deltaTime);

        // Mirar al target un poco más arriba (pecho/cara)
        Vector3 lookPoint = target.position + Vector3.up * 1.5f;
        transform.LookAt(lookPoint);
    }
}
