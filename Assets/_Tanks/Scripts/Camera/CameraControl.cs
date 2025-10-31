using UnityEngine;
using System.Collections.Generic; // Para usar List

// Adaptado para Unity 6
public class CameraControl : MonoBehaviour
{
    public float m_DampTime = 0.2f; // Tiempo para suavizar el movimiento y zoom
    public float m_ScreenEdgeBuffer = 4f; // Espacio extra alrededor de los tanques
    public float m_MinSize = 6.5f; // Tamaño ortográfico mínimo (zoom máximo)
    [HideInInspector] public Transform[] m_Targets; // Array de objetivos a seguir (asignado por GameManager)

    private Camera m_Camera; // Referencia a la cámara hija
    private float m_ZoomSpeed; // Usado por SmoothDamp para el zoom
    private Vector3 m_MoveVelocity; // Usado por SmoothDamp para el movimiento
    private Vector3 m_DesiredPosition; // Posición a la que la cámara quiere ir

    private List<Transform> m_ActiveTargets = new List<Transform>(); // Lista temporal para objetivos activos

    private void Awake()
    {
        m_Camera = GetComponentInChildren<Camera>();
        if (m_Camera == null)
        {
            Debug.LogError("CameraControl: No se encontró un componente Camera en los hijos.");
            this.enabled = false; // Desactiva el script si no hay cámara
        }
        else if (!m_Camera.orthographic)
        {
            Debug.LogError("CameraControl: La cámara hija debe ser Ortográfica.");
            this.enabled = false;
        }
    }

    private void FixedUpdate()
    {
        // Actualiza la lista de objetivos activos
        UpdateActiveTargets();

        // Solo se mueve y hace zoom si hay objetivos activos
        if (m_ActiveTargets.Count > 0)
        {
            Move();
            Zoom();
        }
    }

    // Filtra los objetivos nulos o inactivos
    private void UpdateActiveTargets()
    {
        m_ActiveTargets.Clear();
        if (m_Targets == null) return;

        for (int i = 0; i < m_Targets.Length; i++)
        {
            if (m_Targets[i] != null && m_Targets[i].gameObject.activeSelf)
            {
                m_ActiveTargets.Add(m_Targets[i]);
            }
        }
    }

    private void Move()
    {
        FindAveragePosition();
        // Mueve la cámara suavemente hacia la posición deseada
        transform.position = Vector3.SmoothDamp(transform.position, m_DesiredPosition, ref m_MoveVelocity, m_DampTime);
    }

    // Calcula el punto medio entre todos los objetivos activos
    private void FindAveragePosition()
    {
        Vector3 averagePos = new Vector3();
        int numTargets = 0; // Usa el contador de la lista activa

        for (int i = 0; i < m_ActiveTargets.Count; i++)
        {
            averagePos += m_ActiveTargets[i].position;
            numTargets++;
        }

        if (numTargets > 0)
            averagePos /= numTargets;

        // Mantiene la altura Y actual del CameraRig
        averagePos.y = transform.position.y;
        m_DesiredPosition = averagePos;
    }

    private void Zoom()
    {
        // Calcula el tamaño ortográfico necesario para encuadrar a todos los objetivos
        float requiredSize = FindRequiredSize();
        // Cambia el tamaño de la cámara suavemente
        if (m_Camera != null) // Doble check por si acaso
        {
            m_Camera.orthographicSize = Mathf.SmoothDamp(m_Camera.orthographicSize, requiredSize, ref m_ZoomSpeed, m_DampTime);
        }
    }

    // Calcula el tamaño ortográfico necesario
    private float FindRequiredSize()
    {
        // Convierte la posición deseada al espacio local del CameraRig
        Vector3 desiredLocalPos = transform.InverseTransformPoint(m_DesiredPosition);
        float size = 0f;

        for (int i = 0; i < m_ActiveTargets.Count; i++)
        {
            // Convierte la posición del objetivo al espacio local del CameraRig
            Vector3 targetLocalPos = transform.InverseTransformPoint(m_ActiveTargets[i].position);
            // Calcula la diferencia de posición entre el objetivo y el centro deseado (en espacio local)
            Vector3 desiredPosToTarget = targetLocalPos - desiredLocalPos;

            // Comprueba cuánto tamaño se necesita verticalmente (eje Y local)
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.y));
            // Comprueba cuánto tamaño se necesita horizontalmente (eje X local), ajustado por el aspect ratio
            size = Mathf.Max(size, Mathf.Abs(desiredPosToTarget.x) / m_Camera.aspect);
        }

        // Añade el buffer de borde
        size += m_ScreenEdgeBuffer;
        // Asegura que el tamaño no sea menor que el mínimo
        size = Mathf.Max(size, m_MinSize);

        return size;
    }

    // Función llamada por GameManager para ajustar la cámara al inicio de la ronda
    public void SetStartPositionAndSize()
    {
        UpdateActiveTargets(); // Asegúrate de tener los objetivos correctos

        if (m_ActiveTargets.Count == 0) return; // No hacer nada si no hay objetivos

        FindAveragePosition(); // Calcula la posición central

        // Establece la posición y el tamaño directamente (sin suavizado)
        transform.position = m_DesiredPosition;
        if (m_Camera != null) m_Camera.orthographicSize = FindRequiredSize();
    }
}