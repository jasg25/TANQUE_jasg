using UnityEngine;
using UnityEngine.UI; // Necesario para Slider

// Adaptado para Unity 6 - Mantiene el Input Manager antiguo como el PDF
public class TankShooting : MonoBehaviour
{
    public int m_PlayerNumber = 1;
    public Rigidbody m_Shell; // Prefab de la bomba (debe tener Rigidbody)
    public Transform m_FireTransform; // Punto de origen del disparo
    public Slider m_AimSlider; // Barra de UI para mostrar la carga
    public AudioSource m_ShootingAudio; // AudioSource para sonidos de disparo
    public AudioClip m_ChargingClip; // Sonido al cargar
    public AudioClip m_FireClip; // Sonido al disparar
    public float m_MinLaunchForce = 15f;
    public float m_MaxLaunchForce = 30f;
    public float m_MaxChargeTime = 0.75f; // Tiempo para alcanzar la carga máxima

    private string m_FireButton;
    private float m_CurrentLaunchForce;
    private float m_ChargeSpeed;
    private bool m_Fired; // Para asegurar que solo se dispare una vez por pulsación
    private bool m_Charging = false; // Para controlar el sonido de carga

    private void OnEnable()
    {
        m_CurrentLaunchForce = m_MinLaunchForce;
        if (m_AimSlider != null) m_AimSlider.value = m_MinLaunchForce;
        m_Charging = false;
        m_Fired = false; // Resetea el estado de disparo al activar
    }

    private void Start()
    {
        // Configura el botón de disparo (del Input Manager antiguo)
        m_FireButton = "Fire" + m_PlayerNumber;
        // Calcula cuánto aumenta la fuerza por segundo de carga
        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;

        // Comprobaciones iniciales
        if (m_Shell == null) Debug.LogError($"TankShooting: Prefab 'Shell' no asignado para jugador {m_PlayerNumber}.");
        if (m_FireTransform == null) Debug.LogError($"TankShooting: 'Fire Transform' no asignado para jugador {m_PlayerNumber}.");
        if (m_AimSlider == null) Debug.LogWarning($"TankShooting: 'Aim Slider' no asignado para jugador {m_PlayerNumber}. No se mostrará la carga.");
        if (m_ShootingAudio == null) Debug.LogWarning($"TankShooting: 'Shooting Audio' no asignado para jugador {m_PlayerNumber}. No habrá sonidos de disparo.");
    }

    private void Update()
    {
        // Si no está cargando, resetea el slider visualmente (si existe)
        if (!m_Charging && m_AimSlider != null)
        {
            m_AimSlider.value = m_MinLaunchForce;
        }

        // Si se alcanza la fuerza máxima mientras se carga y aún no se ha disparado
        if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired && m_Charging)
        {
            m_CurrentLaunchForce = m_MaxLaunchForce;
            Fire();
        }
        // Si se presiona el botón de disparo por primera vez
        else if (Input.GetButtonDown(m_FireButton))
        {
            m_Fired = false; // Permite un nuevo disparo
            m_CurrentLaunchForce = m_MinLaunchForce; // Empieza a cargar desde mínimo
            m_Charging = true; // Empieza a cargar

            // Reproduce el sonido de carga (si existe)
            if (m_ShootingAudio != null && m_ChargingClip != null)
            {
                m_ShootingAudio.clip = m_ChargingClip;
                m_ShootingAudio.Play();
            }
        }
        // Si se mantiene presionado el botón, no se ha disparado aún y se está cargando
        else if (Input.GetButton(m_FireButton) && !m_Fired && m_Charging)
        {
            // Incrementa la fuerza de lanzamiento con el tiempo
            m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;
            // Actualiza el slider (si existe)
            if (m_AimSlider != null) m_AimSlider.value = m_CurrentLaunchForce;
        }
        // Si se suelta el botón de disparo, aún no se ha disparado y se estaba cargando
        else if (Input.GetButtonUp(m_FireButton) && !m_Fired && m_Charging)
        {
            Fire(); // Dispara con la fuerza acumulada
        }
    }

    private void Fire()
    {
        m_Fired = true; // Marca como disparado
        m_Charging = false; // Deja de cargar

        // Verifica si el prefab y el punto de disparo están asignados
        if (m_Shell == null || m_FireTransform == null) return;

        // Crea una instancia de la bomba en la posición y rotación del FireTransform
        Rigidbody shellInstance = Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

        // Lanza la bomba con la fuerza actual en la dirección del FireTransform
        if (shellInstance != null)
        {
            shellInstance.linearVelocity = m_CurrentLaunchForce * m_FireTransform.forward;
        }
        else
        {
            Debug.LogError($"TankShooting: El prefab 'Shell' no tiene componente Rigidbody para jugador {m_PlayerNumber}.");
            return; // No se puede aplicar velocidad si no hay Rigidbody
        }


        // Reproduce el sonido de disparo (si existe)
        if (m_ShootingAudio != null && m_FireClip != null)
        {
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play();
        }

        // Resetea la fuerza de lanzamiento para el próximo disparo (por si acaso)
        m_CurrentLaunchForce = m_MinLaunchForce;
        // Resetea el slider visualmente (si existe)
        if (m_AimSlider != null) m_AimSlider.value = m_MinLaunchForce;
    }
}