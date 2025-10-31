using System; // Necesario para [Serializable]
using UnityEngine;

// Adaptado para Unity 6 - Define la configuración de cada tanque
[Serializable] // Permite editar instancias de esta clase en el Inspector (dentro del array de GameManager)
public class TankManager
{
    // --- Variables públicas configurables en el Inspector de GameManager ---
    public Color m_PlayerColor = Color.grey; // Color por defecto si no se asigna
    public Transform m_SpawnPoint; // Punto de aparición asignado en el Inspector

    // --- Variables gestionadas internamente por GameManager y Setup() ---
    [HideInInspector] public int m_PlayerNumber;
    [HideInInspector] public string m_ColoredPlayerText; // Texto con formato de color
    [HideInInspector] public GameObject m_Instance; // La instancia del tanque creada
    [HideInInspector] public int m_Wins = 0; // Contador de victorias

    // --- Referencias a componentes del tanque (se obtienen en Setup) ---
    private TankMovement m_Movement;
    private TankShooting m_Shooting;
    private GameObject m_CanvasGameObject; // El objeto Canvas hijo del tanque

    // Llamado por GameManager después de instanciar el tanque
    public void Setup()
    {
        // Verifica si la instancia del tanque existe
        if (m_Instance == null)
        {
            Debug.LogError($"TankManager.Setup: La instancia del tanque para el jugador {m_PlayerNumber} es nula.");
            return;
        }

        // Obtiene los scripts principales del GameObject instanciado
        m_Movement = m_Instance.GetComponent<TankMovement>();
        m_Shooting = m_Instance.GetComponent<TankShooting>();

        // Busca el Canvas *hijo* del tanque
        Canvas canvas = m_Instance.GetComponentInChildren<Canvas>();
        if (canvas != null)
            m_CanvasGameObject = canvas.gameObject;
        else
            Debug.LogWarning($"TankManager.Setup: No se encontró Canvas hijo para el tanque {m_PlayerNumber}. La UI de vida/mira no se activará/desactivará.");

        // Configura el número de jugador en los scripts correspondientes (si existen)
        if (m_Movement != null)
            m_Movement.m_PlayerNumber = m_PlayerNumber;
        else
            Debug.LogError($"TankManager.Setup: El prefab del tanque no tiene el script TankMovement para el jugador {m_PlayerNumber}.");

        if (m_Shooting != null)
            m_Shooting.m_PlayerNumber = m_PlayerNumber;
        else
            Debug.LogError($"TankManager.Setup: El prefab del tanque no tiene el script TankShooting para el jugador {m_PlayerNumber}.");

        // Crea el texto formateado (ej: "<color=#FF0000FF>PLAYER 2</color>")
        m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

        // Aplica el color del jugador a los materiales del tanque
        MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in renderers)
        {
            // Podría haber varios materiales, pero por simplicidad aplicamos al primero
            if (renderer.material != null)
            {
                renderer.material.color = m_PlayerColor;
            }
        }
    }

    // Desactiva el control del tanque y su UI
    public void DisableControl()
    {
        if (m_Movement != null) m_Movement.enabled = false;
        if (m_Shooting != null) m_Shooting.enabled = false;
        if (m_CanvasGameObject != null) m_CanvasGameObject.SetActive(false);
    }

    // Activa el control del tanque y su UI
    public void EnableControl()
    {
        if (m_Movement != null) m_Movement.enabled = true;
        if (m_Shooting != null) m_Shooting.enabled = true;
        if (m_CanvasGameObject != null) m_CanvasGameObject.SetActive(true);
    }

    // Resetea el tanque a su estado inicial en el SpawnPoint
    public void Reset()
    {
        // Verifica que el SpawnPoint esté asignado
        if (m_SpawnPoint == null)
        {
            Debug.LogError($"TankManager.Reset: SpawnPoint no asignado para el jugador {m_PlayerNumber}.");
            return;
        }
        // Verifica que la instancia del tanque exista
        if (m_Instance == null)
        {
            Debug.LogError($"TankManager.Reset: La instancia del tanque para el jugador {m_PlayerNumber} es nula.");
            return;
        }

        // Mueve el tanque al SpawnPoint
        m_Instance.transform.position = m_SpawnPoint.position;
        m_Instance.transform.rotation = m_SpawnPoint.rotation;

        // Desactiva y reactiva para forzar el llamado a OnEnable() en los scripts (como TankHealth)
        m_Instance.SetActive(false);
        m_Instance.SetActive(true);
    }
}