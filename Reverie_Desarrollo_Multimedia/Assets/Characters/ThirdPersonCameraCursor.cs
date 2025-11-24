using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ThirdPersonCameraCursor : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Punto al que seguirá la cámara (usa tu CameraTarget del Player)")]
    public Transform target;

    [Header("Órbita")]
    public float distance = 0.3f;     // ultra cercano
    public float height = 1.1f;
    public float lookHeight = 1.4f;
    public float mouseSensitivityX = 120f;
    public float mouseSensitivityY = 90f;
    public float minPitch = -10f;
    public float maxPitch = 35f;

    [Header("Filtro de ruido")]
    public float mouseDeadZone = 0.01f;


    private float yaw;
    private float pitch;

    void Start()
    {
        if (target == null)
        {
            Debug.LogWarning("ThirdPersonCameraCursor: asigna un target (CameraTarget del Player).");
        }

        // Inicializar rotación desde la cámara actual
        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;

        // Bloquear y ocultar cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (target == null) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Zona muerta
        if (Mathf.Abs(mouseX) < mouseDeadZone) mouseX = 0f;
        if (Mathf.Abs(mouseY) < mouseDeadZone) mouseY = 0f;

        yaw += mouseX * mouseSensitivityX * Time.deltaTime;
        pitch -= mouseY * mouseSensitivityY * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion rot = Quaternion.Euler(pitch, yaw, 0f);

        Vector3 offset = rot * new Vector3(0f, 0f, -distance);
        offset += Vector3.up * height;

        transform.position = target.position + offset;

        Vector3 lookTarget = target.position + Vector3.up * lookHeight;
        transform.rotation = Quaternion.LookRotation(lookTarget - transform.position, Vector3.up);
    }
}
