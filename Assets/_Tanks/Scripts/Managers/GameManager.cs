using UnityEngine;
using System.Collections; // Necesario para Coroutines (IEnumerator)
using UnityEngine.SceneManagement; // Necesario para LoadScene
using UnityEngine.UI; // Necesario para Text (si usas UI Legacy)
// using TMPro; // Descomenta esta línea y cambia 'Text' por 'TMP_Text' si usas TextMeshPro

// Adaptado para Unity 6
public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;
    public float m_StartDelay = 3f;
    public float m_EndDelay = 3f;
    public CameraControl m_CameraControl; // Script que controla la cámara
    public Text m_MessageText; // Objeto Text de UI para mostrar mensajes
    // public TMP_Text m_MessageText; // Descomenta esta si usas TextMeshPro
    public GameObject m_TankPrefab; // El prefab del tanque que se va a instanciar
    public TankManager[] m_Tanks; // Array configurable en el Inspector con los datos de cada jugador

    private int m_RoundNumber;
    private WaitForSeconds m_StartWait; // Delay precalculado para inicio de ronda
    private WaitForSeconds m_EndWait; // Delay precalculado para fin de ronda
    private TankManager m_RoundWinner; // Quién ganó la ronda actual
    private TankManager m_GameWinner; // Quién ganó el juego completo

    private void Start()
    {
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        // --- Comprobaciones de Errores Comunes ---
        if (m_CameraControl == null) Debug.LogError("GameManager: Falta asignar 'Camera Control' en el Inspector.");
        if (m_MessageText == null) Debug.LogError("GameManager: Falta asignar 'Message Text' en el Inspector.");
        if (m_TankPrefab == null) Debug.LogError("GameManager: Falta asignar 'Tank Prefab' en el Inspector.");
        if (m_Tanks == null || m_Tanks.Length < 1) // Permitimos 1 jugador para pruebas, aunque el PDF es para 2
        {
            Debug.LogError("GameManager: El array 'Tanks' debe tener al menos 1 elemento configurado en el Inspector.");
            this.enabled = false; // Desactiva el GameManager si no hay tanques configurados
            return;
        }
        bool spawnPointsOk = true;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i].m_SpawnPoint == null)
            {
                Debug.LogError($"GameManager: Falta asignar 'Spawn Point' para el Tanque {i + 1} en el array 'Tanks'.");
                spawnPointsOk = false;
            }
        }
        if (!spawnPointsOk)
        {
            this.enabled = false; // Desactiva si faltan SpawnPoints
            return;
        }
        // --- Fin Comprobaciones ---

        SpawnAllTanks();
        SetCameraTargets();

        // Inicia el bucle principal del juego
        StartCoroutine(GameLoop());
    }

    private void SpawnAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // No instanciar si el prefab no está asignado
            if (m_TankPrefab == null) continue;

            // Instancia el tanque y guarda la referencia en el TankManager correspondiente
            m_Tanks[i].m_Instance = Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1; // Asigna número de jugador (1, 2, ...)
            m_Tanks[i].Setup(); // Llama a Setup para configurar colores, controles, etc.
        }
    }

    private void SetCameraTargets()
    {
        if (m_CameraControl == null) return; // Salir si no hay CameraControl

        // Cuenta cuántos tanques se instanciaron correctamente
        int validTankCount = 0;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i] != null && m_Tanks[i].m_Instance != null)
            {
                validTankCount++;
            }
        }

        // Crea el array de Transforms con el tamaño exacto de tanques válidos
        Transform[] targets = new Transform[validTankCount];
        int currentTargetIndex = 0;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i] != null && m_Tanks[i].m_Instance != null)
            {
                targets[currentTargetIndex] = m_Tanks[i].m_Instance.transform;
                currentTargetIndex++;
            }
        }

        // Asigna los objetivos a la cámara
        m_CameraControl.m_Targets = targets;
    }

    // Corutina principal que cicla a través de las fases de la ronda
    private IEnumerator GameLoop()
    {
        yield return StartCoroutine(RoundStarting());
        yield return StartCoroutine(RoundPlaying());
        yield return StartCoroutine(RoundEnding());

        // Si se determinó un ganador del juego en RoundEnding
        if (m_GameWinner != null)
        {
            // Reinicia la escena actual
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else // Si no hay ganador del juego, empieza la siguiente ronda
        {
            StartCoroutine(GameLoop());
        }
    }

    // Corutina para la fase de inicio de ronda
    private IEnumerator RoundStarting()
    {
        ResetAllTanks(); // Coloca los tanques en sus SpawnPoints
        DisableTankControl(); // Impide que se muevan

        // Ajusta la cámara instantáneamente a la nueva posición/zoom
        if (m_CameraControl != null) m_CameraControl.SetStartPositionAndSize();

        m_RoundNumber++;
        if (m_MessageText != null) m_MessageText.text = "ROUND " + m_RoundNumber;

        // Espera el delay inicial antes de empezar a jugar
        yield return m_StartWait;
    }

    // Corutina para la fase de juego activo
    private IEnumerator RoundPlaying()
    {
        EnableTankControl(); // Permite mover los tanques

        if (m_MessageText != null) m_MessageText.text = string.Empty; // Borra el mensaje

        // Bucle que se ejecuta cada frame mientras quede más de un tanque
        while (!OneTankLeft())
        {
            yield return null; // Espera al siguiente frame
        }
    }

    // Corutina para la fase de fin de ronda
    private IEnumerator RoundEnding()
    {
        DisableTankControl(); // Impide mover los tanques

        m_RoundWinner = null; // Resetea el ganador de la ronda anterior
        m_RoundWinner = GetRoundWinner(); // Determina el ganador de esta ronda

        if (m_RoundWinner != null)
        {
            m_RoundWinner.m_Wins++; // Incrementa las victorias del ganador
        }

        m_GameWinner = GetGameWinner(); // Comprueba si alguien ganó el juego

        // Genera y muestra el mensaje final
        string message = EndMessage();
        if (m_MessageText != null) m_MessageText.text = message;

        // Espera el delay final antes de pasar a la siguiente ronda o reiniciar
        yield return m_EndWait;
    }

    // Comprueba si queda 1 o 0 tanques activos
    private bool OneTankLeft()
    {
        int numTanksLeft = 0;
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i] != null && m_Tanks[i].m_Instance != null && m_Tanks[i].m_Instance.activeSelf)
            {
                numTanksLeft++;
            }
        }
        return numTanksLeft <= 1;
    }

    // Devuelve el TankManager del único tanque activo (o null si hay empate/error)
    private TankManager GetRoundWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i] != null && m_Tanks[i].m_Instance != null && m_Tanks[i].m_Instance.activeSelf)
            {
                return m_Tanks[i];
            }
        }
        return null; // Empate o error
    }

    // Devuelve el TankManager que ha ganado el juego (o null si nadie ha ganado)
    private TankManager GetGameWinner()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i] != null && m_Tanks[i].m_Wins == m_NumRoundsToWin)
            {
                return m_Tanks[i];
            }
        }
        return null;
    }

    // Genera el mensaje de fin de ronda/juego
    private string EndMessage()
    {
        string message = "EMPATE!"; // Mensaje por defecto

        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " GANA LA RONDA!";

        message += "\n\n\n";

        for (int i = 0; i < m_Tanks.Length; i++)
        {
            // Añade la puntuación solo si el TankManager existe
            if (m_Tanks[i] != null)
            {
                // Usa ?? "PLAYER X" por si m_ColoredPlayerText es nulo
                message += (m_Tanks[i].m_ColoredPlayerText ?? $"PLAYER {i + 1}") + ": " + m_Tanks[i].m_Wins + " VICTORIAS\n";
            }
        }

        if (m_GameWinner != null)
            message = (m_GameWinner.m_ColoredPlayerText ?? $"PLAYER {m_GameWinner.m_PlayerNumber}") + " GANA EL JUEGO!";

        return message;
    }

    // Llama a Reset() en cada TankManager
    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i] != null && m_Tanks[i].m_Instance != null)
            {
                m_Tanks[i].Reset();
            }
        }
    }

    // Llama a EnableControl() en cada TankManager
    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i] != null && m_Tanks[i].m_Instance != null)
            {
                m_Tanks[i].EnableControl();
            }
        }
    }

    // Llama a DisableControl() en cada TankManager
    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            if (m_Tanks[i] != null && m_Tanks[i].m_Instance != null)
            {
                m_Tanks[i].DisableControl();
            }
        }
    }
}