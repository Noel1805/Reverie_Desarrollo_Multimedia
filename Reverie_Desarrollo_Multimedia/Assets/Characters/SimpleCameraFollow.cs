using UnityEngine;


public class CameraFollowPlayer : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player; 

    [Header("Seguimiento")]
    public float followSpeed = 10f;
    public Vector3 offset = new Vector3(0, 2, 0); 

    [Header("Rotación Automática")]
    public bool autoRotate = true;
    public float rotationSpeed = 3f;

    private Vector3 lastPlayerPosition;
    private float idleTime = 0f;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (player != null)
        {
            lastPlayerPosition = player.position;
        }
    }

    void LateUpdate()
    {
        if (player == null) return;


        Vector3 targetPosition = player.position + offset;
        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );


        if (autoRotate)
        {
            Vector3 playerMovement = player.position - lastPlayerPosition;
            playerMovement.y = 0; 


            if (playerMovement.magnitude > 0.01f)
            {
                idleTime = 0f;


                Vector3 direction = playerMovement.normalized;


                Quaternion targetRotation = Quaternion.LookRotation(direction);


                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
            else
            {

                idleTime += Time.deltaTime;

                if (idleTime > 0.3f) 
                {
                    Vector3 playerForward = player.forward;
                    playerForward.y = 0;

                    if (playerForward.magnitude > 0.1f)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(playerForward);
                        transform.rotation = Quaternion.Slerp(
                            transform.rotation,
                            targetRotation,
                            rotationSpeed * 0.5f * Time.deltaTime
                        );
                    }
                }
            }

            lastPlayerPosition = player.position;
        }
    }
}