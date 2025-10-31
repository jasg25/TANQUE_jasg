using UnityEngine;

// Adaptado para Unity 6 - Mantiene la UI orientada fija, cancelando la rotación del padre
public class UIDirectionControl : MonoBehaviour
{
    public bool m_UseRelativeRotation = true; // Si es true, mantendrá la rotación fija
    private Quaternion m_InitialRotation;     // Almacena la rotación inicial en el mundo

    private void Start()
    {
        // Guarda la rotación que tiene el objeto al empezar en coordenadas del mundo
        m_InitialRotation = transform.rotation;
    }

    // Usamos LateUpdate para asegurar que se ejecute después de que el padre (tanque) se haya movido/rotado
    private void LateUpdate()
    {
        // Si la opción está activada, fuerza la rotación del objeto a ser la inicial
        if (m_UseRelativeRotation)
        {
            transform.rotation = m_InitialRotation;
        }
    }
}