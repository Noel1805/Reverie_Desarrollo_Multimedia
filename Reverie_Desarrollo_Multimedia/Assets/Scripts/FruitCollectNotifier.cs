using UnityEngine;

public class FruitCollectNotifier : MonoBehaviour
{

    public void NotificarRecoleccion()
    {
        if (FruitCounterUI.Instance != null)
        {
            FruitCounterUI.Instance.AgregarFruta();
        }
        else
        {
            Debug.LogWarning("[FruitCollectNotifier] No se encontró FruitCounterUI en la escena.");
        }
    }
}