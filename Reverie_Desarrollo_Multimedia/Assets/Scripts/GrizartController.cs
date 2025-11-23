using UnityEngine;
using System.Collections;

public class GrizartController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform player;          // Referencia a Kaven

    [Header("Detección")]
    [SerializeField] private float radioDeteccion = 10f;

    [Header("Movimiento")]
    [SerializeField] private float velocidadCaminar = 2f;
    [SerializeField] private float velocidadRotacion = 5f;
    [SerializeField] private float distanciaMinima = 2f; // Distancia para detenerse y atacar

    [Header("Ataque")]
    [SerializeField] private float rangoAtaque = 2.5f;
    [SerializeField] private float danoAtaque = 0.5f;   // Media vida
    [SerializeField] private float cooldownAtaque = 2f;

    [Header("Timing Ataque")]
    [Tooltip("Tiempo (en segundos) desde que empieza la animación hasta el impacto del golpe")]
    [SerializeField] private float tiempoImpactoAtaque = 0.4f;

    [Header("Límites de Isla")]
    [SerializeField] private Vector3 centroIsla;        // Centro de la isla
    [SerializeField] private float radioIsla = 15f;     // Radio máximo de la isla

    [Header("Muerte")]
    [SerializeField] private float tiempoAntesDeDestruir = 4f;


    private bool jugadorDetectado = false;
    private bool puedeAtacar = true;
    private float tiempoUltimoAtaque;
    private bool estaMuerto = false;

    private VidaEnemigo vidaEnemigo;
    private VidaKaven vidaKaven;

    void Start()
    {
        // Referencias básicas
        if (animator == null)
            animator = GetComponent<Animator>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        vidaEnemigo = GetComponent<VidaEnemigo>();

        // 1) Si no hay Player asignado en el Inspector, lo busco por Tag
        if (player == null)
        {
            GameObject jugadorGO = GameObject.FindGameObjectWithTag("Player");
            if (jugadorGO != null)
            {
                player = jugadorGO.transform;
            }
        }

        // 2) Si ya tengo player (arrastrado o encontrado), busco VidaKaven SIEMPRE
        if (player != null)
        {
            vidaKaven = player.GetComponent<VidaKaven>();

            if (vidaKaven == null)
            {
                Debug.LogError($"[Grizart] ❌ El objeto '{player.name}' tiene Tag Player, pero NO tiene VidaKaven.");
            }
            else
            {
                Debug.Log("[Grizart] ✅ Referencia a VidaKaven correcta.");
            }
        }
        else
        {
            Debug.LogError("[Grizart] ❌ No encontré ningún GameObject con tag 'Player'.");
        }

        // Centro de la isla por defecto
        if (centroIsla == Vector3.zero)
        {
            centroIsla = transform.position;
        }
    }

    void Update()
    {
        if (estaMuerto) return;

        // Comprobar muerte del enemigo
        if (vidaEnemigo != null && vidaEnemigo.GetVidaActual() <= 0)
        {
            Morir();
            return;
        }

        if (player == null) return;

        DetectarJugador();

        if (jugadorDetectado)
        {
            float distanciaAlJugador = Vector3.Distance(transform.position, player.position);

            // Está en rango de ataque
            if (distanciaAlJugador <= rangoAtaque)
            {
                DetenerseYAtacar();
            }
            // Está en rango de detección pero aún lejos
            else if (distanciaAlJugador <= radioDeteccion)
            {
                PerseguirJugador();
            }
            else
            {
                Idle();
            }
        }
        else
        {
            Idle();
        }
    }

    void DetectarJugador()
    {
        float distancia = Vector3.Distance(transform.position, player.position);
        jugadorDetectado = distancia <= radioDeteccion;
    }

    void PerseguirJugador()
    {
        // Calcular dirección hacia el jugador
        Vector3 direccion = (player.position - transform.position).normalized;
        Vector3 nuevaPos = transform.position + direccion * velocidadCaminar * Time.deltaTime;

        // Limitar a la isla
        if (Vector3.Distance(nuevaPos, centroIsla) > radioIsla)
        {
            Idle();
            return;
        }

        // Rotar hacia el jugador
        Vector3 dirRot = player.position - transform.position;
        dirRot.y = 0f;

        if (dirRot != Vector3.zero)
        {
            Quaternion rotObjetivo = Quaternion.LookRotation(dirRot);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, velocidadRotacion * Time.deltaTime);
        }

        // Mover usando CharacterController
        characterController.Move(direccion * velocidadCaminar * Time.deltaTime);
        // Gravedad simple
        characterController.Move(Vector3.down * 9.81f * Time.deltaTime);

        // Activar animación de caminar
        animator.SetBool("IsWalking", true);
    }

    void DetenerseYAtacar()
    {
        Debug.Log("[Grizart] 🟥 Entró a DetenerseYAtacar()");

        animator.SetBool("IsWalking", false);

        // Mirar hacia el jugador
        Vector3 dirRot = player.position - transform.position;
        dirRot.y = 0f;
        if (dirRot != Vector3.zero)
        {
            Quaternion rotObjetivo = Quaternion.LookRotation(dirRot);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, velocidadRotacion * Time.deltaTime);
        }

        // Comprobar cooldown
        if (puedeAtacar && Time.time - tiempoUltimoAtaque >= cooldownAtaque)
        {
            Debug.Log("[Grizart] 🟨 Condición cumplida, llamando a Atacar()");
            Atacar();
        }
    }

    void Atacar()
    {
        puedeAtacar = false;
        tiempoUltimoAtaque = Time.time;

        Debug.Log("[Grizart] ⚔ Grizart intenta atacar");

        // Disparar animación
        animator.SetTrigger("Attack");

        // Aplicar daño después de un pequeño delay para que coincida con el golpe
        StartCoroutine(AplicarDanioConDelay());

        // Programar cuándo vuelve a poder atacar
        Invoke(nameof(ReiniciarAtaque), cooldownAtaque);
    }

    private System.Collections.IEnumerator AplicarDanioConDelay()
    {
        // Esperar hasta el momento del impacto
        yield return new WaitForSeconds(tiempoImpactoAtaque);

        if (estaMuerto || vidaKaven == null || player == null)
            yield break;

        // Comprobar que todavía esté en rango
        float distancia = Vector3.Distance(transform.position, player.position);
        Debug.Log("[Grizart] ⏱ Momento de impacto. Distancia: " + distancia);

        if (distancia <= rangoAtaque)
        {
            vidaKaven.RecibirDano(danoAtaque, true);  // true = ignora cooldown de daño

            Debug.Log($"[Grizart] ✅ Daño aplicado a Kaven (delay): {danoAtaque}");
        }
        else
        {
            Debug.Log("[Grizart] ❌ Kaven se salió del rango antes del impacto.");
        }
    }

    void ReiniciarAtaque()
    {
        puedeAtacar = true;
    }

    void Idle()
    {
        animator.SetBool("IsWalking", false);
    }

    void Morir()
    {
        if (estaMuerto) return;
        estaMuerto = true;

        Debug.Log("[Grizart] 💀 Enemigo ha muerto");

        // Limpia animaciones que puedan seguir molestando
        animator.SetBool("IsWalking", false);
        animator.ResetTrigger("Attack");
        animator.SetBool("IsDead", true);

        if (characterController != null)
            characterController.enabled = false;

        // Desactivamos la IA
        this.enabled = false;

        // Iniciar Fade Out
        StartCoroutine(FadeOutAndDestroy());
    }


    private IEnumerator FadeOutAndDestroy()
    {
        // Tomamos TODOS los renderers del enemigo (incluye SkinnedMeshRenderer)
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        // Cacheamos los materiales instanciados
        Material[][] materialesPorRenderer = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            // Clonamos los materiales para no afectar a otros enemigos
            materialesPorRenderer[i] = renderers[i].materials;
        }

        float tiempo = 0f;
        float duracion = 2f; // tiempo del fade en segundos

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;
            float alpha = Mathf.Lerp(1f, 0f, t);

            // Debug opcional para ver el alpha en consola
            // Debug.Log("Alpha enemigo: " + alpha);

            for (int i = 0; i < materialesPorRenderer.Length; i++)
            {
                foreach (Material mat in materialesPorRenderer[i])
                {
                    if (mat == null) continue;

                    // URP Lit usa _BaseColor, Standard usa _Color
                    if (mat.HasProperty("_BaseColor"))
                    {
                        Color c = mat.GetColor("_BaseColor");
                        c.a = alpha;
                        mat.SetColor("_BaseColor", c);
                    }
                    else if (mat.HasProperty("_Color"))
                    {
                        Color c = mat.color;
                        c.a = alpha;
                        mat.color = c;
                    }
                }
            }

            yield return null;
        }

        Destroy(gameObject);
    }



    // Gizmos en el editor
    void OnDrawGizmosSelected()
    {
        // Radio de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);

        // Radio de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);

        // Límite de la isla
        Gizmos.color = Color.blue;
        Vector3 centro = centroIsla == Vector3.zero ? transform.position : centroIsla;
        Gizmos.DrawWireSphere(centro, radioIsla);
    }
}
