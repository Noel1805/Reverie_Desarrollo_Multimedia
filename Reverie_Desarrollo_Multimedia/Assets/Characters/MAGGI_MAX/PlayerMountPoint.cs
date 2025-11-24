using UnityEngine;

public class PlayerMountPoint : MonoBehaviour
{
    [Header("Punto donde se sentará el jugador al montar")]
    public Transform mountSeat;

    [Header("Offset desde la posición del Player")]
    public Vector3 seatOffset = new Vector3(0f, 0.9f, 0.1f);

    private void Reset()
    {
        if (mountSeat == null)
        {
            GameObject seat = new GameObject("MountSeat");
            seat.transform.SetParent(transform);
            seat.transform.localPosition = seatOffset;
            seat.transform.localRotation = Quaternion.identity;
            mountSeat = seat.transform;
        }
    }

    private void OnValidate()
    {
        if (mountSeat != null)
        {
            mountSeat.localPosition = seatOffset;
        }
    }
}