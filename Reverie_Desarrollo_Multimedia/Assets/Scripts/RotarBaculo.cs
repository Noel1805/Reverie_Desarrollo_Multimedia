using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotarBaculo : MonoBehaviour
{
    [Header("Rotación")]
    [Tooltip("Velocidad de rotación en grados por segundo")]
    public float velocidadRotacion = 50f;

    [Tooltip("Eje de rotación (Y = vertical, X = horizontal, Z = profundidad)")]
    public Vector3 ejeRotacion = Vector3.up; // Vector3.up es el eje Y

    void Update()
    {
        // Rotar el objeto sobre su eje
        transform.Rotate(ejeRotacion * velocidadRotacion * Time.deltaTime);
    }
}
