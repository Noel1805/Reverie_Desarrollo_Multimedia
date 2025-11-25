using UnityEngine;


public class PlayerCheckpointSystem : MonoBehaviour
{
    [Header("Configuración de Respawn")]
    [SerializeField] private float limiteY = -10f; 
    [SerializeField] private float alturaRespawn = 2f; 
    [SerializeField] private float tiempoInvulnerabilidad = 1f; 

    private Vector3 ultimoCheckpoint;
    private bool checkpointActivo = false;
    private float tiempoUltimoRespawn;
    private CharacterController characterController;
    private bool estaRespawneando = false; 


    private Transform playerModel;
    private Quaternion modelRotacionOriginal;

    void Start()
    {

        characterController = GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("¡El jugador necesita un Character Controller!");
        }


        Animator animator = GetComponentInChildren<Animator>();
        if (animator != null)
        {
            playerModel = animator.transform;
            modelRotacionOriginal = playerModel.localRotation;
            Debug.Log($"✅ Modelo del jugador encontrado: {playerModel.name}");
        }
        else if (transform.childCount > 0)
        {
            playerModel = transform.GetChild(0);
            modelRotacionOriginal = playerModel.localRotation;
            Debug.Log($"✅ Modelo del jugador encontrado (primer hijo): {playerModel.name}");
        }


        GuardarCheckpoint(transform.position);
        tiempoUltimoRespawn = -tiempoInvulnerabilidad;
    }

    void Update()
    {

        if (estaRespawneando) return;


        if (transform.position.y < limiteY && checkpointActivo)
        {

            if (Time.time - tiempoUltimoRespawn > tiempoInvulnerabilidad)
            {
                Respawnear();
            }
        }
    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {

        IslaCheckpoint isla = hit.gameObject.GetComponent<IslaCheckpoint>();
        if (isla != null)
        {

            if (hit.normal.y > 0.5f) 
            {
                isla.RegistrarJugadorEnIsla(hit.point);
            }
        }
    }


    public void GuardarCheckpoint(Vector3 nuevaPosicion)
    {
        ultimoCheckpoint = nuevaPosicion + Vector3.up * alturaRespawn;
        checkpointActivo = true;
        Debug.Log($"✓✓✓ Checkpoint guardado en: {ultimoCheckpoint} ✓✓✓");
    }


    private void Respawnear()
    {

        estaRespawneando = true;

        Debug.Log("🔄 Iniciando respawn...");


        bool estabaMontado = transform.parent != null;
        MountableChicken_New maggiScript = null;

        if (estabaMontado)
        {

            Transform current = transform.parent;
            while (current != null && maggiScript == null)
            {
                maggiScript = current.GetComponent<MountableChicken_New>();
                current = current.parent;
            }

            if (maggiScript != null)
            {
                Debug.Log("🐔 Jugador estaba montado en Maggi - Respawneando a ambos");

                maggiScript.OnPlayerRespawn();
            }
        }


        if (characterController != null)
        {
            characterController.enabled = false;
        }


        transform.position = ultimoCheckpoint;
        transform.rotation = Quaternion.identity; 


        if (playerModel != null)
        {
            playerModel.localRotation = modelRotacionOriginal;
            Debug.Log("✅ Rotación del modelo restaurada");
        }


        if (characterController != null)
        {
            characterController.enabled = true;
        }

        tiempoUltimoRespawn = Time.time;


        VidaKaven vidaKaven = GetComponent<VidaKaven>();
        if (vidaKaven != null && vidaKaven.EstaVivo())
        {
            vidaKaven.RecibirDano(2f, true); 
            Debug.Log("💔 Se quitó 1 corazón por caída");
        }

        Debug.Log($"✅ Jugador respawneado en: {ultimoCheckpoint}");


        Invoke(nameof(DesactivarBanderaRespawn), 0.2f);
    }

    private void DesactivarBanderaRespawn()
    {
        estaRespawneando = false;
        Debug.Log("✓ Respawn completado - Sistema listo");
    }


    public void ForzarRespawn()
    {
        if (checkpointActivo && !estaRespawneando)
        {
            Respawnear();
        }
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            new Vector3(-1000, limiteY, 0),
            new Vector3(1000, limiteY, 0)
        );

        if (checkpointActivo)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(ultimoCheckpoint, 0.5f);
        }
    }
}