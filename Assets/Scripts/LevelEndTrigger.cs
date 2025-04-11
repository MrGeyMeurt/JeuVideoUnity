using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    [SerializeField] private BoxCollider triggerCollider;
    [SerializeField] private bool isFinalLevel = false;  
    [SerializeField] private EndScreenManager endScreenManager;
    
    private void Start()
    {
        // Verify if the triggerCollider is assigned and is a trigger
        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            Debug.LogWarning("Le collider assigné n'est pas configuré comme trigger!");
        }
        
        // If this is the final level, find the EndScreenManager in the scene
        if (isFinalLevel && endScreenManager == null)
        {
            endScreenManager = FindObjectOfType<EndScreenManager>();
            if (endScreenManager == null)
            {
                Debug.LogWarning("EndScreenManager non trouvé! Assurez-vous qu'il existe dans la scène si c'est le niveau final.");
            }
        }
    }
   
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {  
            if (isFinalLevel && endScreenManager != null)
            {
                endScreenManager.ShowEndScreen();
            }
            else
            {
                LevelManager.Instance.LoadNextLevel();
            }
        }
    }
}