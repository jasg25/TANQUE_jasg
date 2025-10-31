using UnityEngine;

// Adaptado para Unity 6 - Mantiene el Input Manager antiguo como el PDF
public class TankMovement : MonoBehaviour
{
    public int m_PlayerNumber = 1;
    public float m_Speed = 12f;
    public float m_TurnSpeed = 180f;
    public AudioSource m_MovementAudio;
    public AudioClip m_EngineIdling;
    public AudioClip m_EngineDriving;
    public float m_PitchRange = 0.2f;

    private string m_MovementAxisName;
    private string m_TurnAxisName;
    private Rigidbody m_Rigidbody;
    private float m_MovementInputValue;
    private float m_TurnInputValue;
    private float m_OriginalPitch;
    private bool m_HasRigidbody = false; // Para evitar errores si no hay Rigidbody

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_HasRigidbody = m_Rigidbody != null; // Comprueba si el Rigidbody existe
        if (!m_HasRigidbody)
        {
            Debug.LogError($"TankMovement: Rigidbody no encontrado en el tanque del jugador {m_PlayerNumber}. El movimiento no funcionará.");
        }
    }

    private void OnEnable()
    {
        if (m_HasRigidbody) m_Rigidbody.isKinematic = false;
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;
    }

    private void OnDisable()
    {
        if (m_HasRigidbody) m_Rigidbody.isKinematic = true;
    }

    private void Start()
    {
        // Configura los ejes de input basados en el número de jugador (del Input Manager antiguo)
        m_MovementAxisName = "Vertical" + m_PlayerNumber;
        m_TurnAxisName = "Horizontal" + m_PlayerNumber;

        if (m_MovementAudio != null)
        {
            m_OriginalPitch = m_MovementAudio.pitch;
        }
        else
        {
            Debug.LogWarning($"TankMovement: AudioSource 'Movement Audio' no asignado en el tanque del jugador {m_PlayerNumber}. No habrá sonido de motor.");
        }
    }

    private void Update()
    {
        // Lee la entrada del Input Manager antiguo
        m_MovementInputValue = Input.GetAxis(m_MovementAxisName);
        m_TurnInputValue = Input.GetAxis(m_TurnAxisName);

        EngineAudio();
    }

    private void EngineAudio()
    {
        // Solo gestiona el audio si hay un AudioSource y clips asignados
        if (m_MovementAudio == null || m_EngineIdling == null || m_EngineDriving == null) return;

        // Si el tanque se está moviendo o girando
        if (Mathf.Abs(m_MovementInputValue) > 0.1f || Mathf.Abs(m_TurnInputValue) > 0.1f)
        {
            // Si el clip actual no es el de moverse, cámbialo y reprodúcelo
            if (m_MovementAudio.clip != m_EngineDriving)
            {
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
        else // Si el tanque está quieto
        {
            // Si el clip actual no es el de estar parado, cámbialo y reprodúcelo
            if (m_MovementAudio.clip != m_EngineIdling)
            {
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
    }

    private void FixedUpdate()
    {
        // Solo intenta mover si existe el Rigidbody
        if (!m_HasRigidbody) return;

        Move();
        Turn();
    }

    private void Move()
    {
        // Calcula el vector de movimiento basado en la entrada, velocidad y tiempo
        Vector3 movement = transform.forward * m_MovementInputValue * m_Speed * Time.fixedDeltaTime; // Usa fixedDeltaTime en FixedUpdate
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }

    private void Turn()
    {
        // Calcula el ángulo de giro basado en la entrada, velocidad de giro y tiempo
        float turn = m_TurnInputValue * m_TurnSpeed * Time.fixedDeltaTime; // Usa fixedDeltaTime en FixedUpdate
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }
}