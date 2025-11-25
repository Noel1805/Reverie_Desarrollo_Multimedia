using UnityEngine;
using System.Collections;

public class GrizartController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform player;         

    [Header("Detección")]
    [SerializeField] private float radioDeteccion = 10f;

    [Header("Movimiento")]
    [SerializeField] private float velocidadCaminar = 2f;
    [SerializeField] private float velocidadRotacion = 5f;
    [SerializeField] private float distanciaMinima = 2f; 

    [Header("Ataque")]
    [SerializeField] private float rangoAtaque = 2.5f;
    [SerializeField] private float danoAtaque = 0.5f;   
    [SerializeField] private float cooldownAtaque = 2f;

    [Header("Timing Ataque")]
    [Tooltip("Tiempo (en segundos) desde que empieza la animación hasta el impacto del golpe")]
    [SerializeField] private float tiempoImpactoAtaque = 0.4f;

    [Header("Límites de Isla")]
    [SerializeField] private Vector3 centroIsla;        
    [SerializeField] private float radioIsla = 15f;     

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

        if (animator == null)
            animator = GetComponent<Animator>();

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        vidaEnemigo = GetComponent<VidaEnemigo>();


        if (player == null)
        {
            GameObject jugadorGO = GameObject.FindGameObjectWithTag("Player");
            if (jugadorGO != null)
            {
                player = jugadorGO.transform;
            }
        }


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


        if (centroIsla == Vector3.zero)
        {
            centroIsla = transform.position;
        }
    }

    void Update()
    {
        if (estaMuerto) return;


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


            if (distanciaAlJugador <= rangoAtaque)
            {
                DetenerseYAtacar();
            }

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

        Vector3 direccion = (player.position - transform.position).normalized;
        Vector3 nuevaPos = transform.position + direccion * velocidadCaminar * Time.deltaTime;


        if (Vector3.Distance(nuevaPos, centroIsla) > radioIsla)
        {
            Idle();
            return;
        }


        Vector3 dirRot = player.position - transform.position;
        dirRot.y = 0f;

        if (dirRot != Vector3.zero)
        {
            Quaternion rotObjetivo = Quaternion.LookRotation(dirRot);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, velocidadRotacion * Time.deltaTime);
        }


        characterController.Move(direccion * velocidadCaminar * Time.deltaTime);

        characterController.Move(Vector3.down * 9.81f * Time.deltaTime);


        animator.SetBool("IsWalking", true);
    }

    void DetenerseYAtacar()
    {
        Debug.Log("[Grizart] 🟥 Entró a DetenerseYAtacar()");

        animator.SetBool("IsWalking", false);


        Vector3 dirRot = player.position - transform.position;
        dirRot.y = 0f;
        if (dirRot != Vector3.zero)
        {
            Quaternion rotObjetivo = Quaternion.LookRotation(dirRot);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotObjetivo, velocidadRotacion * Time.deltaTime);
        }


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


        animator.SetTrigger("Attack");


        StartCoroutine(AplicarDanioConDelay());


        Invoke(nameof(ReiniciarAtaque), cooldownAtaque);
    }

    private System.Collections.IEnumerator AplicarDanioConDelay()
    {

        yield return new WaitForSeconds(tiempoImpactoAtaque);

        if (estaMuerto || vidaKaven == null || player == null)
            yield break;


        float distancia = Vector3.Distance(transform.position, player.position);
        Debug.Log("[Grizart] ⏱ Momento de impacto. Distancia: " + distancia);

        if (distancia <= rangoAtaque)
        {
            vidaKaven.RecibirDano(danoAtaque, true);  

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

        Debug.Log("[Grizart] 💀 Enemigo ha muerto - Iniciando animación");


        animator.SetBool("IsWalking", false);
        animator.ResetTrigger("Attack");


        animator.SetBool("IsDead", true);

        if (characterController != null)
            characterController.enabled = false;


        this.enabled = false;


        StartCoroutine(EsperarYDestruir());
    }

    private IEnumerator EsperarYDestruir()
    {
        Debug.Log("[Grizart] ⏱️ Esperando a que termine la animación de muerte...");


        yield return new WaitForSeconds(tiempoAntesDeDestruir);

        Debug.Log("[Grizart] ✅ Animación de muerte completada, iniciando fade out");


        yield return StartCoroutine(FadeOutAndDestroy());
    }

    private IEnumerator FadeOutAndDestroy()
    {
        Debug.Log("[Grizart] 🌫️ Iniciando fade out");


        Renderer[] renderers = GetComponentsInChildren<Renderer>();


        Material[][] materialesPorRenderer = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            materialesPorRenderer[i] = renderers[i].materials;
        }

        float tiempo = 0f;
        float duracion = 2f; 

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = tiempo / duracion;
            float alpha = Mathf.Lerp(1f, 0f, t);

            for (int i = 0; i < materialesPorRenderer.Length; i++)
            {
                foreach (Material mat in materialesPorRenderer[i])
                {
                    if (mat == null) continue;


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

        Debug.Log("[Grizart] 🗑️ Destruyendo enemigo");
        Destroy(gameObject);
    }




    void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);


        Gizmos.color = Color.blue;
        Vector3 centro = centroIsla == Vector3.zero ? transform.position : centroIsla;
        Gizmos.DrawWireSphere(centro, radioIsla);
    }
}
