using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotarBaculo : MonoBehaviour
{
    [Header("Rotación")]
    [Tooltip("Velocidad de rotación en grados por segundo")]
    public float velocidadRotacion = 50f;

    [Tooltip("Eje de rotación (Y = vertical, X = horizontal, Z = profundidad)")]
    public Vector3 ejeRotacion = Vector3.up; 

    void Update()
    {

        transform.Rotate(ejeRotacion * velocidadRotacion * Time.deltaTime);
    }
}
