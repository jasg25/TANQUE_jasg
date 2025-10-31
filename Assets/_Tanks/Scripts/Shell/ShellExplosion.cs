using UnityEngine;

// Adaptado para Unity 6
public class ShellExplosion : MonoBehaviour
{
    public LayerMask m_TankMask; // Define qu� capas son consideradas "tanques"
    public ParticleSystem m_ExplosionParticles; // Referencia al sistema de part�culas HIJO de este objeto
    public AudioSource m_ExplosionAudio; // Referencia al AudioSource HIJO de este objeto
    public float m_MaxDamage = 100f; // Da�o m�ximo en el epicentro
    public float m_ExplosionForce = 1000f; // Fuerza de la explosi�n aplicada a los tanques
    public float m_MaxLifeTime = 2f; // Tiempo en segundos antes de que el proyectil se autodestruya si no choca
    public float m_ExplosionRadius = 5f; // Radio de la explosi�n

    private bool m_Exploded = false; // Para asegurar que explote solo una vez

    private void Start()
    {
        // Destruye el proyectil despu�s de m_MaxLifeTime si a�n existe
        Destroy(gameObject, m_MaxLifeTime);

        // Comprobaciones iniciales
        if (m_ExplosionParticles == null) Debug.LogError("ShellExplosion: Falta asignar 'Explosion Particles' en el prefab Shell.");
        if (m_ExplosionAudio == null) Debug.LogWarning("ShellExplosion: Falta asignar 'Explosion Audio' en el prefab Shell. No habr� sonido de explosi�n.");

    }

    // Se llama cuando otro Collider entra en el Trigger de este proyectil
    private void OnTriggerEnter(Collider other)
    {
        // Si ya explot�, no hace nada m�s
        if (m_Exploded) return;

        // Busca todos los colliders dentro del radio de explosi�n que est�n en la capa m_TankMask
        Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

        // Itera sobre todos los colliders encontrados
        for (int i = 0; i < colliders.Length; i++)
        {
            Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();
            // Si el objeto no tiene Rigidbody, pasa al siguiente
            if (!targetRigidbody) continue;

            // A�ade la fuerza de la explosi�n al Rigidbody del tanque
            targetRigidbody.AddExplosionForce(m_ExplosionForce, transform.position, m_ExplosionRadius);

            TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();
            // Si el objeto no tiene el script TankHealth, pasa al siguiente
            if (!targetHealth) continue;

            // Calcula el da�o basado en la distancia
            float damage = CalculateDamage(targetRigidbody.position);
            // Aplica el da�o al tanque
            targetHealth.TakeDamage(damage);
        }

        // --- Efectos de Explosi�n ---

        // Desvincula las part�culas del proyectil para que no se destruyan con �l
        if (m_ExplosionParticles != null)
        {
            m_ExplosionParticles.transform.parent = null;
            m_ExplosionParticles.Play(); // Inicia las part�culas
                                         // Programa la destrucci�n del objeto de part�culas despu�s de que terminen
                                         // Usa particleSystem.main.duration para obtener la duraci�n
            float duration = m_ExplosionParticles.main.duration + m_ExplosionParticles.main.startLifetime.constantMax; // Considera la duraci�n y el tiempo de vida m�ximo
            Destroy(m_ExplosionParticles.gameObject, duration);
        }

        // Reproduce el sonido de explosi�n (si existe)
        if (m_ExplosionAudio != null)
        {
            // Si el audio est� en un objeto diferente al de part�culas, tambi�n hay que desvincularlo
            // Si est� en el mismo objeto que las part�culas, ya se desvincul� arriba.
            // Asumimos que est� en el mismo objeto que las part�culas:
            m_ExplosionAudio.Play();
        }

        m_Exploded = true; // Marca como explotado

        // Destruye el GameObject del proyectil inmediatamente
        Destroy(gameObject);
    }

    // Calcula el da�o basado en la distancia del objetivo a la explosi�n
    private float CalculateDamage(Vector3 targetPosition)
    {
        Vector3 explosionToTarget = targetPosition - transform.position;
        float explosionDistance = explosionToTarget.magnitude;

        // Evita divisi�n por cero y da�o si est� fuera del radio
        if (m_ExplosionRadius <= 0f || explosionDistance > m_ExplosionRadius) return 0f;

        // Calcula qu� tan cerca est� el objetivo del epicentro (1 = epicentro, 0 = borde del radio)
        float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;

        // Calcula el da�o basado en la cercan�a y el da�o m�ximo
        float damage = relativeDistance * m_MaxDamage;

        // Asegura que el da�o no sea negativo
        damage = Mathf.Max(0f, damage);

        return damage;
    }
}