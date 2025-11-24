using UnityEngine;

public class FruitCollectNotifier : MonoBehaviour
{
    // Este método lo llamarán Mango, Pera y Mora cuando se consuman
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