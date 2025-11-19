using UnityEngine;
using System.Collections;

// SCRIPT 3: Colocar este script en el JUGADOR
public class AtaqueBaculo : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform puntoLanzamiento; // Punto desde donde sale el VFX (punta del báculo)
    [SerializeField] private GameObject vfxAtaque1; // VFX para ataque 1
    [SerializeField] private GameObject vfxAtaque2; // VFX para ataque 2
    [SerializeField] private GameObject vfxAtaque3; // VFX para ataque 3 (AOE)

    [Header("Configuración de Ataques")]
    [SerializeField] private string nombreAtaque1 = "ataque1";
    [SerializeField] private string nombreAtaque2 = "ataque2";
    [SerializeField] private string nombreAtaque3 = "ataque3";

    [Header("Teclas de Ataque")]
    [SerializeField] private KeyCode teclaAtaque1 = KeyCode.Alpha1;
    [SerializeField] private KeyCode teclaAtaque2 = KeyCode.Alpha2;
    [SerializeField] private KeyCode teclaAtaque3 = KeyCode.Alpha3;

    [Header("Ajustes del Proyectil (Ataques 1 y 2)")]
    [SerializeField] private Vector3 escalaProyectil = Vector3.one;
    [SerializeField] private Vector3 rotacionProyectil = Vector3.zero;

    [Header("Configuración Ataque 1 (Proyectil)")]
    [SerializeField] private float velocidadAtaque1 = 20f;
    [SerializeField] private float dañoAtaque1 = 25f;

    [Header("Configuración Ataque 2 (Proyectil)")]
    [SerializeField] private float velocidadAtaque2 = 20f;
    [SerializeField] private float dañoAtaque2 = 30f;

    [Header("Configuración Ataque 3 (AOE)")]
    [SerializeField] private float distanciaAOE = 4f; // Distancia desde el jugador
    [SerializeField] private Vector3 offsetPosicionAOE = Vector3.zero; // Offset adicional (X, Y, Z)
    [SerializeField] private Vector3 rotacionAOE = new Vector3(-90, 0, 0); // Rotación del VFX (X, Y, Z)
    [SerializeField] private float radioAOE = 3f; // Radio del área de efecto
    [SerializeField] private float dañoAtaque3 = 50f;
    [SerializeField] private float duracionVFXAOE = 2f; // Cuánto dura el efecto visual
    [SerializeField] private Vector3 escalaAOE = new Vector3(3, 3, 3);

    [Header("General")]
    [SerializeField] private float cooldownAtaque = 1f;

    [Header("Timing")]
    [SerializeField] private float tiempoLanzamiento = 0.5f;

    private bool puedeAtacar = true;
    private bool atacando = false;
    private EquipadorBaculo equipador;
    private int ataqueActualIndex = 0;

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

        // Esperar el tiempo de la animación antes de lanzar el ataque
        Invoke(nameof(LanzarAtaque), tiempoLanzamiento);

        // Reiniciar cooldown
        Invoke(nameof(ReiniciarCooldown), cooldownAtaque);
    }

    void LanzarAtaque()
    {
        if (ataqueActualIndex == 3)
        {
            // Ataque 3 es AOE
            LanzarAOE();
        }
        else
        {
            // Ataques 1 y 2 son proyectiles
            LanzarVFX();
        }

        atacando = false;
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
        }

        if (vfxPrefab == null)
        {
            Debug.LogWarning("No se ha asignado el VFX para el ataque " + ataqueActualIndex);
            return;
        }

        // Determinar punto de lanzamiento
        Transform puntoOrigen = puntoLanzamiento != null ? puntoLanzamiento : transform;

        // Calcular rotación final
        Quaternion rotacionFinal = puntoOrigen.rotation * Quaternion.Euler(rotacionProyectil);

        // Instanciar el VFX
        GameObject vfx = Instantiate(vfxPrefab, puntoOrigen.position, rotacionFinal);
        vfx.transform.localScale = escalaProyectil;

        // Añadir componente de proyectil
        ProyectilVFX proyectil = vfx.GetComponent<ProyectilVFX>();
        if (proyectil == null)
        {
            proyectil = vfx.AddComponent<ProyectilVFX>();
        }

        proyectil.Inicializar(velocidad, daño, transform.forward);
    }

    void LanzarAOE()
    {
        if (vfxAtaque3 == null)
        {
            Debug.LogWarning("No se ha asignado el VFX para el ataque AOE");
            return;
        }

        // Calcular posición base (delante del jugador)
        Vector3 posicionBase = transform.position + transform.forward * distanciaAOE;

        // Aplicar offset adicional (relativo a la rotación del jugador)
        Vector3 offsetRotado = transform.TransformDirection(offsetPosicionAOE);
        Vector3 posicionFinal = posicionBase + offsetRotado;

        // Aplicar rotación configurada
        Quaternion rotacionFinal = Quaternion.Euler(rotacionAOE);

        // Instanciar el VFX del AOE
        GameObject vfxAOE = Instantiate(vfxAtaque3, posicionFinal, rotacionFinal);
        vfxAOE.transform.localScale = escalaAOE;

        Debug.Log($"[AOE] Instanciado en: {posicionFinal} | Rotación: {rotacionAOE} | Escala: {escalaAOE}");

        // Intentar reproducir Particle Systems
        ParticleSystem[] particulas = vfxAOE.GetComponentsInChildren<ParticleSystem>();
        if (particulas.Length > 0)
        {
            foreach (ParticleSystem ps in particulas)
            {
                ps.Play();
            }
        }
        else
        {
            // Si no hay Particle Systems, buscar Visual Effect Graph
#if UNITY_2019_3_OR_NEWER
            UnityEngine.VFX.VisualEffect[] vfxGraphs = vfxAOE.GetComponentsInChildren<UnityEngine.VFX.VisualEffect>();
            if (vfxGraphs.Length > 0)
            {
                foreach (var vfx in vfxGraphs)
                {
                    vfx.Play();
                }
            }
#endif
        }

        // Añadir componente AOE
        AtaqueAOE aoe = vfxAOE.GetComponent<AtaqueAOE>();
        if (aoe == null)
        {
            aoe = vfxAOE.AddComponent<AtaqueAOE>();
        }

        aoe.Inicializar(radioAOE, dañoAtaque3, duracionVFXAOE);
    }

    void ReiniciarCooldown()
    {
        puedeAtacar = true;
    }

    // Visualizar el área de AOE en el editor
    void OnDrawGizmosSelected()
    {
        // Calcular posición del AOE para visualización
        Vector3 posicionBase = transform.position + transform.forward * distanciaAOE;
        Vector3 offsetRotado = transform.TransformDirection(offsetPosicionAOE);
        Vector3 posicionFinal = posicionBase + offsetRotado;

        // Dibujar el área de AOE
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(posicionFinal, radioAOE);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(posicionFinal, radioAOE);

        // Línea desde el jugador hasta el centro del AOE
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, posicionFinal);

        // Ejes de rotación
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(posicionFinal, Quaternion.Euler(rotacionAOE), Vector3.one);
        Gizmos.matrix = rotationMatrix;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(Vector3.zero, Vector3.right * 2);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(Vector3.zero, Vector3.up * 2);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(Vector3.zero, Vector3.forward * 2);

        Gizmos.matrix = Matrix4x4.identity;
    }
}

