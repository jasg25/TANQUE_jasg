using UnityEngine;
using UnityEngine.UI; // Necesario para Slider e Image

// Adaptado para Unity 6
public class TankHealth : MonoBehaviour
{
    public float m_StartingHealth = 100f;
    public Slider m_Slider; // Slider de UI para mostrar la vida
    public Image m_FillImage; // Imagen dentro del Slider que cambia de color
    public Color m_FullHealthColor = Color.green;
    public Color m_ZeroHealthColor = Color.red;
    public GameObject m_ExplosionPrefab; // Prefab de la explosión del tanque

    private AudioSource m_ExplosionAudio; // Audio de la explosión (se obtiene del prefab)
    private ParticleSystem m_ExplosionParticles; // Partículas de la explosión (se obtienen del prefab)
    private float m_CurrentHealth;
    private bool m_Dead; // Para asegurar que OnDeath() solo se llame una vez

    private void Awake()
    {
        // Instancia el prefab de explosión al inicio pero lo mantiene inactivo
        if (m_ExplosionPrefab != null)
        {
            GameObject explosionObject = Instantiate(m_ExplosionPrefab);
            m_ExplosionParticles = explosionObject.GetComponent<ParticleSystem>();
            m_ExplosionAudio = explosionObject.GetComponent<AudioSource>(); // Busca el AudioSource en el mismo objeto

            if (m_ExplosionParticles != null)
            {
                m_ExplosionParticles.gameObject.SetActive(false); // Desactiva el objeto de partículas
            }
            else
            {
                Debug.LogError("TankHealth: El 'ExplosionPrefab' asignado no tiene un componente ParticleSystem.");
            }
            // No es un error si no hay audio, solo una advertencia
            if (m_ExplosionAudio == null)
            {
                Debug.LogWarning("TankHealth: El 'ExplosionPrefab' asignado no tiene un componente AudioSource.");
            }
        }
        else
        {
            Debug.LogError("TankHealth: Falta asignar el 'ExplosionPrefab' en el Inspector.");
        }
    }

    private void OnEnable()
    {
        // Al (re)activar el tanque, resetea la vida y el estado 'muerto'
        m_CurrentHealth = m_StartingHealth;
        m_Dead = false;
        SetHealthUI(); // Actualiza la UI
    }

    // Función llamada por otros scripts (como ShellExplosion) para aplicar daño
    public void TakeDamage(float amount)
    {
        // Si ya está muerto, no hace nada
        if (m_Dead) return;

        m_CurrentHealth -= amount;
        SetHealthUI(); // Actualiza la UI

        // Si la vida llega a cero o menos y no estaba muerto antes
        if (m_CurrentHealth <= 0f)
        {
            OnDeath();
        }
    }

    private void SetHealthUI()
    {
        // Actualiza el valor del slider (si existe)
        if (m_Slider != null)
        {
            m_Slider.value = m_CurrentHealth;
        }

        // Interpola el color de la imagen de relleno (si existe)
        if (m_FillImage != null)
        {
            // Usa Lerp para cambiar suavemente entre rojo y verde según el porcentaje de vida
            m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
        }
    }

    private void OnDeath()
    {
        m_Dead = true; // Marca como muerto

        // Activa y reproduce los efectos de explosión (si existen)
        if (m_ExplosionParticles != null)
        {
            m_ExplosionParticles.transform.position = transform.position; // Mueve la explosión a la posición del tanque
            m_ExplosionParticles.gameObject.SetActive(true); // Activa el objeto
            m_ExplosionParticles.Play(); // Inicia las partículas

            if (m_ExplosionAudio != null)
            {
                m_ExplosionAudio.Play(); // Reproduce el sonido
            }
        }

        // Desactiva el GameObject del tanque
        gameObject.SetActive(false);
    }
}