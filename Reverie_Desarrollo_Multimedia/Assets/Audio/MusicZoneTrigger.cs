using UnityEngine;

public class MusicZoneTrigger : MonoBehaviour
{
    public enum MusicType
    {
        SpringExploration,
        AutumnExploration,
        WinterExploration,
        Battle,
        FinalCabin
    }

    [Header("Configuración de Zona")]
    [SerializeField] private MusicType musicType;
    [SerializeField] private bool changeOnEnter = true;


    private void Start()
    {
        Debug.Log($"[MusicZone START] Zona inicializada: {gameObject.name} - Tipo: {musicType}");

        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"[MusicZone ERROR] {gameObject.name} NO tiene Collider!");
        }
        else if (!col.isTrigger)
        {
            Debug.LogError($"[MusicZone ERROR] {gameObject.name} tiene Collider pero NO es Trigger!");
        }
        else
        {
            Debug.Log($"[MusicZone OK] {gameObject.name} configurado correctamente como Trigger");
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"<color=yellow>[MusicZone TRIGGER] {gameObject.name} detectó: {other.gameObject.name} (Tag: {other.tag})</color>");

        if (!changeOnEnter)
        {
            Debug.Log($"[MusicZone] changeOnEnter está desactivado en {gameObject.name}");
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log($"<color=green>[MusicZone SUCCESS] ¡Jugador detectado en {gameObject.name}! Cambiando a: {musicType}</color>");
            ChangeMusicBasedOnType();
        }
        else
        {
            Debug.Log($"<color=orange>[MusicZone] {other.gameObject.name} no es el jugador (tag incorrecto)</color>");
        }
    }


    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"<color=cyan>[MusicZone EXIT] {other.gameObject.name} salió de {gameObject.name}</color>");
    }

    private void ChangeMusicBasedOnType()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("[MusicZone] AudioManager.Instance es null!");
            return;
        }

        switch (musicType)
        {
            case MusicType.SpringExploration:
                Debug.Log("[AudioManager] Cambiando a Spring");
                AudioManager.Instance.PlaySpringExploration();
                break;
            case MusicType.AutumnExploration:
                Debug.Log("[AudioManager] Cambiando a Autumn");
                AudioManager.Instance.PlayAutumnExploration();
                break;
            case MusicType.WinterExploration:
                Debug.Log("[AudioManager] Cambiando a Winter");
                AudioManager.Instance.PlayWinterExploration();
                break;
            case MusicType.Battle:
                Debug.Log("[AudioManager] Cambiando a Battle");
                AudioManager.Instance.PlayBattleMusic();
                break;
            case MusicType.FinalCabin:
                Debug.Log("[AudioManager] Cambiando a Final Cabin");
                AudioManager.Instance.PlayFinalCabinMusic();
                break;
        }
    }

    private void OnDrawGizmos()
    {

        if (Application.isPlaying) return;

        Collider col = GetComponent<Collider>();
        if (col != null && col.enabled)
        {
            Color gizmoColor;
            switch (musicType)
            {
                case MusicType.SpringExploration:
                    gizmoColor = new Color(0, 1, 0, 0.3f);
                    break;
                case MusicType.AutumnExploration:
                    gizmoColor = new Color(1, 0.5f, 0, 0.3f);
                    break;
                case MusicType.WinterExploration:
                    gizmoColor = new Color(0, 0.5f, 1, 0.3f);
                    break;
                case MusicType.Battle:
                    gizmoColor = new Color(1, 0, 0, 0.3f);
                    break;
                case MusicType.FinalCabin:
                    gizmoColor = new Color(1, 1, 0, 0.3f);
                    break;
                default:
                    gizmoColor = new Color(1, 1, 1, 0.3f);
                    break;
            }

            Gizmos.color = gizmoColor;

            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.center, box.size);
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.DrawSphere(transform.position + sphere.center, sphere.radius);
                Gizmos.DrawWireSphere(transform.position + sphere.center, sphere.radius);
            }
        }
    }


    public void TriggerMusicChange()
    {
        ChangeMusicBasedOnType();
    }
}