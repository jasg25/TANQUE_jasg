using UnityEngine;

// Adaptado para Unity 6
public class ShellExplosion : MonoBehaviour
{
    public LayerMask m_TankMask; // Define qué capas son consideradas "tanques"
    public ParticleSystem m_ExplosionParticles; // Referencia al sistema de partículas HIJO de este objeto
    public AudioSource m_ExplosionAudio; // Referencia al AudioSource HIJO de este objeto
    public float m_MaxDamage = 100f; // Daño máximo en el epicentro
    public float m_ExplosionForce = 1000f; // Fuerza de la explosión aplicada a los tanques
    public float m_MaxLifeTime = 2f; // Tiempo en segundos antes de que el proyectil se autodestruya si no choca
    public float m_ExplosionRadius = 5f; // Radio de la explosión

    private bool m_Exploded = false; // Para asegurar que explote solo una vez

    private void Start()
    {
        // Destruye el proyectil después de m_MaxLifeTime si aún existe
        Destroy(gameObject, m_MaxLifeTime);

        // Comprobaciones iniciales
        if (m_ExplosionParticles == null) Debug.LogError("ShellExplosion: Falta asignar 'Explosion Particles' en el prefab Shell.");
        if (m_ExplosionAudio == null) Debug.LogWarning("ShellExplosion: Falta asignar 'Explosion Audio' en el prefab Shell. No habrá sonido de explosión.");

    }

    // Se llama cuando otro Collider entra en el Trigger de este proyectil
    private void OnTriggerEnter(Collider other)
    {
        // Si ya explotó, no hace nada más
        if (m_Exploded) return;

        // Busca todos los colliders dentro del radio de explosión que estén en la capa m_TankMask
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

        // Itera sobre todos los colliders encontrados
        for (int i = 0; i < colliders.Length; i++)
        {
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();
            // Si el objeto no tiene Rigidbody, pasa al siguiente
            if (!targetRigidbody) continue;

            // Añade la fuerza de la explosión al Rigidbody del tanque
            targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

            TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();
            // Si el objeto no tiene el script TankHealth, pasa al siguiente
            if (!targetHealth) continue;

            // Calcula el daño basado en la distancia
            float damage = CalculateDamage(targetRigidbody.position);
            // Aplica el daño al tanque
            targetHealth.TakeDamage(damage);
        }

        // --- Efectos de Explosión ---

        // Desvincula las partículas del proyectil para que no se destruyan con él
        if (m_ExplosionParticles != null)
        {
            m_ExplosionParticles.transform.parent = null;
            m_ExplosionParticles.Play(); // Inicia las partículas
                                         // Programa la destrucción del objeto de partículas después de que terminen
                                         // Usa particleSystem.main.duration para obtener la duración
            float duration = m_ExplosionParticles.main.duration + m_ExplosionParticles.main.startLifetime.constantMax; // Considera la duración y el tiempo de vida máximo
            Destroy(m_ExplosionParticles.gameObject, duration);
        }

        // Reproduce el sonido de explosión (si existe)
        if (m_ExplosionAudio != null)
        {
            // Si el audio está en un objeto diferente al de partículas, también hay que desvincularlo
            // Si está en el mismo objeto que las partículas, ya se desvinculó arriba.
            // Asumimos que está en el mismo objeto que las partículas:
            m_ExplosionAudio.Play();
        }

        m_Exploded = true; // Marca como explotado

        // Destruye el GameObject del proyectil inmediatamente
        Destroy(gameObject);
    }

    // Calcula el daño basado en la distancia del objetivo a la explosión
    private float CalculateDamage(Vector3 targetPosition)
    {
        Vector3 explosionToTarget = targetPosition - transform.position;
        float explosionDistance = explosionToTarget.magnitude;

        // Evita división por cero y daño si está fuera del radio
        if (m_ExplosionRadius <= 0f || explosionDistance > m_ExplosionRadius) return 0f;

        // Calcula qué tan cerca está el objetivo del epicentro (1 = epicentro, 0 = borde del radio)
        float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

        // Calcula el daño basado en la cercanía y el daño máximo
        float damage = relativeDistance * m_MaxDamage;

        // Asegura que el daño no sea negativo
        damage = Mathf.Max(0f, damage);

        return damage;
    }
}