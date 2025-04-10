using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    [SerializeField] private BoxCollider triggerCollider; // Référence au collider avec trigger
    
    private void Start()
    {
        // Vérifier que le collider est bien un trigger
        if (triggerCollider != null && !triggerCollider.isTrigger)
        {
            Debug.LogWarning("Le collider assigné n'est pas configuré comme trigger!");
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Vérifier que la collision se produit avec le bon collider
        if (other.CompareTag("Player"))
        {  
            // Le joueur entre dans la zone de fin => on passe au niveau suivant
            Debug.Log("Plateforme d'arrivée atteinte !");
            LevelManager.Instance.LoadNextLevel();
        }
    }
}