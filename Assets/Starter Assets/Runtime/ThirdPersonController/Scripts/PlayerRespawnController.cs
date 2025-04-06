using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; 


public class PlayerRespawnController : MonoBehaviour
{
    [SerializeField] float m_Threshold = -10.0f;
    Vector3 m_Origin;
    CharacterController m_CharacterController;
    
    private void Awake()
    {
        m_Origin = transform.position;
        m_CharacterController = GetComponent<CharacterController>();
    }
    
    private void Update()
    {
        if (m_CharacterController.transform.position.y < m_Threshold)
        {
            Respawn();
        }
    }
    
    public void OnRespawn(InputValue value)
    {
        Respawn();
    }
    
    private void Respawn()
    {
        m_CharacterController.enabled = false;
        transform.position = m_Origin;
        m_CharacterController.enabled = true;
    }
}