// COMPONENTE PARA PROYECTILES (Ataques 1 y 2)
public class ProyectilVFX : MonoBehaviour
{
    private float velocidad;
    private float daño;
    private Vector3 direccion;

    [SerializeField] private float tiempoVida = 5f;
    [SerializeField] private float radioCollider = 0.5f;

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

        // Añadir Rigidbody si no existe
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
        if (other.CompareTag("Player"))
            return;

        VidaEnemigo enemigo = other.GetComponent<VidaEnemigo>();
        if (enemigo != null)
        {
            enemigo.RecibirDaño(daño);
        }

        Destroy(gameObject);
    }
}

// COMPONENTE PARA ATAQUE AOE (Ataque 3)
public class AtaqueAOE : MonoBehaviour
{
    private float radio;
    private float daño;
    private bool dañoAplicado = false;

    public void Inicializar(float rad, float dmg, float duracion)
    {
        radio = rad;
        daño = dmg;

        // Aplicar daño inmediatamente
        Invoke(nameof(AplicarDaño), 0.1f);

        // Destruir después de la duración
        Destroy(gameObject, duracion);
    }

    void AplicarDaño()
    {
        if (dañoAplicado)
            return;

        dañoAplicado = true;

        // Detectar todos los colliders en el radio
        Collider[] collidersEnArea = Physics.OverlapSphere(transform.position, radio);

        Debug.Log($"[AOE] Detectados {collidersEnArea.Length} objetos en el área");

        int enemigosGolpeados = 0;
        foreach (Collider col in collidersEnArea)
        {
            // Ignorar al jugador
            if (col.CompareTag("Player"))
                continue;

            // Buscar componente de vida
            VidaEnemigo enemigo = col.GetComponent<VidaEnemigo>();
            if (enemigo != null)
            {
                enemigo.RecibirDaño(daño);
                enemigosGolpeados++;
                Debug.Log($"[AOE] Golpeó a {col.gameObject.name} con {daño} de daño");
            }
        }

        Debug.Log($"[AOE] Total enemigos golpeados: {enemigosGolpeados}");
    }

    // Visualizar el área de efecto en Scene View
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, radio);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radio);
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