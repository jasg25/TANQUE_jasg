using UnityEngine;

public class EnemigoMovil : MonoBehaviour
{
    public Transform puntoA; // Asigna un objeto vacío en el Inspector para el punto inicial
    public Transform puntoB; // Asigna un objeto vacío en el Inspector para el punto final
    public float velocidad = 2.0f; // Velocidad de movimiento

    private Transform objetivoActual; // Hacia dónde se está moviendo ahora

    void Start()
    {
        // Comprobaciones iniciales
        if (puntoA == null || puntoB == null)
        {
            Debug.LogError("Asigna los puntos A y B en el Inspector para " + gameObject.name);
            enabled = false; // Desactiva el script si faltan puntos
            return;
        }

        // Empieza moviéndose hacia el punto B
        objetivoActual = puntoB;
        // Coloca el enemigo en el punto A al inicio (opcional, puedes colocarlo manualmente)
        transform.position = puntoA.position;
    }

    void Update()
    {
        // Si no hay puntos asignados, no hacer nada
        if (puntoA == null || puntoB == null) return;

        // Calcula la dirección hacia el objetivo actual
        Vector3 direccion = (objetivoActual.position - transform.position).normalized;

        // Mueve el enemigo hacia el objetivo
        // Usamos MoveTowards para un movimiento a velocidad constante
        transform.position = Vector3.MoveTowards(transform.position, objetivoActual.position, velocidad * Time.deltaTime);

        // Comprueba si ha llegado (o casi llegado) al objetivo actual
        if (Vector3.Distance(transform.position, objetivoActual.position) < 0.1f)
        {
            // Si llegó al punto B, el nuevo objetivo es el punto A
            if (objetivoActual == puntoB)
            {
                objetivoActual = puntoA;
            }
            // Si llegó al punto A, el nuevo objetivo es el punto B
            else
            {
                objetivoActual = puntoB;
            }
        }
    }
}