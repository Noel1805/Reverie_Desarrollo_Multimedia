using UnityEngine;

// SCRIPT 3: Colocar este script en el JUGADOR
public class AtaqueBaculo : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform puntoLanzamiento; // Punto desde donde sale el VFX (punta del báculo)
    [SerializeField] private GameObject vfxAtaque1; // VFX para ataque 1
    [SerializeField] private GameObject vfxAtaque2; // VFX para ataque 2
    [SerializeField] private GameObject vfxAtaque3; // VFX para ataque 3

    [Header("Configuración de Ataques")]
    [SerializeField] private string nombreAtaque1 = "ataque1";
    [SerializeField] private string nombreAtaque2 = "ataque2";
    [SerializeField] private string nombreAtaque3 = "ataque3";

    [Header("Teclas de Ataque")]
    [SerializeField] private KeyCode teclaAtaque1 = KeyCode.Alpha1;
    [SerializeField] private KeyCode teclaAtaque2 = KeyCode.Alpha2;
    [SerializeField] private KeyCode teclaAtaque3 = KeyCode.Alpha3;

    [Header("Ajustes del Proyectil")]
    [SerializeField] private Vector3 escalaProyectil = Vector3.one; // Tamaño del proyectil
    [SerializeField] private Vector3 rotacionProyectil = Vector3.zero; // Rotación adicional del proyectil

    [Header("Configuración por Ataque")]
    [SerializeField] private float velocidadAtaque1 = 20f;
    [SerializeField] private float dañoAtaque1 = 25f;

    [SerializeField] private float velocidadAtaque2 = 20f;
    [SerializeField] private float dañoAtaque2 = 30f;

    [SerializeField] private float velocidadAtaque3 = 20f;
    [SerializeField] private float dañoAtaque3 = 35f;

    [Header("General")]
    [SerializeField] private float cooldownAtaque = 1f;

    [Header("Timing")]
    [SerializeField] private float tiempoLanzamiento = 0.5f;

    private bool puedeAtacar = true;
    private bool atacando = false;
    private EquipadorBaculo equipador;
    private int ataqueActualIndex = 0; // Para saber qué ataque se está ejecutando

    void Start()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        equipador = GetComponent<EquipadorBaculo>();
    }

    void Update()
    {
        // Verificar que tenga el báculo equipado
        if (equipador != null && !equipador.TieneBaculoEquipado())
            return;

        if (!puedeAtacar || atacando)
            return;

        // Detectar teclas de ataque
        if (Input.GetKeyDown(teclaAtaque1))
        {
            IniciarAtaque(nombreAtaque1, 1);
        }
        else if (Input.GetKeyDown(teclaAtaque2))
        {
            IniciarAtaque(nombreAtaque2, 2);
        }
        else if (Input.GetKeyDown(teclaAtaque3))
        {
            IniciarAtaque(nombreAtaque3, 3);
        }
    }

    void IniciarAtaque(string nombreAnimacion, int numeroAtaque)
    {
        atacando = true;
        puedeAtacar = false;
        ataqueActualIndex = numeroAtaque;

        // Reproducir animación
        if (animator != null)
        {
            animator.SetTrigger(nombreAnimacion);
        }

        // Esperar el tiempo de la animación antes de lanzar el VFX
        Invoke(nameof(LanzarVFX), tiempoLanzamiento);

        // Reiniciar cooldown
        Invoke(nameof(ReiniciarCooldown), cooldownAtaque);
    }

    void LanzarVFX()
    {
        // Seleccionar el VFX y configuración según el ataque
        GameObject vfxPrefab = null;
        float velocidad = 0;
        float daño = 0;

        switch (ataqueActualIndex)
        {
            case 1:
                vfxPrefab = vfxAtaque1;
                velocidad = velocidadAtaque1;
                daño = dañoAtaque1;
                break;
            case 2:
                vfxPrefab = vfxAtaque2;
                velocidad = velocidadAtaque2;
                daño = dañoAtaque2;
                break;
            case 3:
                vfxPrefab = vfxAtaque3;
                velocidad = velocidadAtaque3;
                daño = dañoAtaque3;
                break;
        }

        if (vfxPrefab == null)
        {
            Debug.LogWarning("No se ha asignado el VFX para el ataque " + ataqueActualIndex);
            atacando = false;
            return;
        }

        // Determinar punto de lanzamiento
        Transform puntoOrigen = puntoLanzamiento != null ? puntoLanzamiento : transform;

        // Calcular rotación final (rotación del punto + rotación adicional)
        Quaternion rotacionFinal = puntoOrigen.rotation * Quaternion.Euler(rotacionProyectil);

        // Instanciar el VFX
        GameObject vfx = Instantiate(vfxPrefab, puntoOrigen.position, rotacionFinal);

        // Aplicar escala
        vfx.transform.localScale = escalaProyectil;

        // Añadir componente de proyectil si no lo tiene
        ProyectilVFX proyectil = vfx.GetComponent<ProyectilVFX>();
        if (proyectil == null)
        {
            proyectil = vfx.AddComponent<ProyectilVFX>();
        }

        // Configurar el proyectil
        proyectil.Inicializar(velocidad, daño, transform.forward);

        atacando = false;
    }

    void ReiniciarCooldown()
    {
        puedeAtacar = true;
    }
}

// COMPONENTE PARA EL VFX - Se añade automáticamente al VFX
public class ProyectilVFX : MonoBehaviour
{
    private float velocidad;
    private float daño;
    private Vector3 direccion;

    [SerializeField] private float tiempoVida = 5f;
    [SerializeField] private float radioCollider = 0.5f; // Radio del collider esférico

    public void Inicializar(float vel, float dmg, Vector3 dir)
    {
        velocidad = vel;
        daño = dmg;
        direccion = dir.normalized;

        // Añadir collider si no existe
        SphereCollider collider = GetComponent<SphereCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<SphereCollider>();
        }
        collider.isTrigger = true;
        collider.radius = radioCollider;

        // Añadir Rigidbody si no existe (necesario para triggers)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = true;
        rb.useGravity = false;

        Destroy(gameObject, tiempoVida);
    }

    void Update()
    {
        transform.position += direccion * velocidad * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // Ignorar al jugador
        if (other.CompareTag("Player"))
            return;

        // Buscar componente de vida en el enemigo
        VidaEnemigo enemigo = other.GetComponent<VidaEnemigo>();
        if (enemigo != null)
        {
            enemigo.RecibirDaño(daño);
        }

        // Destruir el proyectil al impactar con CUALQUIER cosa
        Destroy(gameObject);
    }
}

// SCRIPT SIMPLE PARA ENEMIGOS - Colocar en cada enemigo
public class VidaEnemigo : MonoBehaviour
{
    [SerializeField] private float vidaMaxima = 100f;
    private float vidaActual;

    void Start()
    {
        vidaActual = vidaMaxima;
    }

    public void RecibirDaño(float cantidad)
    {
        vidaActual -= cantidad;
        Debug.Log(gameObject.name + " recibió " + cantidad + " de daño. Vida restante: " + vidaActual);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        Debug.Log(gameObject.name + " ha muerto");
        Destroy(gameObject);
    }
}