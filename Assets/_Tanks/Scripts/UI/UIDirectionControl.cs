using UnityEngine;

// Adaptado para Unity 6 - Mantiene la UI orientada fija, cancelando la rotaci�n del padre
public class UIDirectionControl : MonoBehaviour
{
    public bool m_UseRelativeRotation = true; // Si es true, mantendr� la rotaci�n fija
    private Quaternion m_InitialRotation;     // Almacena la rotaci�n inicial en el mundo

    private void Start()
    {
        // Guarda la rotaci�n que tiene el objeto al empezar en coordenadas del mundo
        m_InitialRotation = transform.rotation;
    }

    // Usamos LateUpdate para asegurar que se ejecute despu�s de que el padre (tanque) se haya movido/rotado
    private void LateUpdate()
    {
        // Si la opci�n est� activada, fuerza la rotaci�n del objeto a ser la inicial
        if (m_UseRelativeRotation)
        {
            transform.rotation = m_InitialRotation;
        }
    }
}